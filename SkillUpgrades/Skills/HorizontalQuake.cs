using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using SkillUpgrades.Util;
using UnityEngine.SceneManagement;
using System.Collections;

namespace SkillUpgrades.Skills
{
    internal static class HorizontalQuake
    {
        private static bool horizontalDiveEnabled => SkillUpgrades.globalSettings.GlobalToggle && SkillUpgrades.globalSettings.HorizontalDiveEnabled;

        private static QuakeDirection _quakeState;

        internal static QuakeDirection QuakeState
        {
            get => _quakeState;

            set
            {
                if (_quakeState == QuakeDirection.Normal && value == QuakeDirection.Leftward)
                {
                    HeroController.instance.transform.Rotate(0, 0, -90);
                }
                else if (_quakeState == QuakeDirection.Normal && value == QuakeDirection.Rightward)
                {
                    HeroController.instance.transform.Rotate(0, 0, 90);
                }
                else if (_quakeState == QuakeDirection.Leftward && value == QuakeDirection.Normal)
                {
                    HeroController.instance.transform.Rotate(0, 0, 90);
                }
                else if (_quakeState == QuakeDirection.Rightward && value == QuakeDirection.Normal)
                {
                    HeroController.instance.transform.Rotate(0, 0, -90);
                }
                _quakeState = value;
            }
        }

        internal enum QuakeDirection
        {
            Normal = 0,
            Leftward,
            Rightward
        }

        internal static void Hook()
        {
            On.HeroController.EnterScene += DisableHorizontalQuakeEntry;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ResetQuakeState;

            On.HeroController.Start += ModifyQuakeFSM;
        }



        private static IEnumerator DisableHorizontalQuakeEntry(On.HeroController.orig_EnterScene orig, HeroController self, TransitionPoint enterGate, float delayBeforeEnter)
        {
            GlobalEnums.GatePosition gatePosition = enterGate.GetGatePosition();
            if (gatePosition == GlobalEnums.GatePosition.left || gatePosition == GlobalEnums.GatePosition.right || gatePosition == GlobalEnums.GatePosition.door)
            {
                self.exitedQuake = false;
            }

            return orig(self, enterGate, delayBeforeEnter);
        }

        private static void ResetQuakeState(Scene arg0, Scene arg1)
        {
            QuakeState = QuakeDirection.Normal;
        }

        private static void ModifyQuakeFSM(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            PlayMakerFSM fsm = self.spellControl;

            fsm.GetState("Quake Finish").AddFirstAction(new ExecuteLambda(() =>
            {
                QuakeState = QuakeDirection.Normal;
            }));
            fsm.GetState("Reset Cam Zoom").AddFirstAction(new ExecuteLambda(() =>
            {
                QuakeState = QuakeDirection.Normal;
            }));

            #region Add FSM Variables
            FsmFloat vSpeed = fsm.AddFsmFloat("V Speed HQ");
            FsmFloat hSpeed = fsm.AddFsmFloat("H Speed HQ");
            #endregion

            #region Set Direction Value
            fsm.GetState("Quake Antic").AddFirstAction(new ExecuteLambda(() =>
            {
                if (horizontalDiveEnabled)
                {
                    if (InputHandler.Instance.inputActions.right.IsPressed) QuakeState = QuakeDirection.Rightward;
                    else if (InputHandler.Instance.inputActions.left.IsPressed) QuakeState = QuakeDirection.Leftward;
                }
                switch (QuakeState)
                {
                    case QuakeDirection.Rightward:
                        fsm.FsmVariables.FindFsmFloat("Quake Antic Speed").Value = 0f;
                        vSpeed.Value = 0f;
                        hSpeed.Value = 50f;
                        break;

                    case QuakeDirection.Leftward:
                        fsm.FsmVariables.FindFsmFloat("Quake Antic Speed").Value = 0f;
                        vSpeed.Value = 0f;
                        hSpeed.Value = -50f;
                        break;

                    case QuakeDirection.Normal:
                        vSpeed.Value = -50f;
                        hSpeed.Value = 0f;
                        break;
                }
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
                        ccs.bottomHitEvent = null;
                        ccs.rightHitEvent = null;
                    }
                    else if (hSpeed.Value > 0.4f)
                    {
                        ccs.leftHitEvent = null;
                        ccs.bottomHitEvent = null;
                        ccs.rightHitEvent = heroLanded;
                    }
                    else
                    {
                        ccs.leftHitEvent = null;
                        ccs.bottomHitEvent = heroLanded;
                        ccs.rightHitEvent = null;
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
