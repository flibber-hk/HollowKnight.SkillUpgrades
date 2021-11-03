using System;
using System.Runtime.CompilerServices;
using Modding;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;
using SkillUpgrades.Util;
using System.Collections.Generic;

namespace SkillUpgrades.Skills
{
    public class DirectionalDash : AbstractSkillUpgrade
    {
        /// <summary>
        /// True: down dashes behave normal (i.e. no down diagonal, and down iff dashmaster equipped).
        /// False: all 8 directions are allowed
        /// </summary>
        [DefaultBoolValue(false)]
        [MenuTogglable(desc = "Toggle whether down dashes behave as normal")]
        public static bool UnmodifiedDownDashes;
        /// <summary>
        /// True: after a vertical dash, continue moving upwards (affected by gravity)
        /// False: after a vertical dash, stop moving immediately
        /// Deliberately not part of the menu
        /// </summary>
        [DefaultBoolValue(true)]
        public static bool MaintainVerticalMomentum;
        /// <summary>
        /// Straight up dashes get multiplied by this number
        /// </summary>
        [DefaultFloatValue(0.7937005f)] // 2^(-1/3)
        public static float UpdashPenalty;

        public override string Description => "Toggle whether dash can be used in all 8 directions.";


        protected override void RepeatableInitialize()
        {
            ModHooks.DashPressedHook += CalculateDashVector;
            ModHooks.DashVectorHook += OverrideDashVector;
            On.HeroController.HeroDash += ModifyPrefabDirection;
            IL.HeroController.HeroDash += ModifyDashmasterBool;
            ModHooks.GetPlayerBoolHook += InterpretDashmasterBool;

            On.HeroController.JumpReleased += MaintainMomentum;
            On.HeroController.Update += CancelPersistentMomentum;

            // This can fail if the UnmodifiedDownDashes setting changes after the skill is initialized, but I think that's unlikely to happen - 
            // particularly as OldDashmaster is disabled by default
            if (ModHooks.GetMod("QoL") is Mod && !UnmodifiedDownDashes)
            {
                DisableOldDashmaster();
            }
        }
        protected override void Unload()
        {
            ModHooks.DashPressedHook -= CalculateDashVector;
            ModHooks.DashVectorHook -= OverrideDashVector;
            On.HeroController.HeroDash -= ModifyPrefabDirection;
            IL.HeroController.HeroDash -= ModifyDashmasterBool;
            ModHooks.GetPlayerBoolHook -= InterpretDashmasterBool;

            On.HeroController.JumpReleased -= MaintainMomentum;
            On.HeroController.Update -= CancelPersistentMomentum;

            if (ModHooks.GetMod("QoL") is Mod)
            {
                RemoveOldDashmasterOverride();
            }
        }

        #region QoL interop
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void DisableOldDashmaster()
        {
            QoL.SettingsOverride.OverrideModuleToggle(nameof(QoL.Modules.OldDashmaster), false);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RemoveOldDashmasterOverride()
        {
            QoL.SettingsOverride.RemoveModuleOverride(nameof(QoL.Modules.OldDashmaster));
        }
        #endregion

        #region Maintaining vertical momentum out of an upward dash
        private void CancelPersistentMomentum(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            if (self.current_velocity.y <= 0f && !self.cState.dashing)
            {
                _maintainingVerticalDashMomentum = false;
            }
        }

        private void MaintainMomentum(On.HeroController.orig_JumpReleased orig, HeroController self)
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

        private bool CalculateDashVector()
        {
            HeroActions ia = InputHandler.Instance.inputActions;

            DashDirection direction = DashDirection.None;

            if (ia.up.IsPressed && !ia.down.IsPressed)
            {
                direction |= DashDirection.Up;
                if (ia.right.IsPressed) direction |= DashDirection.Right;
                else if (ia.left.IsPressed) direction |= DashDirection.Left;
            }
            else if (!UnmodifiedDownDashes && ia.down.IsPressed && !ia.up.IsPressed && !HeroController.instance.cState.onGround)
            {
                direction |= DashDirection.Down;
                if (ia.right.IsPressed) direction |= DashDirection.Right;
                else if (ia.left.IsPressed) direction |= DashDirection.Left;
            }

            _dashDirection = direction;
            return false;
        }

        private Vector2 OverrideDashVector(Vector2 orig)
        {
            if (_dashDirection == DashDirection.None) return orig;

            HeroController hero = HeroController.instance;

            float num = PlayerData.instance.GetBool(nameof(PlayerData.equippedCharm_16)) && hero.cState.shadowDashing
                ? hero.DASH_SPEED_SHARP
                : hero.DASH_SPEED;


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
            else if (_dashDirection == DashDirection.Up)
            {
                y *= Mathf.Clamp(UpdashPenalty, 0, 1);
            }

            _maintainingVerticalDashMomentum = _dashDirection.HasFlag(DashDirection.Up);
            return new Vector2(x, y);
        }

        private void ModifyPrefabDirection(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            orig(self); if (_dashDirection == DashDirection.None) return;

            float z = GetPrefabRotation(_dashDirection);

            float scale = self.cState.facingRight ? -1 : 1;
            self.dashBurst.transform.RotateAround(self.transform.position, Vector3.forward, z * scale);

            if (_dashDirection == DashDirection.Up || self.cState.shadowDashing)
            {
                // The dash effect prefab is either the shadow dash trail or the ground smoke. If we're dashing diagonally from the ground, 
                // we don't want to rotate the smoke or it won't appear.
                GameObject dashEffect = ReflectionHelper.GetField<HeroController, GameObject>(self, "dashEffect");
                dashEffect?.transform.RotateAround(self.transform.position, Vector3.forward, z * scale);
            }
        }

        private float GetPrefabRotation(DashDirection direction)
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

        private void ModifyDashmasterBool(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext
            (
                i => i.MatchLdstr(nameof(PlayerData.equippedCharm_31))
            ))
            {
                cursor.Remove();
                cursor.Emit(OpCodes.Ldstr, EnabledBool);
            }
        }
        private bool InterpretDashmasterBool(string name, bool orig)
        {
            if (name == EnabledBool)
            {
                // If UnmodifiedDownDashes is on, keep normal behaviour; if off, force the game to keep the downdashing field as false so we can implement our own
                return PlayerData.instance.GetBool(nameof(PlayerData.equippedCharm_31)) && UnmodifiedDownDashes;
            }
            return orig;
        }

        // The direction to dash, except return None if we're not overriding the normal behaviour
        private DashDirection _dashDirection;
        private bool _maintainingVerticalDashMomentum = false;

        private const string EnabledBool = "DirectionalDash.EquippedDashmaster";
    }
}
