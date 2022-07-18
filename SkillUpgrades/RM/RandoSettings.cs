using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SkillUpgrades.RM
{
    public enum MainSkillUpgradeRandoType
    {
        None,
        All,
        RandomSelection,
        EnabledSkills,
    }

    public class RandoSettings
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MainSkillUpgradeRandoType SkillUpgradeRandomization = MainSkillUpgradeRandoType.None;

        [JsonIgnore]
        public bool Any => SkillUpgradeRandomization != MainSkillUpgradeRandoType.None;
    }
}
