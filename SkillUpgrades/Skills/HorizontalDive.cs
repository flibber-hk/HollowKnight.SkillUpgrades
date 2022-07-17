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
using SkillUpgrades.Components;
using SkillUpgrades.FsmStateActions;
using SkillUpgrades.Util;

namespace SkillUpgrades.Skills
{
    public class HorizontalDive : AbstractSkillUpgrade
    {
        [DefaultBoolValue(true)]
        [MenuTogglable(name: "Horizontal transition dives", desc: "Toggle whether to remain diving after leaving a horizontal transition")]
        public static bool PersistDiveThroughHorizontalTransitions;
        [DefaultBoolValue(true)]
        [NotSaved]
        public static bool LeftwardDiveAllowed;
        [DefaultBoolValue(true)]
        [NotSaved]
        public static bool RightwardDiveAllowed;

        public const float DiveSpeed = 50f;

        public override string Description => "Toggle whether Desolate Dive can be used horizontally.";

        private ILHook _hook;
        protected override void StartUpInitialize()
        {
            On.HeroController.EnterScene += ModifyHorizontalQuakeEntry;
            On.HeroController.Start += ModifyQuakeFSM;
            On.HeroController.Respawn += RepairOnWarp;

            _hook = new ILHook
            (
                typeof(HeroController).GetMethod(nameof(HeroController.EnterScene)).GetStateMachineTarget(),
                AllowHorizontalQuakeEntry
            );
        }

        private IEnumerator RepairOnWarp(On.HeroController.orig_Respawn orig, HeroController self)
        {
            // Unrotate the knight when they benchwarp while mid-dive
            ResetQuakeAngle();
            return orig(self);
        }

        /// <summary>
        /// The angle the knight is diving, measured anticlockwise (regardless of facing direction)
        /// </summary>
        internal float QuakeAngle { get; set; } = 0f;
        internal void ResetQuakeAngle()
        {
            if (QuakeAngle == 0f) return;

            QuakeAngle = 0f;
            HeroRotator.Instance.ResetRotation();
        }


        private void AllowHorizontalQuakeEntry(ILContext il)
        {
            ILCursor cursor = new(il);

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
                /* The only times in the function that it checks for exitedSuperdashing are in the left/right gate positions, when
                 * it does the enter cdash stuff and then the coroutine finishes. We can use the same part of the code to implement
                 * the hdive entry behaviour - so we branch off if either exitedQuake or exitedSuperDashing is true, and choose
                 * the event to send to the ProxyFSM according to which is true. */
                cursor.EmitDelegate<Func<bool, bool>>(b => b || (HeroController.instance.exitedQuake && PersistDiveThroughHorizontalTransitions));
            }

            cursor.Goto(0);
            while (cursor.TryGotoNext(MoveType.After, i => i.MatchLdstr("HeroCtrl-EnterSuperDash")))
            {
                cursor.EmitDelegate<Func<string, string>>(s =>
                {
                    if (!PersistDiveThroughHorizontalTransitions) return s;
                    if (HeroController.instance.exitedQuake) return "HeroCtrl-EnterQuake";
                    return s;
                });
            }
        }

        // Failsafe if they quake through enough scenes without landing
        private int QuakedTransitions = 0;
        private IEnumerator ModifyHorizontalQuakeEntry(On.HeroController.orig_EnterScene orig, HeroController self, TransitionPoint enterGate, float delayBeforeEnter)
        {
            if (QuakedTransitions > 6)
            {
                QuakedTransitions = 0;
                ResetQuakeAngle();
                self.exitedQuake = false;
            }
            else if (self.exitedQuake)
            {
                QuakedTransitions++;
            }

            if (!self.exitedQuake)
            {
                return orig(self, enterGate, delayBeforeEnter);
            }

            GatePosition gatePosition = enterGate.GetGatePosition();

            switch (gatePosition)
            {
                case GatePosition.left when !PersistDiveThroughHorizontalTransitions:
                case GatePosition.right when !PersistDiveThroughHorizontalTransitions:
                    self.exitedQuake = false;
                    ResetQuakeAngle();
                    break;
                case GatePosition.left when PersistDiveThroughHorizontalTransitions:
                    QuakeAngle = 90;
                    HeroRotator.Instance.SetRotation(QuakeAngle, respectFacingDirection: false);
                    break;
                case GatePosition.right when PersistDiveThroughHorizontalTransitions:
                    QuakeAngle = -90;
                    HeroRotator.Instance.SetRotation(QuakeAngle, respectFacingDirection: false);
                    break;
                case GatePosition.top:
                    ResetQuakeAngle();
                    break;
                case GatePosition.door:
                case GatePosition.bottom:
                case GatePosition.unknown:
                    // I think I don't mind banning hdive from below or doors for now
                    self.exitedQuake = false;
                    ResetQuakeAngle();
                    break;
            }

            return orig(self, enterGate, delayBeforeEnter);
        }

        private void ModifyQuakeFSM(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            PlayMakerFSM fsm = self.spellControl;

            // Clear the failsafe whenever a dive finishes
            fsm.GetState("Quake Finish").AddFirstAction(new ExecuteLambda(() => { ResetQuakeAngle(); QuakedTransitions = 0; }));
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
                    HeroRotator.Instance.Rotate(QuakeAngle, respectFacingDirection: false);
                }

                float quakeAnticSpeed = fsm.FsmVariables.FindFsmFloat("Quake Antic Speed").Value;
                if (QuakeAngle != 0f && quakeAnticSpeed != 0f)
                {
                    // Very small Quake Antic Speed to move off the ground, so we don't stop quaking when hitting a seam
                    fsm.FsmVariables.FindFsmFloat("Quake Antic Speed").Value = Math.Max(0.09f, quakeAnticSpeed * Mathf.Cos(QuakeAngle * Mathf.PI / 180));
                }

                vSpeed.Value = -DiveSpeed * Mathf.Cos(QuakeAngle * Mathf.PI / 180);
                hSpeed.Value = DiveSpeed * Mathf.Sin(QuakeAngle * Mathf.PI / 180);
            }));

            // Fix horizontal quake into Cliffs_02[right1], Mines_34[left1]
            fsm.GetState("Enter Quake").AddFirstAction(new ExecuteLambda(() =>
            {
                vSpeed.Value = -DiveSpeed * Mathf.Cos(QuakeAngle * Mathf.PI / 180);
                hSpeed.Value = DiveSpeed * Mathf.Sin(QuakeAngle * Mathf.PI / 180);
                if (Math.Abs(vSpeed.Value) < 0.1f)
                {
                    // If we don't translate the hero up a little, the hdive just ends from the CheckCollisionSide action.
                    // We need to translate by something slightly more than 0.08f.
                    Vector3 vec = HeroController.instance.transform.position;
                    vec += 0.081f * Vector3.up;
                    HeroController.instance.transform.position = vec;
                }
            }));
            #endregion

            #region Velocity values
            void ModifyQuakeDownState(FsmState s)
            {
                FsmStateAction setCCSevent = new ExecuteLambda(() =>
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

                FsmStateAction decideToStop = new DecideToStopQuake(hSpeed, vSpeed);

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
