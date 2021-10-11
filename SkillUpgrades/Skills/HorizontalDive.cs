using System;
using System.Collections;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using SkillUpgrades.Util;

namespace SkillUpgrades.Skills
{
    public class HorizontalDive : AbstractSkillUpgrade
    {
        public bool PersistThroughHorizontalTransitions => GetBool(true);
        public bool LeftwardDiveAllowed => GetBool(true);
        public bool RightwardDiveAllowed => GetBool(true);

        public override string UIName => "Horizontal Dive";
        public override string Description => "Toggle whether Desolate Dive can be used horizontally.";

        public override bool InvolvesHeroRotation => true;


        private ILHook _hook;
        public override void Initialize()
        {
            On.HeroController.EnterScene += DisableHorizontalQuakeEntry;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ResetQuakeStateThroughTransitions;

            On.HeroController.Start += ModifyQuakeFSM;

            _hook = new ILHook
            (
                typeof(HeroController).GetMethod(nameof(HeroController.EnterScene)).GetStateMachineTarget(),
                AllowHorizontalQuakeEntry
            );
        }

        /// <summary>
        /// The angle the knight is diving, measured anticlockwise (regardless of facing direction)
        /// </summary>
        internal float QuakeAngle { get; set; } = 0f;
        internal void ResetQuakeAngle()
        {
            if (QuakeAngle == 0f) return;

            QuakeAngle = 0f;
            HeroRotation.ResetHero();
        }


        private void AllowHorizontalQuakeEntry(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            // slightly cursed code, because more involved modifications to IL code are fairly cursed
            while (cursor.TryGotoNext
            (
                MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdcI4(-1),
                i => i.Match(OpCodes.Stfld),
                i => i.MatchLdloc(1),
                i => i.MatchLdfld<HeroController>(nameof(HeroController.exitedSuperDashing))
            ))
            {
                cursor.EmitDelegate<Func<bool, bool>>(b => b || (HeroController.instance.exitedQuake && PersistThroughHorizontalTransitions));
            }

            cursor.Goto(0);
            while (cursor.TryGotoNext(MoveType.After, i => i.MatchLdstr("HeroCtrl-EnterSuperDash")))
            {
                cursor.EmitDelegate<Func<string, string>>(s =>
                {
                    if (!PersistThroughHorizontalTransitions) return s;
                    if (HeroController.instance.exitedQuake) return "HeroCtrl-EnterQuake";
                    return s;
                });
            }
        }

        private IEnumerator DisableHorizontalQuakeEntry(On.HeroController.orig_EnterScene orig, HeroController self, TransitionPoint enterGate, float delayBeforeEnter)
        {
            GatePosition gatePosition = enterGate.GetGatePosition();
            if (gatePosition == GatePosition.left || gatePosition == GatePosition.right)
            {
                if (!PersistThroughHorizontalTransitions) self.exitedQuake = false;
            }
            else if (gatePosition == GatePosition.door)
            {
                self.exitedQuake = false;
                ResetQuakeAngle();
            }
            else if (gatePosition == GatePosition.top)
            {
                // Need to do this to fix hdive into Town[top1] or Mines_13[top1]
                ResetQuakeAngle();
            }

            return orig(self, enterGate, delayBeforeEnter);
        }

        private void ResetQuakeStateThroughTransitions(Scene arg0, Scene arg1)
        {
            if (!PersistThroughHorizontalTransitions) ResetQuakeAngle();
        }

        private void ModifyQuakeFSM(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            PlayMakerFSM fsm = self.spellControl;

            fsm.GetState("Quake Finish").AddFirstAction(new ExecuteLambda(ResetQuakeAngle));
            fsm.GetState("Reset Cam Zoom").AddFirstAction(new ExecuteLambda(ResetQuakeAngle));
            fsm.GetState("FSM Cancel").AddFirstAction(new ExecuteLambda(ResetQuakeAngle));

            #region Add FSM Variables
            FsmFloat vSpeed = fsm.AddFsmFloat("V Speed HQ");
            FsmFloat hSpeed = fsm.AddFsmFloat("H Speed HQ");
            #endregion

            #region Set Direction Value
            fsm.GetState("Quake Antic").AddFirstAction(new ExecuteLambda(() =>
            {
                if (SkillUpgradeActive)
                {
                    if (InputHandler.Instance.inputActions.right.IsPressed && RightwardDiveAllowed) QuakeAngle = 90;
                    else if (InputHandler.Instance.inputActions.left.IsPressed && LeftwardDiveAllowed) QuakeAngle = -90;
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

            // Fix horizontal quake into Cliffs_02[right1], Mines_34[left1]
            fsm.GetState("Enter Quake").AddFirstAction(new ExecuteLambda(() =>
            {
                vSpeed.Value = -50f * Mathf.Cos(QuakeAngle * Mathf.PI / 180);
                hSpeed.Value = 50f * Mathf.Sin(QuakeAngle * Mathf.PI / 180);
                if (Math.Abs(vSpeed.Value) < 0.1f)
                {
                    Vector3 vec = HeroController.instance.transform.position;
                    vec += 0.1f * Vector3.up;
                    HeroController.instance.transform.position = vec;
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
