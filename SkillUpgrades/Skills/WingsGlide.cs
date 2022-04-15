﻿using System;
using System.Linq;
using Modding;
using MonoMod.Cil;
using UnityEngine;

namespace SkillUpgrades.Skills
{
    public class WingsGlide : AbstractSkillUpgrade
    {
        [DefaultFloatValue(0.1f)]
        public static float GlideFallSpeedMultiplier;

        public override string Description => "Toggle whether to glide when holding jump after a double jump";

        protected override void RepeatableInitialize()
        {
            ModHooks.HeroUpdateHook += MonitorGlideRelease;
            On.HeroController.DoDoubleJump += OnDoubleJump;
            IL.HeroController.FixedUpdate += SetGlideVelocity;
            On.HeroController.ShouldHardLand += ShouldHardLand;

            Glidable = false;
        }

        protected override void Unload()
        {
            ModHooks.HeroUpdateHook -= MonitorGlideRelease;
            On.HeroController.DoDoubleJump -= OnDoubleJump;
            IL.HeroController.FixedUpdate -= SetGlideVelocity;
            On.HeroController.ShouldHardLand -= ShouldHardLand;

            Glidable = false;
        }

        private GameObject _doubleJumpPrefab;
        private GameObject DoubleJumpPrefab
        {
            get
            {
                if (_doubleJumpPrefab != null)
                {
                    return _doubleJumpPrefab;
                }
                GameObject go = ReflectionHelper.GetField<HeroController, GameObject>(HeroController.instance, "dJumpWingsPrefab");
                _doubleJumpPrefab = go;
                return go;
            }
        }

        private bool _glidable = false;
        /// <summary>
        /// If this is true, then the player hasn't released jump or landed since they last double jumped, and should glide.
        /// Always returns false if the skill upgrade is not active.
        /// </summary>
        public bool Glidable
        {
            get
            {
                return SkillUpgradeActive && _glidable;
            }
            private set
            {
                _glidable = value;
            }
        }
        private void OnDoubleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            orig(self);
            Glidable = true;
        }

        private bool ShouldNotBeGlidable()
        {
            return !InputHandler.Instance.inputActions.jump.IsPressed 
                || HeroController.instance.cState.onGround
                || HeroController.instance.cState.wallSliding;
        }

        private void MonitorGlideRelease()
        {
            if (Glidable && ShouldNotBeGlidable())
            {
                Glidable = false;
            }
            else if (Glidable)
            {
                // In this case they're gliding, so we replay the animation
                if (!DoubleJumpPrefab.activeSelf)
                {
                    DoubleJumpPrefab.SetActive(true);
                }
            }
        }

        private bool ShouldHardLand(On.HeroController.orig_ShouldHardLand orig, HeroController self, Collision2D collision)
        {
            // Never hard land if they're gliding
            if (Glidable) return false;
            return orig(self, collision);
        }

        private void SetGlideVelocity(ILContext il)
        {
            ILCursor cursor = new(il);

            // Replace all references to the terminal velocity with our own
            while (cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<HeroController>(nameof(HeroController.MAX_FALL_VELOCITY))))
            {
                cursor.EmitDelegate<Func<float, float>>(vel =>
                {
                    if (!Glidable) return vel;

                    return vel * GlideFallSpeedMultiplier;
                });
            }
        }
    }
}
