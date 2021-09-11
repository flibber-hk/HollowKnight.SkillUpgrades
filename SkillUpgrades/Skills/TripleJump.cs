using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using UnityEngine;

namespace SkillUpgrades.Skills
{
    internal class TripleJump : AbstractSkillUpgrade
    {
        [SerializeToSetting]
        public static int DoubleJumpMax = 2;

        public override string Name => "Multiple Wings";
        public override string Description => "Toggle whether wings can be used more than once before landing.";


        public override void Initialize()
        {
            doubleJumpCount = 0;
            AddRefreshHooks();
            On.HeroController.DoDoubleJump += AllowTripleJump;
        }
        public override void Unload()
        {
            RemoveRefreshHooks();
            On.HeroController.DoDoubleJump -= AllowTripleJump;
        }


        private static int doubleJumpCount;
        internal static void RefreshDoubleJump()
        {
            doubleJumpCount = 0;
        }

        private static void AllowTripleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            // If the player has double jumped, deactivate the wings prefabs so they can reactivate
            if (doubleJumpCount > 0)
            {
                self.dJumpWingsPrefab.SetActive(false);
                self.dJumpFlashPrefab.SetActive(false);
            }

            orig(self);
            doubleJumpCount++;

            if (doubleJumpCount < DoubleJumpMax || DoubleJumpMax == -1)
            {
                GameManager.instance.StartCoroutine(RefreshWingsInAir());
            }
        }

        private static System.Collections.IEnumerator RefreshWingsInAir()
        {
            yield return new WaitUntil(() => doubleJumpCount == 0 || !InputHandler.Instance.inputActions.jump.IsPressed);
            if (doubleJumpCount != 0)
            {
                ReflectionHelper.SetField(HeroController.instance, "doubleJumped", false);
            }
        }

        // Apparently, these are all the places where the game refreshes the player's wings; we need to set the doubleJumpCount to 0
        #region Restore Double Jump
        private static void AddRefreshHooks()
        {
            On.HeroController.BackOnGround += HeroController_BackOnGround;
            On.HeroController.Bounce += HeroController_Bounce;
            On.HeroController.BounceHigh += HeroController_BounceHigh;
            On.HeroController.DoWallJump += HeroController_DoWallJump;
            On.HeroController.EnterScene += HeroController_EnterScene;
            On.HeroController.EnterSceneDreamGate += HeroController_EnterSceneDreamGate;
            On.HeroController.ExitAcid += HeroController_ExitAcid;
            On.HeroController.HazardRespawn += HeroController_HazardRespawn;
            On.HeroController.ResetAirMoves += HeroController_ResetAirMoves;
            On.HeroController.Respawn += HeroController_Respawn;
            On.HeroController.ShroomBounce += HeroController_ShroomBounce;

            On.HeroController.LookForInput += HeroController_LookForInput;
            On.HeroController.Update += HeroController_Update;
        }
        private static void RemoveRefreshHooks()
        {
            On.HeroController.BackOnGround -= HeroController_BackOnGround;
            On.HeroController.Bounce -= HeroController_Bounce;
            On.HeroController.BounceHigh -= HeroController_BounceHigh;
            On.HeroController.DoWallJump -= HeroController_DoWallJump;
            On.HeroController.EnterScene -= HeroController_EnterScene;
            On.HeroController.EnterSceneDreamGate -= HeroController_EnterSceneDreamGate;
            On.HeroController.ExitAcid -= HeroController_ExitAcid;
            On.HeroController.HazardRespawn -= HeroController_HazardRespawn;
            On.HeroController.ResetAirMoves -= HeroController_ResetAirMoves;
            On.HeroController.Respawn -= HeroController_Respawn;
            On.HeroController.ShroomBounce -= HeroController_ShroomBounce;

            On.HeroController.LookForInput -= HeroController_LookForInput;
            On.HeroController.Update -= HeroController_Update;
        }

        private static void HeroController_BackOnGround(On.HeroController.orig_BackOnGround orig, HeroController self)
        {
            orig(self);
            RefreshDoubleJump();
        }
        private static void HeroController_Bounce(On.HeroController.orig_Bounce orig, HeroController self)
        {
            orig(self);
            RefreshDoubleJump();
        }
        private static void HeroController_BounceHigh(On.HeroController.orig_BounceHigh orig, HeroController self)
        {
            orig(self);
            RefreshDoubleJump();
        }
        private static void HeroController_DoWallJump(On.HeroController.orig_DoWallJump orig, HeroController self)
        {
            orig(self);
            RefreshDoubleJump();
        }
        private static void HeroController_EnterSceneDreamGate(On.HeroController.orig_EnterSceneDreamGate orig, HeroController self)
        {
            orig(self);
            RefreshDoubleJump();
        }
        private static void HeroController_ExitAcid(On.HeroController.orig_ExitAcid orig, HeroController self)
        {
            orig(self);
            RefreshDoubleJump();
        }
        private static void HeroController_ResetAirMoves(On.HeroController.orig_ResetAirMoves orig, HeroController self)
        {
            orig(self);
            RefreshDoubleJump();
        }
        private static void HeroController_ShroomBounce(On.HeroController.orig_ShroomBounce orig, HeroController self)
        {
            orig(self);
            RefreshDoubleJump();
        }

        private static System.Collections.IEnumerator HeroController_EnterScene(On.HeroController.orig_EnterScene orig, HeroController self, TransitionPoint enterGate, float delayBeforeEnter)
        {
            yield return orig(self, enterGate, delayBeforeEnter);
            RefreshDoubleJump();
        }
        private static System.Collections.IEnumerator HeroController_HazardRespawn(On.HeroController.orig_HazardRespawn orig, HeroController self)
        {
            yield return orig(self);
            RefreshDoubleJump();
        }
        private static System.Collections.IEnumerator HeroController_Respawn(On.HeroController.orig_Respawn orig, HeroController self)
        {
            yield return orig(self);
            RefreshDoubleJump();
        }
        private static void HeroController_LookForInput(On.HeroController.orig_LookForInput orig, HeroController self)
        {
            orig(self);

            // For some reson, this function is private, and I'd rather copy and paste the code rather than reflect
            bool canWallSlide = (self.cState.wallSliding && GameManager.instance.isPaused) || (!self.cState.touchingNonSlider && !self.inAcid
                && !self.cState.dashing && self.playerData.GetBool("hasWalljump") && !self.cState.onGround && !self.cState.recoiling
                && !GameManager.instance.isPaused && !self.controlReqlinquished && !self.cState.transitioning
                && (self.cState.falling || self.cState.wallSliding) && !self.cState.doubleJumping && self.CanInput());

            if (PlayerData.instance.GetBool(nameof(PlayerData.hasWalljump)) && canWallSlide && !self.cState.attacking)
            {
                if (self.touchingWallL && InputHandler.Instance.inputActions.left.IsPressed && !self.cState.wallSliding)
                {
                    RefreshDoubleJump();
                }
                if (self.touchingWallR && InputHandler.Instance.inputActions.right.IsPressed && !self.cState.wallSliding)
                {
                    RefreshDoubleJump();
                }
            }
        }
        private static void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);

            if (self.cState.wallSliding)
            {
                RefreshDoubleJump();
            }
        }
        #endregion
    }
}
