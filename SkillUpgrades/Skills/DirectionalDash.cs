using System;
using System.Reflection;
using GlobalEnums;
using Modding;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;
using SkillUpgrades.Util;

namespace SkillUpgrades.Skills
{
    internal class DirectionalDash : AbstractSkillUpgrade
    {
        [SerializeToSetting]
        public static bool AllowDownDiagonalDashes = true;
        [SerializeToSetting]
        public static bool MaintainVerticalMomentum = true;

        public override string Name => "Directional Dash";
        public override string Description => "Toggle whether dash can be used in all 8 directions.";


        public override void Initialize()
        {
            _dashEffect = typeof(HeroController).GetField("dashEffect", BindingFlags.NonPublic | BindingFlags.Instance);

            ModHooks.DashPressedHook += CalculateDashVector;
            ModHooks.DashVectorHook += OverrideDashVector;
            On.HeroController.HeroDash += ModifyPrefabDirection;
            IL.HeroController.HeroDash += ModifyDashmasterBool;
            ModHooks.GetPlayerBoolHook += InterpretDashmasterBool;

            On.HeroController.JumpReleased += MaintainMomentum;
            On.HeroController.Update += CancelPersistentMomentum;
        }
        public override void Unload()
        {
            _dashEffect = null;

            ModHooks.DashPressedHook -= CalculateDashVector;
            ModHooks.DashVectorHook -= OverrideDashVector;
            On.HeroController.HeroDash -= ModifyPrefabDirection;
            IL.HeroController.HeroDash -= ModifyDashmasterBool;
            ModHooks.GetPlayerBoolHook -= InterpretDashmasterBool;

            On.HeroController.JumpReleased -= MaintainMomentum;
            On.HeroController.Update -= CancelPersistentMomentum;
        }

        #region Maintaining vertical momentum out of an upward dash
        private static void CancelPersistentMomentum(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            if (self.current_velocity.y <= 0f && !self.cState.dashing)
            {
                _maintainingVerticalDashMomentum = false;
            }
        }

        private static void MaintainMomentum(On.HeroController.orig_JumpReleased orig, HeroController self)
        {
            if (Ref.HeroRigidBody.velocity.y > 0 && !self.inAcid && !self.cState.shroomBouncing 
                && _maintainingVerticalDashMomentum && MaintainVerticalMomentum)
            {
                ReflectionHelper.SetField<HeroController, bool>(self, "jumpQueuing", false);
                ReflectionHelper.SetField<HeroController, bool>(self, "doubleJumpQueuing", false);
                if (self.cState.swimming) self.cState.swimming = false;
                return;
            }

            orig(self);
        }
        #endregion

        private static bool CalculateDashVector()
        {
            HeroActions ia = InputHandler.Instance.inputActions;
            HeroController hero = HeroController.instance;

            DashDirection direction = DashDirection.None;

            if (ia.up.IsPressed) direction |= DashDirection.Up;
            else if (ia.down.IsPressed && !HeroController.instance.cState.onGround) direction |= DashDirection.Down;
            if (ia.right.IsPressed) direction |= DashDirection.Right;
            else if (ia.left.IsPressed) direction |= DashDirection.Left;
            if (direction == DashDirection.None)
            {
                if (hero.cState.facingRight) direction |= DashDirection.Right;
                else direction |= DashDirection.Left;
            }

            if (!AllowDownDiagonalDashes && direction.HasFlag(DashDirection.Down))
            {
                if (direction == (DashDirection.Down | DashDirection.Left)) direction = DashDirection.Left;
                else if (direction == (DashDirection.Down | DashDirection.Right)) direction = DashDirection.Right;
            }

            _dashDirection = direction;

            return false;
        }

        private static Vector2 OverrideDashVector(Vector2 arg)
        {
            HeroController hero = HeroController.instance;

            float num;
            if (PlayerData.instance.equippedCharm_16 && hero.cState.shadowDashing)
            {
                num = hero.DASH_SPEED_SHARP;
            }
            else
            {
                num = hero.DASH_SPEED;
            }

            float x = 0f;
            float y = 0f;
            if (_dashDirection.HasFlag(DashDirection.Up))
            {
                y = num;
            }
            else if (_dashDirection.HasFlag(DashDirection.Down))
            {
                y = -num;
            }
            if (y == 0)
            {
                CollisionSide collisonTest = HeroController.instance.cState.facingRight ? CollisionSide.right : CollisionSide.left;
                if (hero.CheckForBump(collisonTest))
                {
                    y = hero.cState.onGround ? 4f : 5f;
                }
            }
            if (_dashDirection.HasFlag(DashDirection.Right))
            {
                x = num;
            }
            else if (_dashDirection.HasFlag(DashDirection.Left))
            {
                x = -num;
            }

            if ((_dashDirection.HasFlag(DashDirection.Up) || _dashDirection.HasFlag(DashDirection.Down))
                && (_dashDirection.HasFlag(DashDirection.Left) || _dashDirection.HasFlag(DashDirection.Right)))
            {
                x *= (float)(1 / Math.Sqrt(2));
                y *= (float)(1 / Math.Sqrt(2));
            }

            _lastDashVector = new Vector2(x, y);
            _maintainingVerticalDashMomentum = _dashDirection.HasFlag(DashDirection.Up);
            return _lastDashVector;
        }

        private static void ModifyPrefabDirection(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            orig(self);

            float z = GetPrefabDirection(_dashDirection);

            self.dashBurst.transform.RotateAround(self.transform.position, Vector3.forward, z * self.transform.localScale.x);

            if (_dashDirection == DashDirection.Up || self.cState.shadowDashing)
            {
                // The dash effect prefab is either the shadow dash trail or the ground smoke. If we're dashing diagonally from the ground, 
                // we don't want to rotate the smoke or it won't appear.
                ((GameObject)_dashEffect.GetValue(self))?.transform.RotateAround(self.transform.position, Vector3.forward, z * self.transform.localScale.x);
            }
        }

        private static float GetPrefabDirection(DashDirection direction)
        {
            bool facingRight = HeroController.instance.cState.facingRight;

            switch (direction)
            {
                case DashDirection.Down:
                    return 90f;
                case DashDirection.Left:
                    return facingRight ? 180f : 0f;
                case DashDirection.Up:
                    return 270f;
                case DashDirection.Right:
                    return facingRight ? 0f : 180f;
                case DashDirection.Down | DashDirection.Left:
                    return facingRight ? 135f : 45f;
                case DashDirection.Down | DashDirection.Right:
                    return facingRight ? 45f : 135f;
                case DashDirection.Up | DashDirection.Left:
                    return facingRight ? 225f : 315f;
                case DashDirection.Up | DashDirection.Right:
                    return facingRight ? 315f : 225f;
            }

            return 0f;
        }

        [Flags]
        private enum DashDirection
        {
            None = 0,
            Left = 1,
            Right = 2,
            Up = 4,
            Down = 8
        }

        // Consider dashmaster to be off for the purpose of downdashing - only replace the first instruction, the second refers to the cooldown timer.
        // This isn't ideal, because the dash burst is a little off-center with the current rotation, but it looks good enough.
        private static void ModifyDashmasterBool(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext
            (
                i => i.MatchLdstr("equippedCharm_31")
            ))
            {
                cursor.Remove();
                cursor.Emit(OpCodes.Ldstr, EnabledBool);
            }
        }

        private static bool InterpretDashmasterBool(string name, bool orig)
        {
            if (name == EnabledBool)
            {
                return false;
            }
            return orig;
        }

        private static DashDirection _dashDirection;
        private static FieldInfo _dashEffect;

        private static Vector2 _lastDashVector;
        private static bool _maintainingVerticalDashMomentum = false;
        private const string EnabledBool = "DirectionalDash_Dashmaster";
    }
}
