using System.Collections.Generic;
using System.Linq;

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
        public MainSkillUpgradeRandoType MainSetting = MainSkillUpgradeRandoType.None;

        [Newtonsoft.Json.JsonIgnore]
        public bool Any => MainSetting != MainSkillUpgradeRandoType.None;
    }
}
