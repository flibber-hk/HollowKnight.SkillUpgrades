﻿using System;
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
    internal class BonusDash : AbstractSkillUpgrade
    {

        public int AirDashMax => GetInt(2);

        public override string UIName => "Multiple Air Dash";
        public override string Description => "Toggle whether dash can be used more than once before landing.";

        public override void Initialize()
        {
            AddRefreshHooks();
            On.HeroController.HeroDash += AllowExtraAirDash;
        }

        public override void Unload()
        {
            RemoveRefreshHooks();
            On.HeroController.HeroDash -= AllowExtraAirDash;
        }


        private int airDashCount;

        private void AllowExtraAirDash(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            bool shouldAirDash = !self.cState.onGround && !self.cState.inAcid;

            orig(self);
            if (shouldAirDash)
            {
                airDashCount++;

                if (airDashCount < AirDashMax || AirDashMax == -1)
                {
                    GameManager.instance.StartCoroutine(RefreshDashInAir());
                }
            }
        }

        private IEnumerator RefreshDashInAir()
        {
            yield return new WaitUntil(() => airDashCount == 0 || !InputHandler.Instance.inputActions.dash.IsPressed);
            if (airDashCount != 0)
            {
                ReflectionHelper.SetField(HeroController.instance, "airDashed", false);
            }
        }

        // Apparently, these are all the places where the game refreshes the player's air dash; we need to set the airDashCount to 0
        #region Restore Air Dash

        private readonly List<ILHook> _hooked = new List<ILHook>();
        private readonly string[] CoroHooks = new string[]
        {
            "<EnterScene>",
            "<HazardRespawn>",
            "<Respawn>"
        };

        private void AddRefreshHooks()
        {
            IL.HeroController.BackOnGround += RefreshAirDash;
            IL.HeroController.Bounce += RefreshAirDash;
            IL.HeroController.BounceHigh += RefreshAirDash;
            IL.HeroController.DoWallJump += RefreshAirDash;
            IL.HeroController.EnterSceneDreamGate += RefreshAirDash;
            IL.HeroController.ExitAcid += RefreshAirDash;
            IL.HeroController.LookForInput += RefreshAirDash;
            IL.HeroController.RegainControl += RefreshAirDash;
            IL.HeroController.ResetAirMoves += RefreshAirDash;
            IL.HeroController.ShroomBounce += RefreshAirDash;

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (string nested in CoroHooks)
            {
                Type nestedType = typeof(HeroController).GetNestedTypes(flags).First(x => x.Name.Contains(nested));

                _hooked.Add
                (
                    new ILHook
                    (
                        nestedType.GetMethod("MoveNext", flags),
                        RefreshAirDash
                    )
                );
            }

            _hooked.Add(new ILHook
            (
                typeof(HeroController).GetMethod("orig_Update", flags),
                RefreshAirDash
            ));
        }

        private void RemoveRefreshHooks()
        {
            IL.HeroController.BackOnGround -= RefreshAirDash;
            IL.HeroController.Bounce -= RefreshAirDash;
            IL.HeroController.BounceHigh -= RefreshAirDash;
            IL.HeroController.DoWallJump -= RefreshAirDash;
            IL.HeroController.EnterSceneDreamGate -= RefreshAirDash;
            IL.HeroController.ExitAcid -= RefreshAirDash;
            IL.HeroController.LookForInput -= RefreshAirDash;
            IL.HeroController.RegainControl -= RefreshAirDash;
            IL.HeroController.ResetAirMoves -= RefreshAirDash;
            IL.HeroController.ShroomBounce -= RefreshAirDash;

            foreach (ILHook hook in _hooked)
            {
                hook?.Dispose();
            }
            _hooked.Clear();
        }

        private void RefreshAirDash(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext
            (
                MoveType.After,
                i => i.MatchLdcI4(0),
                i => i.MatchStfld<HeroController>("airDashed")
            ))
            {
                cursor.EmitDelegate<Action>(() => airDashCount = 0);
            }
        }
        #endregion
    }
}
