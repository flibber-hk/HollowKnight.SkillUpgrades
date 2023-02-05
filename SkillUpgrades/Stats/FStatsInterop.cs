using Modding;

namespace SkillUpgrades.Stats
{
    internal static class FStatsInterop
    {
        public static void HookFStats()
        {
            if (ModHooks.GetMod(nameof(FStats.FStatsMod)) is null) return;

            HookFStatsInternal();
        }

        private static void HookFStatsInternal()
        {
            FStats.API.OnGenerateFile += r => r(new SkillUpgradeStats());
            FStats.API.RegisterGlobalStat<SkillUpgradeStats>();
        }
    }
}
