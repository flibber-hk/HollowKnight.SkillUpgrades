using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modding;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace SkillUpgrades.Skills
{
    public class TripleJump : AbstractSkillUpgrade
    {
        [DefaultIntValue(2)]
        public static int DoubleJumpMax;
        [DefaultIntValue(0)]
        [NotSaved]
        public static int LocalExtraJumps;

        public override string Description => "Toggle whether wings can be used more than once before landing.";


        protected override void RepeatableInitialize()
        {
            doubleJumpCount = 0;
            AddRefreshHooks();
            On.HeroController.DoDoubleJump += AllowTripleJump;
        }
        protected override void Unload()
        {
            RemoveRefreshHooks();
            On.HeroController.DoDoubleJump -= AllowTripleJump;
        }

        public override void AddToMenuList(List<IMenuMod.MenuEntry> entries)
        {
            void saver(int opt)
            {
                int val = Math.Abs((int)SkillUpgrades.GS.GetDefaultValue(nameof(TripleJump), nameof(DoubleJumpMax)));
                if (opt == 1) val *= -1;
                SkillUpgrades.GS.SetValue(nameof(TripleJump), nameof(DoubleJumpMax), val, SkillFieldSetOptions.ApplyToGlobalSetting);
            }

            IMenuMod.MenuEntry entry = new IMenuMod.MenuEntry()
            {
                Name = "Infinite Double Jump",
                Description = string.Empty,
                Values = new string[] {"False", "True"},
                Loader = () => (int)SkillUpgrades.GS.GetDefaultValue(nameof(TripleJump), nameof(DoubleJumpMax)) < 0 ? 1 : 0,
                Saver = saver
            };

            entries.Add(entry);
        }

        private int doubleJumpCount;

        private void AllowTripleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            // If the player has double jumped, deactivate the wings prefabs so they can reactivate
            if (doubleJumpCount > 0)
            {
                self.dJumpWingsPrefab.SetActive(false);
                self.dJumpFlashPrefab.SetActive(false);
            }

            orig(self);

            if (doubleJumpCount > 0)
            {
                InvokeUsedSkillUpgrade();
            }

            doubleJumpCount++;

            if (doubleJumpCount < DoubleJumpMax || DoubleJumpMax < 0)
            {
                GameManager.instance.StartCoroutine(RefreshWingsInAir());
            }
            else if (LocalExtraJumps > 0)
            {
                LocalExtraJumps -= 1;
                GameManager.instance.StartCoroutine(RefreshWingsInAir());
            }
        }

        private IEnumerator RefreshWingsInAir()
        {
            yield return new WaitUntil(() => doubleJumpCount == 0 || !InputHandler.Instance.inputActions.jump.IsPressed);
            if (doubleJumpCount != 0)
            {
                Modding.ReflectionHelper.SetField(HeroController.instance, "doubleJumped", false);
            }
        }

        // Apparently, these are all the places where the game refreshes the player's wings; we need to set the doubleJumpCount to 0
        #region Restore Double Jump

        private readonly List<ILHook> _hooked = new List<ILHook>();
        private readonly string[] CoroHooks = new string[]
        {
            "<EnterScene>",
            "<HazardRespawn>",
            "<Respawn>"
        };

        private void AddRefreshHooks()
        {
            IL.HeroController.BackOnGround += RefreshDoubleJump;
            IL.HeroController.Bounce += RefreshDoubleJump;
            IL.HeroController.BounceHigh += RefreshDoubleJump;
            IL.HeroController.DoWallJump += RefreshDoubleJump;
            IL.HeroController.EnterSceneDreamGate += RefreshDoubleJump;
            IL.HeroController.ExitAcid += RefreshDoubleJump;
            IL.HeroController.LookForInput += RefreshDoubleJump;
            IL.HeroController.ResetAirMoves += RefreshDoubleJump;
            IL.HeroController.ShroomBounce += RefreshDoubleJump;

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (string nested in CoroHooks)
            {
                Type nestedType = typeof(HeroController).GetNestedTypes(flags).First(x => x.Name.Contains(nested));

                _hooked.Add
                (
                    new ILHook
                    (
                        nestedType.GetMethod("MoveNext", flags),
                        RefreshDoubleJump
                    )
                );
            }

            _hooked.Add(new ILHook
            (
                typeof(HeroController).GetMethod("orig_Update", flags),
                RefreshDoubleJump
            ));
        }

        private void RemoveRefreshHooks()
        {
            IL.HeroController.BackOnGround -= RefreshDoubleJump;
            IL.HeroController.Bounce -= RefreshDoubleJump;
            IL.HeroController.BounceHigh -= RefreshDoubleJump;
            IL.HeroController.DoWallJump -= RefreshDoubleJump;
            IL.HeroController.EnterSceneDreamGate -= RefreshDoubleJump;
            IL.HeroController.ExitAcid -= RefreshDoubleJump;
            IL.HeroController.LookForInput -= RefreshDoubleJump;
            IL.HeroController.ResetAirMoves -= RefreshDoubleJump;
            IL.HeroController.ShroomBounce -= RefreshDoubleJump;

            foreach (ILHook hook in _hooked)
            {
                hook?.Dispose();
            }
            _hooked.Clear();
        }

        private void RefreshDoubleJump(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext
            (
                MoveType.After,
                i => i.MatchLdcI4(0),
                i => i.MatchStfld<HeroController>("doubleJumped")
            ))
            {
                cursor.EmitDelegate<Action>(() => doubleJumpCount = 0);
            }
        }
        #endregion
    }
}
