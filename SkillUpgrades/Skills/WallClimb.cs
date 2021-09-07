using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SkillUpgrades.Util;

namespace SkillUpgrades.Skills
{
    internal static class WallClimb
    {
        private static bool climbEnabled => SkillUpgrades.globalSettings.GlobalToggle && SkillUpgrades.globalSettings.WallClimb;
        internal static void Hook()
        {
            On.HeroController.Start += SetWallslideSpeed;
            On.HeroController.Update += CheckClimb;
        }

        private static void SetWallslideSpeed(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);
            self.WALLSLIDE_SPEED = 0f;
        }

        private static void CheckClimb(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
        }
    }
}
