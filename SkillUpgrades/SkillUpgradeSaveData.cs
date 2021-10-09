using System.Collections.Generic;

namespace SkillUpgrades
{
    public class SkillUpgradeSaveData
    {
        public Dictionary<string, bool> EnabledSkills { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, bool> Booleans { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, float> Floats { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, int> Integers { get; set; } = new Dictionary<string, int>();

        public void SetInt(string skillName, string intName, int? set)
        {
            string key = SkillUpgradeSettings.GetKey(skillName, intName);

            if (set == null)
            {
                Integers.Remove(key);
            }
            else
            {
                Integers[key] = set ?? default;
            }
        }
        public void SetBool(string skillName, string boolName, bool? set)
        {
            string key = SkillUpgradeSettings.GetKey(skillName, boolName);

            if (set == null)
            {
                Booleans.Remove(key);
            }
            else
            {
                Booleans[key] = set ?? default;
            }
        }
        public void SetFloat(string skillName, string floatName, float? set)
        {
            string key = SkillUpgradeSettings.GetKey(skillName, floatName);

            if (set == null)
            {
                Floats.Remove(key);
            }
            else
            {
                Floats[key] = set ?? default;
            }
        }

        public void SetSkill(string skillName, bool? set)
        {
            if (set == null)
            {
                EnabledSkills.Remove(skillName);
            }
            else
            {
                EnabledSkills[skillName] = set ?? default;
            }

            SkillUpgrades.UpdateSkillState(skillName);
        }

    }
}
