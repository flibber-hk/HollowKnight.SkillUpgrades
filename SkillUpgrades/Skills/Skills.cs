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
            TripleJump.Hook();
            BonusDash.Hook();
            DirectionalDash.Hook();
            VerticalSuperdash.Hook();
            HorizontalQuake.Hook();
            SpiralScream.Hook();
            DownwardFireball.Hook();
            WallClimb.Hook();
        }

    }
}
