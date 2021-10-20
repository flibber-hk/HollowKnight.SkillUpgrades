using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using SkillUpgrades.FsmStateActions;
using SkillUpgrades.Util;

namespace SkillUpgrades.Skills
{
    public class VerticalSuperdash : AbstractSkillUpgrade
    {
        public bool DiagonalSuperdash => GetBool(true);
        public bool BreakDiveFloorsFromBelow => GetBool(false);
        public bool ChangeDirectionInMidair => GetBool(false);


        public override string Description => "Toggle whether Crystal Heart can be used in non-horizontal directions";

        public override bool InvolvesHeroRotation => true;

        protected override void StartUpInitialize()
        {
            On.CameraTarget.Update += FixVerticalCamera;
            On.GameManager.FinishedEnteringScene += DisableUpwardOneways;
            On.HeroController.Start += ModifySuperdashFsm;
        }

        /// <summary>
        /// The angle the knight is superdashing, measured anticlockwise when the knight is facing left and clockwise when facing right
        /// </summary>
        internal float SuperdashAngle { get; set; } = 0f;

        private GameObject burst;
        internal void ResetSuperdashAngle()
        {
            if (!HeroController.instance.cState.superDashing)
            {
                return;
            }

            SuperdashAngle = 0f;
            HeroRotation.ResetHero();

            if (BreakDiveFloorsFromBelow) PlayMakerFSM.BroadcastEvent("QUAKE FALL END");

            if (burst != null)
            {
                burst.transform.parent = HeroController.instance.gameObject.transform;
                burst.transform.rotation = Quaternion.identity;
                burst.SetActive(false);
            }
        }

        // This function is slightly broken :(
        private void FixVerticalCamera(On.CameraTarget.orig_Update orig, CameraTarget self)
        {
            orig(self);
            if (self.hero_ctrl == null || GameManager.instance == null || !GameManager.instance.IsGameplayScene()) return;
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

        private void ModifySuperdashFsm(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);
            burst = self.transform.Find("Effects/SD Burst").gameObject;

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
                if (DiagonalSuperdash && SkillUpgradeActive)
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
                if (SkillUpgradeActive && !shouldDiagonal)
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
                if (DiagonalSuperdash && SkillUpgradeActive)
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
                if (BreakDiveFloorsFromBelow) PlayMakerFSM.BroadcastEvent("QUAKE FALL START");
            }));
            fsm.GetState("Right").AddAction(new ExecuteLambda(() =>
            {
                HeroController.instance.RotateHero(SuperdashAngle);
                if (BreakDiveFloorsFromBelow) PlayMakerFSM.BroadcastEvent("QUAKE FALL START");
            }));
            #endregion

            #region Modify Dashing and Cancelable states
            FsmState dashing = fsm.GetState("Dashing");
            FsmState cancelable = fsm.GetState("Cancelable");
            FsmBool zeroLast = fsm.FsmVariables.GetFsmBool("Zero Last Frame");
            FsmFloat zeroTimer = fsm.FsmVariables.GetFsmFloat("Zero Timer");

            void setVelocityVariables()
            {
                float velComponent = Math.Abs(fsm.FsmVariables.GetFsmFloat("Current SD Speed").Value);

                vSpeed.Value = velComponent * (-1) * Mathf.Sin(SuperdashAngle * Mathf.PI / 180);
                hSpeed.Value = velComponent * Mathf.Cos(SuperdashAngle * Mathf.PI / 180) * (HeroController.instance.cState.facingRight ? 1 : -1);
            }

            void monitorDirectionalInputs()
            {
                if (!ChangeDirectionInMidair) return;

                // If any button was pressed this frame, we need to update for sure.
                // Otherwise, if any button was released, we only update if there's something being pressed (so they let go of up, and still go up).
                // If no inputs changed, then we don't need to bother.

                HeroActions ia = InputHandler.Instance.inputActions;
                if (!(ia.left.WasPressed || ia.right.WasPressed || ia.up.WasPressed || ia.down.WasPressed
                    || ia.left.WasReleased || ia.right.WasReleased || ia.up.WasReleased || ia.down.WasReleased)) return;

                bool horizontalPressed = false;
                bool verticalPressed = false;
                if (ia.left.IsPressed && !ia.right.IsPressed)
                {
                    horizontalPressed = true;
                    if (HeroController.instance.cState.facingRight)
                    {
                        HeroController.instance.FaceLeft();
                    }
                    
                }
                else if (!ia.left.IsPressed && ia.right.IsPressed)
                {
                    horizontalPressed = true;
                    if (!HeroController.instance.cState.facingRight)
                    {
                        HeroController.instance.FaceRight();
                    }
                }

                float newSuperdashAngle = 0f;
                if (ia.up.IsPressed && !ia.down.IsPressed)
                {
                    newSuperdashAngle = -90f;
                    verticalPressed = true;
                }
                else if (!ia.up.IsPressed && ia.down.IsPressed)
                {
                    newSuperdashAngle = 90f;
                    verticalPressed = true;
                }

                if (horizontalPressed) newSuperdashAngle /= 2f;

                if (horizontalPressed || verticalPressed)
                {
                    HeroController.instance.SetHeroRotation(newSuperdashAngle);
                    SuperdashAngle = newSuperdashAngle;
                    zeroTimer.Value = 0f;
                    setVelocityVariables();
                }
            }

            ExecuteLambda setVelocityVariablesAction = new ExecuteLambda(setVelocityVariables);

            SetVelocity2d setVel = dashing.GetActionOfType<SetVelocity2d>();
            setVel.x = hSpeed;
            setVel.y = vSpeed;

            DecideToStopSuperdash decideToStop = new DecideToStopSuperdash(hSpeed, vSpeed, zeroLast);
            ExecuteLambdaEveryFrame turnInMidair = new ExecuteLambdaEveryFrame(monitorDirectionalInputs);

            dashing.Actions = new FsmStateAction[]
            {
                    setVelocityVariablesAction,
                    dashing.Actions[0], // Stop if speed is zero
                    dashing.Actions[1], // Move to cancelable after enough time
                    setVel,
                    dashing.Actions[3], // Not affected by gravity
                    dashing.Actions[4], // (same, sort of)
                    decideToStop,
                    dashing.Actions[7], // Check if speed has been zero for long enough to stop
                    dashing.Actions[8], // (same as above)
            };

            cancelable.Actions = new FsmStateAction[]
            {
                    cancelable.Actions[0], // Cancel if pressed Jump
                    setVel,
                    cancelable.Actions[2], // Cancel if pressed Superdash
                    decideToStop,
                    cancelable.Actions[5], // Check if speed has been zero for long enough to stop
                    cancelable.Actions[6], // (same as above)
                    turnInMidair,
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
