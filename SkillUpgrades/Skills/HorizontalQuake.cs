using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using SkillUpgrades.Util;
using UnityEngine.SceneManagement;
using System.Collections;

namespace SkillUpgrades.Skills
{
    internal class HorizontalQuake : AbstractSkillUpgrade
    {
        public override string UIName => "Horizontal Dive";
        public override string Description => "Toggle whether Desolate Dive can be used horizontally.";

        public override bool InvolvesHeroRotation => true;

        public override void Initialize()
        {
            On.HeroController.EnterScene += DisableHorizontalQuakeEntry;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ResetQuakeState;

            On.HeroController.Start += ModifyQuakeFSM;
        }

        /// <summary>
        /// The angle the knight is diving, measured anticlockwise (regardless of facing direction)
        /// </summary>
        internal float QuakeAngle { get; set; } = 0f;
        internal void ResetQuakeAngle()
        {
            QuakeAngle = 0f;
            HeroRotation.ResetHero();
        }

        private IEnumerator DisableHorizontalQuakeEntry(On.HeroController.orig_EnterScene orig, HeroController self, TransitionPoint enterGate, float delayBeforeEnter)
        {
            GlobalEnums.GatePosition gatePosition = enterGate.GetGatePosition();
            if (gatePosition == GlobalEnums.GatePosition.left || gatePosition == GlobalEnums.GatePosition.right || gatePosition == GlobalEnums.GatePosition.door)
            {
                self.exitedQuake = false;
            }

            return orig(self, enterGate, delayBeforeEnter);
        }

        private void ResetQuakeState(Scene arg0, Scene arg1)
        {
            ResetQuakeAngle();
        }

        private void ModifyQuakeFSM(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            PlayMakerFSM fsm = self.spellControl;

            fsm.GetState("Quake Finish").AddFirstAction(new ExecuteLambda(() =>
            {
                ResetQuakeAngle();
            }));
            fsm.GetState("Reset Cam Zoom").AddFirstAction(new ExecuteLambda(() =>
            {
                ResetQuakeAngle();
            }));

            #region Add FSM Variables
            FsmFloat vSpeed = fsm.AddFsmFloat("V Speed HQ");
            FsmFloat hSpeed = fsm.AddFsmFloat("H Speed HQ");
            #endregion

            #region Set Direction Value
            fsm.GetState("Quake Antic").AddFirstAction(new ExecuteLambda(() =>
            {
                if (skillUpgradeActive)
                {
                    if (InputHandler.Instance.inputActions.right.IsPressed) QuakeAngle = 90;
                    else if (InputHandler.Instance.inputActions.left.IsPressed) QuakeAngle = -90;
                    HeroController.instance.RotateHero(QuakeAngle, respectFacingDirection: false);
                }

                float quakeAnticSpeed = fsm.FsmVariables.FindFsmFloat("Quake Antic Speed").Value;
                if (QuakeAngle != 0f && quakeAnticSpeed != 0f)
                {
                    // Very small Quake Antic Speed to move off the ground, so we don't stop quaking when hitting a seam
                    fsm.FsmVariables.FindFsmFloat("Quake Antic Speed").Value = Math.Max(0.1f, quakeAnticSpeed * Mathf.Cos(QuakeAngle * Mathf.PI / 180));
                }

                vSpeed.Value = -50f * Mathf.Cos(QuakeAngle * Mathf.PI / 180);
                hSpeed.Value = 50f * Mathf.Sin(QuakeAngle * Mathf.PI / 180);
            }));
            #endregion

            #region Velocity values
            void ModifyQuakeDownState(FsmState s)
            {
                ExecuteLambda setCCSevent = new ExecuteLambda(() =>
                {
                    CheckCollisionSide ccs = s.GetActionOfType<CheckCollisionSide>();
                    FsmEvent heroLanded = FsmEvent.GetFsmEvent("HERO LANDED");

                    if (hSpeed.Value < -0.4f)
                    {
                        ccs.leftHitEvent = heroLanded;
                        ccs.rightHitEvent = null;
                    }
                    else if (hSpeed.Value > 0.4f)
                    {
                        ccs.leftHitEvent = null;
                        ccs.rightHitEvent = heroLanded;
                    }
                    else
                    {
                        ccs.leftHitEvent = null;
                        ccs.rightHitEvent = null;
                    }
                    if (vSpeed.Value > 0.4f)
                    {
                        ccs.bottomHitEvent = heroLanded;
                    }
                    else
                    {
                        ccs.bottomHitEvent = null;
                    }
                });

                SetVelocity2d setvel = s.GetActionOfType<SetVelocity2d>();
                setvel.x = hSpeed;
                setvel.y = vSpeed;

                DecideToStopQuake decideToStop = new DecideToStopQuake(hSpeed, vSpeed);

                s.Actions = new FsmStateAction[]
                {
                    setCCSevent,
                    s.Actions[0],
                    s.Actions[1],
                    s.Actions[2],
                    s.Actions[3],
                    s.Actions[4],
                    s.Actions[5],
                    setvel,
                    s.Actions[7],
                    s.Actions[8], // CheckCollisionSide
                    decideToStop
                };
            }

            ModifyQuakeDownState(fsm.GetState("Quake1 Down"));
            ModifyQuakeDownState(fsm.GetState("Quake2 Down"));
            #endregion
        }
    }
}
