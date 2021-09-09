using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using SkillUpgrades.Util;

namespace SkillUpgrades.Skills
{
    internal static class WallClimb
    {
        private static bool climbEnabled => SkillUpgrades.globalSettings.GlobalToggle && SkillUpgrades.globalSettings.WallClimb;

        public static float ClimbSpeed = SkillUpgrades.globalSettings.ClimbSpeed;
        public static float ClimbSpeedConveyor = ClimbSpeed;
        
        internal static void Hook()
        {
            // Replace WALLSLIDE_SPEED with 0 in the Fixed Update function, so the knight doesn't move down walls. 
            // Doing it like this allows the skill to be more easily toggled "live".
            IL.HeroController.FixedUpdate += SetWallslideSpeed;

            // When the knight is on the wall, we need to set gravity to 0 so it doesn't fall down 
            // (it would be very slow, though, because of the WALLSLIDE_SPEED being read as 0)
            IL.HeroController.LookForInput += SetGravityOnWallslide;
            IL.HeroController.RegainControl += SetGravityOnWallslide;
            IL.HeroController.FinishedDashing += SetGravityOnWallslide;
            // When the knight leaves the wall, return gravity
            IL.HeroController.CancelWallsliding += SetGravityOnWallslide;
            IL.HeroController.TakeDamage += SetGravityOnWallslide;

            // Stay wallsliding - "wallclinging" - when the player presses down
            IL.HeroController.LookForInput += StayOnWall;

            // If the game tries to reset the cState, make sure gravity is set to true - this probably won't actually ever matter
            On.HeroController.ResetState += HeroController_ResetState;

            // Allow the player to climb up and down
            On.HeroController.Update += MoveUpOrDown;

            // Allow the player to climb up and down, while on a conveyor
            IL.ConveyorMovementHero.LateUpdate += Conveyor_MoveUpOrDown;

            // Don't restore gravity if the player cancels a wall cdash
            On.HeroController.Start += SuperdashWallCancel;
        }

        private static void Conveyor_MoveUpOrDown(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            if (cursor.TryGotoNext
            (
                i => i.MatchLdfld<ConveyorMovementHero>("ySpeed"),
                i => i.MatchNewobj<Vector2>()
            ))
            {
                cursor.GotoNext();
                cursor.EmitDelegate<Func<float, float>>(ySpeed =>
                {
                    if (InputHandler.Instance.inputActions.down.IsPressed && !HeroController.instance.CheckTouchingGround())
                    {
                        ySpeed -= ClimbSpeedConveyor;
                    }

                    if (InputHandler.Instance.inputActions.up.IsPressed)
                    {
                        ySpeed += ClimbSpeedConveyor;
                    }

                    return ySpeed;
                });
            }
        }

        private static void SuperdashWallCancel(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            FsmState wallCancel = self.superDash.GetState("Charge Cancel Wall");
            if (wallCancel.Actions[2] is SendMessage _)
            {
                wallCancel.Actions[2] = new ExecuteLambda(() =>
                {
                    if (!climbEnabled)
                    {
                        HeroController.instance.AffectedByGravity(true);
                    }
                });
            }

            FsmState regainControl = self.superDash.GetState("Regain Control");
            if (regainControl.Actions[5] is SendMessage _)
            {
                regainControl.Actions[5] = new ExecuteLambda(() =>
                {
                    if (!climbEnabled || !HeroController.instance.cState.wallSliding)
                    {
                        HeroController.instance.AffectedByGravity(true);
                    }
                });
            }
        }

        private static void StayOnWall(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            while (cursor.TryGotoNext
            (
                // There is only one check for down.WasPressed in LookForInput
                MoveType.After,
                i => i.MatchLdfld<InputHandler>(nameof(InputHandler.inputActions)),
                i => i.MatchLdfld<HeroActions>(nameof(HeroActions.down)),
                i => i.MatchCallvirt<InControl.OneAxisInputControl>("get_WasPressed")
            ))
            {
                cursor.EmitDelegate<Func<bool, bool>>(pressed => pressed && !climbEnabled);
            }
        }

        private static void SetWallslideSpeed(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            while (cursor.TryGotoNext
            (
                MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>(nameof(HeroController.WALLSLIDE_SPEED))
            ))
            {
                cursor.EmitDelegate<Func<float, float>>((s) => climbEnabled ? 0 : s);
            }
        }

        private static void MoveUpOrDown(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);

            if (climbEnabled && self.cState.wallSliding && Ref.HeroRigidBody.gravityScale <= Mathf.Epsilon && !self.cState.onConveyorV)
            {
                Vector2 pos = HeroController.instance.transform.position;

                // Don't go down if touching ground because they'll go OOB
                if (InputHandler.Instance.inputActions.down.IsPressed && !self.CheckTouchingGround())
                {
                    pos.y -= Time.deltaTime * ClimbSpeed;
                }

                if (InputHandler.Instance.inputActions.up.IsPressed)
                {
                    pos.y += Time.deltaTime * ClimbSpeed;
                }

                HeroController.instance.transform.position = pos;
            }
        }

        private static void HeroController_ResetState(On.HeroController.orig_ResetState orig, HeroController self)
        {
            if (self.cState.wallSliding && climbEnabled) HeroController.instance.AffectedByGravity(true);
            orig(self);
        }

        // Currently, this does not stop gravity from being reactivated when the player cancels a wall C-dash before finishing charging.
        private static void SetGravityOnWallslide(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);
            int matchedBool = -1;

            while (cursor.TryGotoNext
            (
                MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>(nameof(HeroController.cState)),
                i => i.MatchLdcI4(out matchedBool),
                i => i.MatchStfld<HeroControllerStates>(nameof(HeroControllerStates.wallSliding))
            ))
            {
                switch (matchedBool)
                {
                    case 0:
                        cursor.EmitDelegate<Action>(() => { if (climbEnabled) HeroController.instance.AffectedByGravity(true); });
                        break;
                    case 1:
                        cursor.EmitDelegate<Action>(() => { if (climbEnabled) HeroController.instance.AffectedByGravity(false); });
                        break;
                    default:
                        break;
                }

            }
        }
    }
}
