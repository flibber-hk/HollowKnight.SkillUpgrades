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
    internal static class VerticalSuperdash
    {
        internal enum SuperdashDirection
        {
            Normal = 0,     // Anything not caused by this mod
            Upward
        }

        private static SuperdashDirection _superdashState = SuperdashDirection.Normal;
        internal static SuperdashDirection SuperdashState
        {
            get => _superdashState;

            set
            {
                if (value == SuperdashDirection.Upward && _superdashState == SuperdashDirection.Normal)
                {
                    HeroController.instance.transform.Rotate(0, 0, -90 * HeroController.instance.transform.localScale.x);
                }
                else if (value == SuperdashDirection.Normal && _superdashState == SuperdashDirection.Upward)
                {
                    // We need to set the SD Burst inactive before un-rotating the hero,
                    // so it doesn't rotate with it
                    if (GameObject.Find("SD Burst") is GameObject burst)
                    {
                        burst.transform.parent = HeroController.instance.gameObject.transform;
                        burst.SetActive(false);
                    }
                    HeroController.instance.transform.Rotate(0, 0, 90 * HeroController.instance.transform.localScale.x);
                }
                _superdashState = value;
            }
        }


        internal static void Hook()
        {
            On.CameraTarget.Update += FixVerticalCamera;
            On.GameManager.FinishedEnteringScene += DisableUpwardOneways;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ResetSuperdashState;
            On.HeroController.Start += ModifySuperdashFsm;
        }

        private static void FixVerticalCamera(On.CameraTarget.orig_Update orig, CameraTarget self)
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
            }
        }
        // Deactivate upward oneway transitions after spawning in so the player doesn't accidentally
        // softlock by vc-ing into them
        private static void DisableUpwardOneways(On.GameManager.orig_FinishedEnteringScene orig, GameManager self)
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
        private static void ResetSuperdashState(Scene arg0, Scene arg1)
        {
            SuperdashState = SuperdashDirection.Normal;
        }

        private static void ModifySuperdashFsm(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            PlayMakerFSM fsm = self.superDash;

            FsmState upDirectionCheck = new FsmState(fsm.GetState("Direction"))
            {
                Name = "Up Direction Check VC"
            };
            upDirectionCheck.ClearTransitions();
            fsm.AddState(upDirectionCheck);

            FsmState upStateR = new FsmState(fsm.GetState("Right"))
            {
                Name = "Up Right VC"
            };
            upStateR.ClearTransitions();
            upStateR.AddAction(new ExecuteLambda(() => SuperdashState = SuperdashDirection.Upward));
            fsm.AddState(upStateR);

            FsmState upStateL = new FsmState(fsm.GetState("Right"))
            {
                Name = "Up Left VC"
            };
            upStateL.ClearTransitions();
            upStateL.AddAction(new ExecuteLambda(() => SuperdashState = SuperdashDirection.Upward));
            fsm.AddState(upStateL);

            FsmState directionCheck = fsm.GetState("Direction");
            directionCheck.AddFirstAction(new ExecuteLambda(() =>
            {
                if (GameManager.instance.inputHandler.inputActions.up.IsPressed && SkillUpgrades.instance.globalSettings.VerticalSuperdashEnabled)
                {
                    fsm.SendEvent("BUTTON UP"); // This should be the "UP PRESSED" event, but IDK if we can use events not in the list
                }
            }));

            // Start dashing up
            FsmState upDashStart = new FsmState(fsm.GetState("Dash Start"))
            {
                Name = "Up Dash Start VC"
            };
            upDashStart.ClearTransitions();
            fsm.AddState(upDashStart);

            // Dashing Up
            FsmState upDashing = new FsmState(fsm.GetState("Dashing"))
            {
                Name = "Up Dashing VC"
            };
            upDashing.GetActionOfType<SetVelocity2d>().SwapXandY();
            upDashing.GetActionOfType<GetVelocity2d>().SwapXandY();
            upDashing.RemoveTransitionsTo("Cancelable");
            fsm.AddState(upDashing);

            // Cancelable dashing up
            FsmState upCancelable = new FsmState(fsm.GetState("Cancelable"))
            {
                Name = "Up Cancelable VC"
            };
            upCancelable.GetActionOfType<SetVelocity2d>().SwapXandY();
            upCancelable.GetActionOfType<GetVelocity2d>().SwapXandY();
            fsm.AddState(upCancelable);

            // Adding transitions
            directionCheck.AddTransition("BUTTON UP", upDirectionCheck.Name);
            upDirectionCheck.AddTransition("LEFT", upStateL.Name);
            upDirectionCheck.AddTransition("RIGHT", upStateR.Name);
            upStateR.AddTransition("FINISHED", upDashStart.Name);
            upStateL.AddTransition("FINISHED", upDashStart.Name);
            upDashStart.AddTransition("FINISHED", upDashing.Name);
            upDashing.AddTransition("WAIT", upCancelable.Name);

            // Reset Vertical Charge variable
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
        }
    }
}
