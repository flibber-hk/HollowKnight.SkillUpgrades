using System;
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

        protected override void StartUpInitialize()
        {
            base.StartUpInitialize();
        }
        protected override void RepeatableInitialize()
        {
            ModHooks.HeroUpdateHook += MonitorGlideRelease;
            On.HeroController.DoDoubleJump += OnDoubleJump;
            On.HeroController.BackOnGround += OnLand;
            IL.HeroController.FixedUpdate += SetGlideVelocity;
        }

        protected override void Unload()
        {
            ModHooks.HeroUpdateHook -= MonitorGlideRelease;
            On.HeroController.DoDoubleJump -= OnDoubleJump;
            On.HeroController.BackOnGround -= OnLand;
            IL.HeroController.FixedUpdate -= SetGlideVelocity;
        }

        /// <summary>
        /// If this is true, then the player hasn't released jump since they last double jumped.
        /// </summary>
        private bool Glidable = false;
        private void OnDoubleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            orig(self);
            Glidable = true;
        }
        private void MonitorGlideRelease()
        {
            if (Glidable && !InputHandler.Instance.inputActions.jump.IsPressed)
            {
                Glidable = false;
            }
            else if (Glidable)
            {
                GameObject dj = ReflectionHelper.GetField<HeroController, GameObject>(HeroController.instance, "dJumpWingsPrefab");
                if (!dj.activeSelf)
                {
                    dj.SetActive(true);
                }
            }
        }
        private void OnLand(On.HeroController.orig_BackOnGround orig, HeroController self)
        {
            Glidable = false;
            orig(self);
        }

        private void SetGlideVelocity(ILContext il)
        {
            ILCursor cursor = new(il);

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
