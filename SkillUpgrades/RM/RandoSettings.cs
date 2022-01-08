using System.Collections.Generic;
using System.Linq;

namespace SkillUpgrades.RM
{
    public class RandoSettings
    {
        public Dictionary<string, bool> SkillSettings = SkillUpgrades.GS.EnabledSkills.ToDictionary(kvp => kvp.Key, kvp => false);

        [Newtonsoft.Json.JsonIgnore]
        public bool Any => SkillSettings.Values.Any();
    }
}
