using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using SkillUpgrades.Util;
using UnityEngine.SceneManagement;

namespace SkillUpgrades.Skills
{
    internal class VerticalSuperdash : AbstractSkillUpgrade
    {
        public bool DiagonalSuperdash => GetBool(true);
        public bool BreakDiveFloorsFromBelow => GetBool(false);

        public override string UIName => "Vertical Superdash";
        public override string Description => "Toggle whether Crystal Heart can be used in non-horizontal directions";

        public override bool InvolvesHeroRotation => true;

        public override void Initialize()
        {
            On.CameraTarget.Update += FixVerticalCamera;
            On.GameManager.FinishedEnteringScene += DisableUpwardOneways;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ResetSuperdashState;
            On.HeroController.Start += ModifySuperdashFsm;
        }

        /// <summary>
        /// The angle the knight is superdashing, measured anticlockwise when the knight is facing left and clockwise when facing right
        /// </summary>
        internal float SuperdashAngle { get; set; } = 0f;

        internal void ResetSuperdashAngle()
        {
            if (SuperdashAngle == 0f) return;

            if (GameObject.Find("SD Burst") is GameObject burst)
            {
                burst.transform.parent = HeroController.instance.gameObject.transform;
                burst.SetActive(false);
            }

            SuperdashAngle = 0f;
            HeroRotation.ResetHero();
            if (BreakDiveFloorsFromBelow) PlayMakerFSM.BroadcastEvent("QUAKE FALL END");
        }


        private void FixVerticalCamera(On.CameraTarget.orig_Update orig, CameraTarget self)
        {
            orig(self);
            if (self.hero_ctrl == null || !GameManager.instance.IsGameplayScene()) return;
            if (!self.superDashing) return;

            self.cameraCtrl.lookOffset += Math.Abs(self.dashOffset) * Mathf.Sin(SuperdashAngle * Mathf.PI / 180);
            self.dashOffset *= Mathf.Cos(SuperdashAngle * Mathf.PI / 180);
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
            ResetSuperdashAngle();
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
                    SuperdashAngle = -45;
                }
                else if (shouldVertical)
                {
                    SuperdashAngle = -90;
                }
            }));

            fsm.GetState("Direction Wall").AddFirstAction(new ExecuteLambda(() =>
            {
                if (DiagonalSuperdash && skillUpgradeActive)
                {
                    if (GameManager.instance.inputHandler.inputActions.up.IsPressed)
                    {
                        SuperdashAngle = -45;
                    }
                    else if (GameManager.instance.inputHandler.inputActions.down.IsPressed)
                    {
                        SuperdashAngle = 45;
                    }
                }
            }));

            fsm.GetState("Left").AddAction(new ExecuteLambda(() =>
            {
                HeroController.instance.RotateHero(SuperdashAngle);
                if (BreakDiveFloorsFromBelow && SuperdashAngle <= -30) PlayMakerFSM.BroadcastEvent("QUAKE FALL START");
            })); 
            fsm.GetState("Right").AddAction(new ExecuteLambda(() =>
            {
                HeroController.instance.RotateHero(SuperdashAngle);
                if (BreakDiveFloorsFromBelow && SuperdashAngle <= -30) PlayMakerFSM.BroadcastEvent("QUAKE FALL START");
            }));
            #endregion

            #region Modify Dashing and Cancelable states
            FsmState dashing = fsm.GetState("Dashing");
            ExecuteLambda setVelocityVariables = new ExecuteLambda(() =>
            {
                float velComponent = Math.Abs(fsm.FsmVariables.GetFsmFloat("Current SD Speed").Value);

                vSpeed.Value = velComponent * (-1) * Mathf.Sin(SuperdashAngle * Mathf.PI / 180);
                hSpeed.Value = velComponent * Mathf.Cos(SuperdashAngle * Mathf.PI / 180) * (HeroController.instance.cState.facingRight ? 1 : -1);
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
                ResetSuperdashAngle();
            }));
            fsm.GetState("Cancel").AddFirstAction(new ExecuteLambda(() =>
            {
                ResetSuperdashAngle();
            }));
            fsm.GetState("Hit Wall").AddFirstAction(new ExecuteLambda(() =>
            {
                ResetSuperdashAngle();
            }));
            #endregion
        }
    }
}
