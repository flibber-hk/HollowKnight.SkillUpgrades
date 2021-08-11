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

            FsmState qOnGround = fsm.GetState("Q On Ground");
            FsmState qOffGround = fsm.GetState("Q Off Ground");
            FsmState directionCheck = new FsmState(fsm.GetState("Cancel"))
            {
                Name = "Direction Check HQ"
            };
            directionCheck.ClearTransitions();
            directionCheck.RemoveActionsOfType<FsmStateAction>();
            directionCheck.AddAction(new ExecuteLambda(() =>
            {
                if (!horizontalDiveEnabled) fsm.SendEvent("FINISHED");
                else if (InputHandler.Instance.inputActions.right.IsPressed) fsm.SendEvent("RIGHT");
                else if (InputHandler.Instance.inputActions.left.IsPressed) fsm.SendEvent("LEFT");
                else fsm.SendEvent("FINISHED");
            }));
            fsm.AddState(directionCheck);

            qOnGround.RemoveTransitionsTo("Quake Antic");
            qOffGround.RemoveTransitionsTo("Quake Antic");
            qOnGround.AddTransition("FINISHED", directionCheck.Name);
            qOffGround.AddTransition("FINISHED", directionCheck.Name);
            directionCheck.AddTransition("FINISHED", "Quake Antic");

            // Adding 12 states whee
            FsmState qaLeft = new FsmState(fsm.GetState("Quake Antic")) { Name = "Quake Antic HQL" };
            qaLeft.ClearTransitions();
            fsm.AddState(qaLeft);
            FsmState qaRight = new FsmState(fsm.GetState("Quake Antic")) { Name = "Quake Antic HQR" };
            qaRight.ClearTransitions();
            fsm.AddState(qaRight);
            FsmState lcLeft = new FsmState(fsm.GetState("Level Check 2")) { Name = "Level Check 2 HQL" };
            lcLeft.ClearTransitions();
            fsm.AddState(lcLeft);
            FsmState lcRight = new FsmState(fsm.GetState("Level Check 2")) { Name = "Level Check 2 HQR" };
            lcRight.ClearTransitions();
            fsm.AddState(lcRight);
            FsmState diveELeft = new FsmState(fsm.GetState("Q1 Effect")) { Name = "Q1 Effect HQL" };
            diveELeft.ClearTransitions();
            fsm.AddState(diveELeft);
            FsmState diveERight = new FsmState(fsm.GetState("Q1 Effect")) { Name = "Q1 Effect HQR" };
            diveERight.ClearTransitions();
            fsm.AddState(diveERight);
            FsmState darkELeft = new FsmState(fsm.GetState("Q2 Effect")) { Name = "Q2 Effect HQL" };
            darkELeft.ClearTransitions();
            fsm.AddState(darkELeft);
            FsmState darkERight = new FsmState(fsm.GetState("Q2 Effect")) { Name = "Q2 Effect HQR" };
            darkERight.ClearTransitions();
            fsm.AddState(darkERight);
            FsmState diveDLeft = new FsmState(fsm.GetState("Quake1 Down")) { Name = "Quake1 Down HQL" };
            fsm.AddState(diveDLeft);
            diveDLeft.FixTransitions();
            FsmState diveDRight = new FsmState(fsm.GetState("Quake1 Down")) { Name = "Quake1 Down HQR" };
            fsm.AddState(diveDRight);
            diveDRight.FixTransitions();
            FsmState darkDLeft = new FsmState(fsm.GetState("Quake2 Down")) { Name = "Quake2 Down HQL" };
            fsm.AddState(darkDLeft);
            darkDLeft.FixTransitions();
            FsmState darkDRight = new FsmState(fsm.GetState("Quake2 Down")) { Name = "Quake2 Down HQR" };
            fsm.AddState(darkDRight);
            darkDRight.FixTransitions();

            // Transitions
            directionCheck.AddTransition("LEFT", qaLeft.Name);
            directionCheck.AddTransition("RIGHT", qaRight.Name);
            qaLeft.AddTransition("ANIM END", lcLeft.Name);
            qaRight.AddTransition("ANIM END", lcRight.Name);
            lcLeft.AddTransition("LEVEL 1", diveELeft.Name);
            lcLeft.AddTransition("LEVEL 2", darkELeft.Name);
            lcRight.AddTransition("LEVEL 1", diveERight.Name);
            lcRight.AddTransition("LEVEL 2", darkERight.Name);
            diveELeft.AddTransition("FINISHED", diveDLeft.Name);
            diveERight.AddTransition("FINISHED", diveDRight.Name);
            darkELeft.AddTransition("FINISHED", darkDLeft.Name);
            darkERight.AddTransition("FINISHED", darkDRight.Name);


            // Don't need to leave the ground when horizontal quaking
            qaLeft.AddFirstAction(new ExecuteLambda(() => fsm.FsmVariables.FindFsmFloat("Quake Antic Speed").Value = 0f));
            qaRight.AddFirstAction(new ExecuteLambda(() => fsm.FsmVariables.FindFsmFloat("Quake Antic Speed").Value = 0f));

            // Set dive state
            qaLeft.AddFirstAction(new ExecuteLambda(() => QuakeState = QuakeDirection.Leftward));
            qaRight.AddFirstAction(new ExecuteLambda(() => QuakeState = QuakeDirection.Rightward));


            // Set velocity
            diveDLeft.GetActionOfType<SetVelocity2d>().SwapXandY();
            diveDLeft.GetActionOfType<GetVelocity2d>().SwapXandY();
            diveDLeft.GetActionOfType<CheckCollisionSide>().SetBottomToLeft();

            darkDLeft.GetActionOfType<SetVelocity2d>().SwapXandY();
            darkDLeft.GetActionOfType<GetVelocity2d>().SwapXandY();
            darkDLeft.GetActionOfType<CheckCollisionSide>().SetBottomToLeft();


            diveDRight.GetActionOfType<SetVelocity2d>().SwapXandY();
            diveDRight.GetActionOfType<SetVelocity2d>().x.Value *= -1;
            diveDRight.GetActionOfType<GetVelocity2d>().SwapXandY();
            diveDRight.GetActionOfType<CheckCollisionSide>().SetBottomToRight();

            darkDRight.GetActionOfType<SetVelocity2d>().SwapXandY();
            darkDRight.GetActionOfType<SetVelocity2d>().x.Value *= -1;
            darkDRight.GetActionOfType<GetVelocity2d>().SwapXandY();
            darkDRight.GetActionOfType<CheckCollisionSide>().SetBottomToRight();


            // Fix hero on "swag dive"
            fsm.GetState("Reset Cam Zoom").AddFirstAction(new ExecuteLambda(() =>
            {
                QuakeState = QuakeDirection.Normal;
            }));
        }
    }
}
