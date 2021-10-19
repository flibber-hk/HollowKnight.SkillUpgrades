using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MonoMod.Cil;
using UnityEngine;
using SkillUpgrades.Util;

namespace SkillUpgrades.Skills
{
    public class WallClimb : AbstractSkillUpgrade
    {
        public float ClimbSpeed => GetFloat(7.2f);
        public float ClimbSpeedConveyor => ClimbSpeed;


        public override string UIName => "Wall Climb";
        public override string Description => "Toggle whether claw can be used to climb up and down walls.";
        
        public override void Initialize()
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

        private void Conveyor_MoveUpOrDown(ILContext il)
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

        private void SuperdashWallCancel(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            FsmState wallCancel = self.superDash.GetState("Charge Cancel Wall");
            if (wallCancel.Actions[2] is SendMessage _)
            {
                wallCancel.Actions[2] = new ExecuteLambda(() =>
                {
                    if (!SkillUpgradeActive)
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
                    if (!SkillUpgradeActive || !HeroController.instance.cState.wallSliding)
                    {
                        HeroController.instance.AffectedByGravity(true);
                    }
                });
            }
        }

        private void StayOnWall(ILContext il)
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
                cursor.EmitDelegate<Func<bool, bool>>(pressed => pressed && !SkillUpgradeActive);
            }
        }

        private void SetWallslideSpeed(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            while (cursor.TryGotoNext
            (
                MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<HeroController>(nameof(HeroController.WALLSLIDE_SPEED))
            ))
            {
                cursor.EmitDelegate<Func<float, float>>((s) => SkillUpgradeActive ? 0 : s);
            }
        }

        private void MoveUpOrDown(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);

            if (SkillUpgradeActive && self.cState.wallSliding && Ref.HeroRigidBody.gravityScale <= Mathf.Epsilon && !self.cState.onConveyorV)
            {
                Vector2 pos = HeroController.instance.transform.position;

                // Don't go down if touching ground because they'll go OOB
                if (InputHandler.Instance.inputActions.down.IsPressed && !self.CheckTouchingGround())
                {
                    pos.y -= Time.deltaTime * ClimbSpeed;
                }

                // Don't go up if touching ceiling
                if (InputHandler.Instance.inputActions.up.IsPressed && !HeroCentreNearRoof(0.1f))
                {
                    pos.y += Time.deltaTime * ClimbSpeed;
                }

                HeroController.instance.transform.position = pos;
            }
        }

        private void HeroController_ResetState(On.HeroController.orig_ResetState orig, HeroController self)
        {
            if (self.cState.wallSliding && SkillUpgradeActive) HeroController.instance.AffectedByGravity(true);
            orig(self);
        }

        private void SetGravityOnWallslide(ILContext il)
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
                        cursor.EmitDelegate<Action>(() => { if (SkillUpgradeActive) HeroController.instance.AffectedByGravity(true); });
                        break;
                    case 1:
                        cursor.EmitDelegate<Action>(() => { if (SkillUpgradeActive) HeroController.instance.AffectedByGravity(false); });
                        break;
                    default:
                        break;
                }
            }
        }

        private bool HeroCentreNearRoof(float tol)
        {
            Vector2 vec = new Vector2(Ref.HeroCollider.bounds.center.x, Ref.HeroCollider.bounds.max.y);

            RaycastHit2D raycastHit2D = Physics2D.Raycast(vec, Vector2.up, tol, 256);
            return raycastHit2D.collider != null;
        }

        private bool HeroNearRoof(float tol)
        {
            Vector2 vec = new Vector2(Ref.HeroCollider.bounds.min.x, Ref.HeroCollider.bounds.max.y);
            RaycastHit2D raycastHit2D = Physics2D.Raycast(vec, Vector2.up, tol, 256);
            if (raycastHit2D.collider != null) return true;

            Vector2 vec2 = new Vector2(Ref.HeroCollider.bounds.center.x, Ref.HeroCollider.bounds.max.y);
            RaycastHit2D raycastHit2D2 = Physics2D.Raycast(vec2, Vector2.up, tol, 256);
            if (raycastHit2D2.collider != null) return true;

            Vector2 vec3 = new Vector2(Ref.HeroCollider.bounds.max.x, Ref.HeroCollider.bounds.max.y);
            RaycastHit2D raycastHit2D3 = Physics2D.Raycast(vec3, Vector2.up, tol, 256);
            if (raycastHit2D3.collider != null) return true;

            return false;
        }
    }
}
