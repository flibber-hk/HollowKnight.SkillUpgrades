using FStats;
using FStats.Util;
using SkillUpgrades.Skills;
using SkillUpgrades.Util;
using System.Collections.Generic;
using System.Linq;

namespace SkillUpgrades.Stats
{
    public class SkillUpgradeStats : StatController
    {
        public Dictionary<string, int> SkillUpgradeUsageCount = new();

        public override void Initialize()
        {
            AbstractSkillUpgrade.OnUsedSkillUpgrade += RecordSkillUsages;
        }

        public override void Unload()
        {
            AbstractSkillUpgrade.OnUsedSkillUpgrade -= RecordSkillUsages;
        }

        private void RecordSkillUsages(string message)
        {
            SkillUpgradeUsageCount.IncrementValue(message);
        }

        public IEnumerable<DisplayInfo> GetDisplayInfosBoth()
        {
            if (SkillUpgradeUsageCount.Values.Sum() == 0) return Enumerable.Empty<DisplayInfo>();

            DisplayInfo template = new()
            {
                Title = "Skill Upgrade Usage" + SaveFileCountString(),
                Priority = BuiltinScreenPriorityValues.DirectionalStats + 100f,
            };

            List<string> entries = SkillUpgradeUsageCount
                .Where(kvp => kvp.Value != 0)
                .OrderBy(kvp => kvp.Key, System.StringComparer.InvariantCultureIgnoreCase)
                .Select(kvp => $"{kvp.Key.FromCamelCase()}: {kvp.Value}")
                .ToList();

            return ColumnUtility.CreateDisplay(
                template: template,
                entries: entries);
        }

        public override IEnumerable<DisplayInfo> GetGlobalDisplayInfos()
        {
            return GetDisplayInfosBoth();
        }

        public override IEnumerable<DisplayInfo> GetDisplayInfos()
        {
            return GetDisplayInfosBoth();
        }
    }
}
