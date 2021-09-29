using System;

namespace SkillUpgrades
{
    [Serializable]
    public class LocalSettings : SkillUpgradeSettings
    {
        internal bool SetSkill(string skillName, bool? set)
        {
            EnabledSkills[skillName] = set;

            if (SkillUpgrades.globalSettings.EnabledSkills[skillName] == null)
            {
                SkillUpgrades.instance.LogWarn($"SetSkill: Skill not loaded: {skillName}");
                return false;
            }

            SkillUpgrades.UpdateSkillState(skillName);

            return true;
        }
        internal void SetInt(string skillName, string intName, int? set)
        {
            string key = GetKey(skillName, intName);

            if (set == null)
            {
                Integers.Remove(key);
            }
            else
            {
                Integers[key] = (int)set;
            }
        }
        internal void SetBool(string skillName, string boolName, bool? set)
        {
            string key = GetKey(skillName, boolName);

            if (set == null)
            {
                Booleans.Remove(key);
            }
            else
            {
                Booleans[key] = (bool)set;
            }
        }
        internal void SetFloat(string skillName, string floatName, float? set)
        {
            string key = GetKey(skillName, floatName);

            if (set == null)
            {
                Floats.Remove(key);
            }
            else
            {
                Floats[key] = (float)set;
            }
        }
    }
}
