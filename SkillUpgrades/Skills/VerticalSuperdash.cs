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
        private static bool verticalSuperdashEnabled => SkillUpgrades.globalSettings.GlobalToggle && SkillUpgrades.globalSettings.VerticalSuperdashEnabled;
        private static bool diagonalSuperdashEnabled => verticalSuperdashEnabled && SkillUpgrades.globalSettings.DiagonalSuperdashEnabled;
        internal enum SuperdashDirection
        {
            Normal = 0,     // Anything not caused by this mod
            Upward,
            Diagonal
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
                else if (value == SuperdashDirection.Diagonal && _superdashState == SuperdashDirection.Normal)
                {
                    HeroController.instance.transform.Rotate(0, 0, -45 * HeroController.instance.transform.localScale.x);
                }
                else if (value == SuperdashDirection.Normal && _superdashState == SuperdashDirection.Diagonal)
                {
                    // We need to set the SD Burst inactive before un-rotating the hero,
                    // so it doesn't rotate with it
                    if (GameObject.Find("SD Burst") is GameObject burst)
                    {
                        burst.transform.parent = HeroController.instance.gameObject.transform;
                        burst.SetActive(false);
                    }
                    HeroController.instance.transform.Rotate(0, 0, 45 * HeroController.instance.transform.localScale.x);
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
                else if (SuperdashState == SuperdashDirection.Diagonal)
                {
                    self.cameraCtrl.lookOffset += Math.Abs(self.dashOffset) * ((float)Math.Sqrt(2) / 2f);
                    self.dashOffset *= (float)Math.Sqrt(2) / 2f;
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

            #region Vertical
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
                if (GameManager.instance.inputHandler.inputActions.up.IsPressed && verticalSuperdashEnabled)
                {
                    fsm.SendEvent("UP PRESSED");
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
            upDashing.FixTransitions();

            // Cancelable dashing up
            FsmState upCancelable = new FsmState(fsm.GetState("Cancelable"))
            {
                Name = "Up Cancelable VC"
            };
            upCancelable.GetActionOfType<SetVelocity2d>().SwapXandY();
            upCancelable.GetActionOfType<GetVelocity2d>().SwapXandY();
            fsm.AddState(upCancelable);
            upCancelable.FixTransitions();

            // Adding transitions
            directionCheck.AddTransition("UP PRESSED", upDirectionCheck.Name);
            upDirectionCheck.AddTransition("LEFT", upStateL.Name);
            upDirectionCheck.AddTransition("RIGHT", upStateR.Name);
            upStateR.AddTransition("FINISHED", upDashStart.Name);
            upStateL.AddTransition("FINISHED", upDashStart.Name);
            upDashStart.AddTransition("FINISHED", upDashing.Name);
            upDashing.AddTransition("WAIT", upCancelable.Name);
            #endregion


            #region Diagonal
            FsmFloat vSpeed = fsm.AddFsmFloat("V Speed VC");
            FsmFloat hSpeed = fsm.AddFsmFloat("H Speed VC");

            FsmState diagDirectionCheck = new FsmState(fsm.GetState("Direction"))
            {
                Name = "Diag Direction Check VC"
            };
            diagDirectionCheck.ClearTransitions();
            fsm.AddState(diagDirectionCheck);

            FsmState diagStateR = new FsmState(fsm.GetState("Right"))
            {
                Name = "Diag Right VC"
            };
            diagStateR.ClearTransitions();
            diagStateR.AddAction(new ExecuteLambda(() => SuperdashState = SuperdashDirection.Diagonal));
            fsm.AddState(diagStateR);

            FsmState diagStateL = new FsmState(fsm.GetState("Right"))
            {
                Name = "Diag Left VC"
            };
            diagStateL.ClearTransitions();
            diagStateL.AddAction(new ExecuteLambda(() => SuperdashState = SuperdashDirection.Diagonal));
            fsm.AddState(diagStateL);

            directionCheck.AddFirstAction(new ExecuteLambda(() =>
            {
                if (diagonalSuperdashEnabled)
                {
                    if (GameManager.instance.inputHandler.inputActions.up.IsPressed)
                    {
                        bool shouldDiagonal = false;
                        if (GameManager.instance.inputHandler.inputActions.right.IsPressed && HeroController.instance.cState.facingRight)
                        {
                            shouldDiagonal = true;
                        }
                        else if (GameManager.instance.inputHandler.inputActions.left.IsPressed && !HeroController.instance.cState.facingRight)
                        {
                            shouldDiagonal = true;
                        }
                        if (shouldDiagonal) fsm.SendEvent("DIAG PRESSED");
                    }
                }
            }));

            // Start dashing diag
            FsmState diagDashStart = new FsmState(fsm.GetState("Dash Start"))
            {
                Name = "Diag Dash Start VC"
            };
            diagDashStart.ClearTransitions();
            fsm.AddState(diagDashStart);

            // Dashing Diag
            FsmState diagDashing = new FsmState(fsm.GetState("Dashing"))
            {
                Name = "Diag Dashing VC"
            };

            {
                ExecuteLambda setVelocityVariables = new ExecuteLambda(() =>
                {
                    float velComponent = fsm.FsmVariables.GetFsmFloat("Current SD Speed").Value;
                    velComponent *= (float)(Math.Sqrt(2) / 2f);
                    vSpeed.Value = velComponent;
                    hSpeed.Value = velComponent * (HeroController.instance.cState.facingRight ? 1 : -1);
                });
                ExecuteLambdaEveryFrame decideToStop = new ExecuteLambdaEveryFrame(() =>
                {
                    Vector2 vector = HeroController.instance.gameObject.GetComponent<Rigidbody2D>().velocity;
                    if (Math.Abs(vector.x) < 0.1f || Math.Abs(vector.y) < 0.1f)
                    {
                        fsm.FsmVariables.GetFsmBool("Zero Last Frame").Value = true;
                    }
                });
                SetVelocity2d setVel = diagDashing.GetActionOfType<SetVelocity2d>();
                setVel.x = hSpeed;
                setVel.y = vSpeed;

                diagDashing.Actions = new FsmStateAction[]
                {
                    setVelocityVariables,
                    diagDashing.Actions[0],
                    diagDashing.Actions[1],
                    setVel,
                    diagDashing.Actions[3],
                    diagDashing.Actions[4],
                    decideToStop,
                    diagDashing.Actions[7],
                    diagDashing.Actions[8],
                };
            }

            diagDashing.RemoveTransitionsTo("Cancelable");
            fsm.AddState(diagDashing);
            diagDashing.FixTransitions();

            // Cancelable dashing diag
            FsmState diagCancelable = new FsmState(fsm.GetState("Cancelable"))
            {
                Name = "Diag Cancelable VC"
            };

            {
                ExecuteLambdaEveryFrame decideToStop = new ExecuteLambdaEveryFrame(() =>
                {
                    Vector2 vector = HeroController.instance.gameObject.GetComponent<Rigidbody2D>().velocity;
                    if (Math.Abs(vector.x) < 0.1f || Math.Abs(vector.y) < 0.1f)
                    {
                        fsm.FsmVariables.GetFsmBool("Zero Last Frame").Value = true;
                    }
                });
                SetVelocity2d setVel = diagCancelable.GetActionOfType<SetVelocity2d>();
                setVel.x = hSpeed;
                setVel.y = vSpeed;

                diagCancelable.Actions = new FsmStateAction[]
                {
                    diagCancelable.Actions[0],
                    setVel,
                    diagCancelable.Actions[2],
                    decideToStop,
                    diagCancelable.Actions[5],
                    diagCancelable.Actions[6],
                };
            }
            fsm.AddState(diagCancelable);
            diagCancelable.FixTransitions();

            // Adding transitions
            directionCheck.AddTransition("DIAG PRESSED", diagDirectionCheck.Name);
            diagDirectionCheck.AddTransition("LEFT", diagStateL.Name);
            diagDirectionCheck.AddTransition("RIGHT", diagStateR.Name);
            diagStateR.AddTransition("FINISHED", diagDashStart.Name);
            diagStateL.AddTransition("FINISHED", diagDashStart.Name);
            diagDashStart.AddTransition("FINISHED", diagDashing.Name);
            diagDashing.AddTransition("WAIT", diagCancelable.Name);
            #endregion

            #region Diagonal Wall Cdash
            FsmState wallDirectionCheck = fsm.GetState("Direction Wall");

            ExecuteLambda wallDiagTest = new ExecuteLambda(() =>
            {
                if (diagonalSuperdashEnabled)
                {
                    if (GameManager.instance.inputHandler.inputActions.up.IsPressed)
                    {
                        fsm.SendEvent("UP PRESSED");
                    }
                }
            });

            wallDirectionCheck.Actions = new FsmStateAction[]
            {
                wallDirectionCheck.Actions[0],
                wallDirectionCheck.Actions[1],
                wallDirectionCheck.Actions[2],
                wallDirectionCheck.Actions[3],
                wallDiagTest,
                wallDirectionCheck.Actions[4]
            };

            wallDirectionCheck.AddTransition("UP PRESSED", diagDirectionCheck.Name);
            #endregion

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
