using System;
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

            ResetState();
        }

        protected override void Unload()
        {
            ModHooks.HeroUpdateHook -= MonitorGlideRelease;
            On.HeroController.DoDoubleJump -= OnDoubleJump;
            IL.HeroController.FixedUpdate -= SetGlideVelocity;
            On.HeroController.ShouldHardLand -= ShouldHardLand;

            ResetState();
        }

        private void ResetState()
        {
            Glidable = false;
            TimeSinceFinishedGliding = 0f;
            LoopedAnimation = false;
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

        /// <summary>
        /// Track time since finished gliding to modify the hard land condition
        /// </summary>
        public float TimeSinceFinishedGliding { get; private set; } = 0f;

        /// <summary>
        /// True if the wings animation has already been looped by us
        /// </summary>
        private bool LoopedAnimation { get; set; } = false;

        private void OnDoubleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            orig(self);
            Glidable = true;
            TimeSinceFinishedGliding = 0f;
        }

        private bool ShouldNotBeGlidable()
        {
            return !InputHandler.Instance.inputActions.jump.IsPressed 
                || HeroController.instance.cState.onGround
                || HeroController.instance.cState.wallSliding
                || HeroController.instance.cState.recoiling
                || HeroController.instance.cState.hazardDeath;
        }

        private void MonitorGlideRelease()
        {
            if (!Glidable)
            {
                TimeSinceFinishedGliding += Time.deltaTime;
            }

            if (Glidable && ShouldNotBeGlidable())
            {
                Glidable = false;
                // Remove the wings animation if they stop gliding
                if (DoubleJumpPrefab.activeSelf && LoopedAnimation)
                {
                    DoubleJumpPrefab.SetActive(false);
                }
                LoopedAnimation = false;
            }
            else if (Glidable)
            {
                // In this case they're gliding, so we replay the animation
                if (!DoubleJumpPrefab.activeSelf)
                {
                    DoubleJumpPrefab.SetActive(true);
                    LoopedAnimation = true;
                }
            }
        }

        private bool ShouldHardLand(On.HeroController.orig_ShouldHardLand orig, HeroController self, Collision2D collision)
        {
            // Never hard land if they're gliding
            if (Glidable) return false;
            
            // If they recently stopped gliding, prevent hardfall
            // The computation here is crude, but has the property that if the fall speed multiplier is 1 (i.e. glide has no effect)
            // then they don't change whether they hard fall when not gliding
            // TODO - if Tg is the time to reach glide velocity, then the RHS should be BIG_FALL_TIME - Tg.
            if (TimeSinceFinishedGliding < (1 - GlideFallSpeedMultiplier) * HeroController.instance.BIG_FALL_TIME)
            {
                return false;
            }

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
