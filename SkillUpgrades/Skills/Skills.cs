using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkillUpgrades.Skills
{
    public static class Skills
    {
        public static void HookSkillUpgrades()
        {
            if (SkillUpgrades.globalSettings.GlobalToggle == null) return;

            if (SkillUpgrades.globalSettings.TripleJumpEnabled != null) TripleJump.Hook();
            if (SkillUpgrades.globalSettings.BonusAirDashEnabled != null) BonusDash.Hook();
            if (SkillUpgrades.globalSettings.DirectionalDashEnabled != null) DirectionalDash.Hook();
            if (SkillUpgrades.globalSettings.VerticalSuperdashEnabled != null) VerticalSuperdash.Hook();
            if (SkillUpgrades.globalSettings.HorizontalDiveEnabled != null) HorizontalQuake.Hook();
            if (SkillUpgrades.globalSettings.SpiralScreamEnabled != null) SpiralScream.Hook();
            if (SkillUpgrades.globalSettings.DownwardFireballEnabled != null) DownwardFireball.Hook();
            if (SkillUpgrades.globalSettings.WallClimbEnabled != null) WallClimb.Hook();
        }
    }
}
