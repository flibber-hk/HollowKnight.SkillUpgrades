using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using SkillUpgrades.Util;
using UnityEngine.SceneManagement;

namespace SkillUpgrades.Skills
{
    internal class VerticalSuperdash : AbstractSkillUpgrade
    {
        [SerializeToSetting]
        public static bool DiagonalSuperdash = true;

        public override string Name => "Vertical Cdash";
        public override string Description => "Toggle whether Crystal Heart can be used in non-horizontal directions";

        public override bool InvolvesHeroRotation => true;

        public override void Initialize()
        {
            On.CameraTarget.Update += FixVerticalCamera;
            On.GameManager.FinishedEnteringScene += DisableUpwardOneways;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ResetSuperdashState;
            On.HeroController.Start += ModifySuperdashFsm;
        }


        internal enum SuperdashDirection
        {
            Normal = 0,     // Anything not caused by this mod
            Upward,
            Diagonal
        }

        private static SuperdashDirection _queuedSuperdashState = SuperdashDirection.Normal;
        private static SuperdashDirection _superdashState = SuperdashDirection.Normal;
        internal static SuperdashDirection SuperdashState
        {
            get => _superdashState;

            set
            {
                if (value == SuperdashDirection.Upward && _superdashState == SuperdashDirection.Normal)
                {
                    HeroController.instance.RotateHero(-90);
                }
                else if (value == SuperdashDirection.Diagonal && _superdashState == SuperdashDirection.Normal)
                {
                    HeroController.instance.RotateHero(-45);
                }
                else if (value == SuperdashDirection.Normal)
                {
                    // We need to set the SD Burst inactive before un-rotating the hero,
                    // so it doesn't rotate with it
                    if (GameObject.Find("SD Burst") is GameObject burst)
                    {
                        burst.transform.parent = HeroController.instance.gameObject.transform;
                        burst.SetActive(false);
                    }
                    HeroRotation.ResetHero();
                }

                _superdashState = value;
                _queuedSuperdashState = SuperdashDirection.Normal;
            }
        }




        private void FixVerticalCamera(On.CameraTarget.orig_Update orig, CameraTarget self)
        {
            orig(self);

            if (self.hero_ctrl != null && GameManager.instance.IsGameplayScene())
            {
                if (!self.superDashing) return;
                
                if (SuperdashState == SuperdashDirection.Upward)     // if vertical cdash
                {
                    self.cameraCtrl.lookOffset += Math.Abs(self.dashOffset);
                    self.dashOffset = 0;
                }
                else if (SuperdashState == SuperdashDirection.Diagonal)
                {
                    self.cameraCtrl.lookOffset += Math.Abs(self.dashOffset) * ((float)Math.Sqrt(2) / 2f);
                    self.dashOffset *= (float)Math.Sqrt(2) / 2f;
                }
            }
        }
        // Deactivate upward oneway transitions after spawning in so the player doesn't accidentally
        // softlock by vc-ing into them
        private void DisableUpwardOneways(On.GameManager.orig_FinishedEnteringScene orig, GameManager self)
        {
            orig(self);

            switch (self.sceneName)
            {
                // The KP top transition is the only one that needs to be disabled; the others have collision
                case "Tutorial_01":
                    if (GameObject.Find("top1") is GameObject topTransition)
                        topTransition.SetActive(false);
                    break;
            }
        }
        private void ResetSuperdashState(Scene arg0, Scene arg1)
        {
            SuperdashState = SuperdashDirection.Normal;
        }

        private void ModifySuperdashFsm(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            PlayMakerFSM fsm = self.superDash;

            #region Add FSM Variables
            FsmFloat vSpeed = fsm.AddFsmFloat("V Speed VC");
            FsmFloat hSpeed = fsm.AddFsmFloat("H Speed VC");
            #endregion

            #region Set Direction
            fsm.GetState("Direction").AddFirstAction(new ExecuteLambda(() =>
            {
                bool shouldDiagonal = false;
                bool shouldVertical = false;
                if (DiagonalSuperdash && skillUpgradeActive)
                {
                    if (GameManager.instance.inputHandler.inputActions.up.IsPressed)
                    {
                        if (GameManager.instance.inputHandler.inputActions.right.IsPressed && HeroController.instance.cState.facingRight)
                        {
                            shouldDiagonal = true;
                        }
                        else if (GameManager.instance.inputHandler.inputActions.left.IsPressed && !HeroController.instance.cState.facingRight)
                        {
                            shouldDiagonal = true;
                        }
                    }
                }
                if (skillUpgradeActive && !shouldDiagonal)
                {
                    if (GameManager.instance.inputHandler.inputActions.up.IsPressed)
                    {
                        shouldVertical = true;
                    }
                }

                if (shouldDiagonal)
                {
                    _queuedSuperdashState = SuperdashDirection.Diagonal;
                }
                else if (shouldVertical)
                {
                    _queuedSuperdashState = SuperdashDirection.Upward;
                }
            }));

            fsm.GetState("Direction Wall").AddFirstAction(new ExecuteLambda(() =>
            {
                if (DiagonalSuperdash && skillUpgradeActive && GameManager.instance.inputHandler.inputActions.up.IsPressed)
                {
                    _queuedSuperdashState = SuperdashDirection.Diagonal;
                }
            }));

            fsm.GetState("Left").AddAction(new ExecuteLambda(() =>
            {
                SuperdashState = _queuedSuperdashState;
            })); 
            fsm.GetState("Right").AddAction(new ExecuteLambda(() =>
            {
                SuperdashState = _queuedSuperdashState;
            }));
            #endregion

            #region Modify Dashing and Cancelable states
            FsmState dashing = fsm.GetState("Dashing");
            ExecuteLambda setVelocityVariables = new ExecuteLambda(() =>
            {
                float velComponent = Math.Abs(fsm.FsmVariables.GetFsmFloat("Current SD Speed").Value);
                switch (SuperdashState)
                {
                    case SuperdashDirection.Diagonal:
                        velComponent *= (float)(Math.Sqrt(2) / 2f);
                        vSpeed.Value = velComponent;
                        hSpeed.Value = velComponent * (HeroController.instance.cState.facingRight ? 1 : -1);
                        break;
                    case SuperdashDirection.Upward:
                        vSpeed.Value = velComponent;
                        hSpeed.Value = 0f;
                        break;
                    default:
                    case SuperdashDirection.Normal:
                        vSpeed.Value = 0f;
                        hSpeed.Value = velComponent * (HeroController.instance.cState.facingRight ? 1 : -1);
                        break;
                }
            });

            SetVelocity2d setVel = dashing.GetActionOfType<SetVelocity2d>();
            setVel.x = hSpeed;
            setVel.y = vSpeed;

            DecideToStopSuperdash decideToStop = new DecideToStopSuperdash(hSpeed, vSpeed, fsm.FsmVariables.GetFsmBool("Zero Last Frame"));

            dashing.Actions = new FsmStateAction[]
            {
                    setVelocityVariables,
                    dashing.Actions[0],
                    dashing.Actions[1],
                    setVel,
                    dashing.Actions[3],
                    dashing.Actions[4],
                    decideToStop,
                    dashing.Actions[7],
                    dashing.Actions[8],
            };

            FsmState cancelable = fsm.GetState("Cancelable");
            cancelable.Actions = new FsmStateAction[]
            {
                    cancelable.Actions[0],
                    setVel,
                    cancelable.Actions[2],
                    decideToStop,
                    cancelable.Actions[5],
                    cancelable.Actions[6],
            };
            #endregion

            #region Reset Vertical Charge variable
            fsm.GetState("Air Cancel").AddFirstAction(new ExecuteLambda(() =>
            {
                SuperdashState = SuperdashDirection.Normal;
            }));
            fsm.GetState("Cancel").AddFirstAction(new ExecuteLambda(() =>
            {
                SuperdashState = SuperdashDirection.Normal;
            }));
            fsm.GetState("Hit Wall").AddFirstAction(new ExecuteLambda(() =>
            {
                SuperdashState = SuperdashDirection.Normal;
            }));
            #endregion
        }
    }
}
