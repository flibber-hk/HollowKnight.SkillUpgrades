using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using SkillUpgrades.Components;
using SkillUpgrades.FsmStateActions;
using SkillUpgrades.Util;

namespace SkillUpgrades.Skills
{
    public class VerticalSuperdash : AbstractSkillUpgrade
    {
        [DefaultBoolValue(true)]
        [MenuTogglable(desc: "Toggle whether diagonally superdashing is allowed")]
        public static bool DiagonalSuperdash;
        [DefaultBoolValue(false)]
        [MenuTogglable(desc: "Only works on certain dive floors")]
        public static bool BreakDiveFloorsFromBelow;
        [DefaultBoolValue(false)]
        [MenuTogglable(desc: "Affects Vertical Superdash")]
        public static bool ChangeDirectionInMidair;

        public override string Description => "Toggle whether Crystal Heart can be used in non-horizontal directions";

        private static readonly FastReflectionDelegate finishedEnteringScene = typeof(HeroController)
            .GetMethod("FinishedEnteringScene", BindingFlags.Instance | BindingFlags.NonPublic)
            .CreateFastDelegate();
        private static void FinishedEnteringScene(HeroController hero, bool setHazardMarker, bool preventRunBob)
        {
            finishedEnteringScene.Invoke(hero, setHazardMarker, preventRunBob);
        }

        #region Cached Compiler-Generated EnterScene related infos
        private static readonly MethodInfo heroEnterSceneMethod = typeof(HeroController)
            .GetMethod(nameof(HeroController.EnterScene))
            .GetStateMachineTarget();

        private static readonly Type heroEnterSceneIteratorType = heroEnterSceneMethod.DeclaringType;

        private static readonly FieldInfo HeroEnterSceneIteratorStateField = heroEnterSceneIteratorType
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .First(x => x.Name.Contains("state"));

        private static int GetEnterSceneState(IEnumerator e) => (int)HeroEnterSceneIteratorStateField.GetValue(e);
        #endregion

        private ILHook _hook;

        protected override void StartUpInitialize()
        {
            On.CameraTarget.Update += FixVerticalCamera;
            On.HeroController.Start += ModifySuperdashFsm;

            // Don't play weird animations when they should be cdashing
            On.HeroAnimationController.canPlayTurn += FixCdashAnimation;
            On.HeroAnimationController.PlayFromFrame += DontPlaySuperdash;
        }
        protected override void RepeatableInitialize()
        {
            // We need to move the entry coordinates for non-vertical oneways (Mines_34, Cliffs_02), and the TransitionPoint.entryOffset
            // is not used for horizontal transitions
            _hook = new(heroEnterSceneMethod, RepairHorizontalOneways);
            // The colliders of upward oneways are disabled (except for Tutorial_01[top1]) so we need to enable them
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ActivateUpwardOneways;
            // Complicated function to allow different behaviour when they enter scene from below with a cdash
            On.HeroController.EnterScene += EnableTransitionCdash;
        }
        protected override void Unload()
        {
            _hook?.Dispose();
            _hook = null;

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= ActivateUpwardOneways;
            On.HeroController.EnterScene -= EnableTransitionCdash;
        }

        private void RepairHorizontalOneways(ILContext il)
        {
            ILCursor cursor = new(il);

            cursor.GotoNext(i => i.MatchLdfld<HeroController>(nameof(HeroController.gatePosition)), i => i.MatchLdcI4(2));
            cursor.GotoNext(MoveType.After, i => i.MatchCallvirt<HeroController>("FindGroundPointY"));
            cursor.EmitDelegate<Func<float, float>>(x => GameManager.instance.sceneName == ItemChanger.SceneNames.Mines_34 ? 54.4f : x);

            cursor.GotoNext(i => i.MatchLdfld<HeroController>(nameof(HeroController.gatePosition)), i => i.MatchLdcI4(1));
            cursor.GotoNext(MoveType.After, i => i.MatchCallvirt<HeroController>("FindGroundPointY"));
            cursor.EmitDelegate<Func<float, float>>(x => GameManager.instance.sceneName == ItemChanger.SceneNames.Cliffs_02 ? 28.4f : x);
        }

        private void DontPlaySuperdash(On.HeroAnimationController.orig_PlayFromFrame orig, HeroAnimationController self, string clipName, int frame)
        {
            // This only matters when cdashing out of an upward transition, but there's no reason to play this anim anyway
            if (clipName == "Airborne" && HeroController.instance.cState.superDashing)
            {
                return;
            }
            orig(self, clipName, frame);
        }

        private void ActivateUpwardOneways(Scene _, Scene scene)
        {
            GameObject upwardOneway = scene.name switch
            {
                ItemChanger.SceneNames.RestingGrounds_02 => scene.GetRootGameObjects().First(x => x.name == "top1"),
                ItemChanger.SceneNames.Mines_13 => scene.GetRootGameObjects().First(x => x.name == "top1"),
                ItemChanger.SceneNames.Mines_23 => scene.GetRootGameObjects().First(x => x.name == "top1"),
                ItemChanger.SceneNames.Town => scene.GetRootGameObjects().First(x => x.name == "_Transition Gates").transform.Find("top1").gameObject,
                ItemChanger.SceneNames.Tutorial_01 => scene.GetRootGameObjects().First(x => x.name == "_Transition Gates").transform.Find("top1").gameObject,
                ItemChanger.SceneNames.Fungus2_25 => scene.GetRootGameObjects().First(x => x.name == "top2"),
                ItemChanger.SceneNames.Deepnest_East_03 => scene.GetRootGameObjects().First(x => x.name == "top2"),
                ItemChanger.SceneNames.Deepnest_01b => scene.GetRootGameObjects().First(x => x.name == "_Transition Gates").transform.Find("top2").gameObject,
                _ => null
            };

            if (upwardOneway != null)
            {
                upwardOneway.AddComponent<SuperdashMonitor>();
            }
        }

        private bool FixCdashAnimation(On.HeroAnimationController.orig_canPlayTurn orig, HeroAnimationController self)
        {
            return !HeroController.instance.cState.superDashing && orig(self);
        }

        private IEnumerator EnableTransitionCdash(On.HeroController.orig_EnterScene orig, HeroController self, TransitionPoint enterGate, float delayBeforeEnter)
        {
            IEnumerator e = orig(self, enterGate, delayBeforeEnter);

            if (e.GetType() != heroEnterSceneIteratorType)
            {
                LogWarn("Editing EnterScene blocked by a mod in assembly:\n" + e.GetType().Assembly.FullName);
                yield return e;
            }

            bool exitedSuperdashing = self.exitedSuperDashing;

            while (e.MoveNext())
            {
                yield return e.Current;

                if (GetEnterSceneState(e) == 10 && exitedSuperdashing)
                {
                    if (enterGate.GetGatePosition() != GatePosition.bottom)
                    {
                        LogError($"Unexpected Gate Position: {enterGate.GetGatePosition()}");
                    }

                    if (!enterGate.customFade)
                    {
                        GameManager.instance.FadeSceneIn();
                    }

                    self.exitedSuperDashing = true;
                    self.IgnoreInput();
                    self.proxyFSM.SendEvent("HeroCtrl-EnterSuperDash");
                    yield return new WaitForSeconds(0.25f);
                    FinishedEnteringScene(self, true, false);
                    yield break;
                }
            }
        }

        /// <summary>
        /// The angle the knight is superdashing, measured anticlockwise when the knight is facing left and clockwise when facing right
        /// </summary>
        internal float SuperdashAngle { get; set; } = 0f;

        private GameObject burst;
        internal void ResetSuperdashAngle()
        {
            if (!HeroController.instance.cState.superDashing && SuperdashAngle == 0f)
            {
                return;
            }

            SuperdashAngle = 0f;
            HeroRotator.Instance.ResetRotation();

            if (BreakDiveFloorsFromBelow) PlayMakerFSM.BroadcastEvent("QUAKE FALL END");

            if (burst != null)
            {
                burst.transform.parent = HeroController.instance.transform;
                burst.transform.rotation = Quaternion.identity;

                Vector3 vec = burst.transform.localScale;
                vec.x = Math.Abs(vec.x);
                burst.transform.localScale = vec;

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
                HeroRotator.Instance.Rotate(SuperdashAngle);
                if (BreakDiveFloorsFromBelow) PlayMakerFSM.BroadcastEvent("QUAKE FALL START");
            }));
            fsm.GetState("Right").AddAction(new ExecuteLambda(() =>
            {
                HeroRotator.Instance.Rotate(SuperdashAngle);
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

            void monitorDirectionalInputs(bool firstFrame)
            {
                if (!ChangeDirectionInMidair || !SkillUpgradeActive) return;

                // If any button was pressed this frame, we need to update for sure.
                // Otherwise, if any button was released, we only update if there's something being pressed (so they let go of up, and still go up).
                // If no inputs changed, then we don't need to bother, except on the first frame.

                HeroActions ia = InputHandler.Instance.inputActions;
                if (!(ia.left.WasPressed || ia.right.WasPressed || ia.up.WasPressed || ia.down.WasPressed
                    || ia.left.WasReleased || ia.right.WasReleased || ia.up.WasReleased || ia.down.WasReleased
                    || firstFrame)) return;

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

                if (horizontalPressed && DiagonalSuperdash) newSuperdashAngle /= 2f;

                if (horizontalPressed || verticalPressed)
                {
                    HeroRotator.Instance.SetRotation(newSuperdashAngle);
                    SuperdashAngle = newSuperdashAngle;
                    zeroTimer.Value = 0f;
                    setVelocityVariables();
                }
            }

            FsmStateAction setVelocityVariablesAction = new ExecuteLambda(setVelocityVariables);

            SetVelocity2d setVel = dashing.GetActionOfType<SetVelocity2d>();
            setVel.x = hSpeed;
            setVel.y = vSpeed;

            FsmStateAction decideToStop = new DecideToStopSuperdash(hSpeed, vSpeed, zeroLast);
            FsmStateAction turnInMidair = new ExecuteLambdaEveryFrame(monitorDirectionalInputs);

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

            void SetSuperdashAngleOnEntry()
            {
                switch (HeroController.instance.sceneEntryGate.GetGatePosition())
                {
                    case GatePosition.bottom: SuperdashAngle = -90f; break;
                    case GatePosition.top: SuperdashAngle = 90f; break;
                }
                HeroRotator.Instance.Rotate(SuperdashAngle);
            }
            fsm.GetState("Enter Velocity").Actions = new FsmStateAction[]
            {
                new ExecuteLambda(SetSuperdashAngleOnEntry),
                new SetVelocity2d()
                {
                    gameObject = setVel.gameObject,
                    vector = setVel.vector,
                    x = setVel.x,
                    y = setVel.y,
                    everyFrame = false
                },
                new ExecuteLambda(() =>
                {
                    if (BreakDiveFloorsFromBelow) PlayMakerFSM.BroadcastEvent("QUAKE FALL START");
                })
            };
            #endregion

            #region Reset Vertical Charge variable
            fsm.GetState("Air Cancel").AddFirstAction(new ExecuteLambda(() =>
            {
                ResetSuperdashAngle();
            }));
            fsm.GetState("Cancel").AddFirstAction(new ExecuteLambda(() =>
            {
                // Called on scene change
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
