using System.Collections.Generic;
using JetBrains.Annotations;

namespace SkillUpgrades
{
    [PublicAPI]
    public class SkillUpgradeSaveData
    {
        public Dictionary<string, bool> EnabledSkills { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, bool> Booleans { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, float> Floats { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, int> Integers { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Call this method in the OnLoadLocal of your mod class to apply the overrides
        /// </summary>
        /// <param name="overwrite">If this is set to true, clear the original save data</param>
        public void Apply(bool overwrite = false)
        {
            if (overwrite)
            {
                SettingsOverrides.EnabledSkills.Clear();
                foreach (string name in SkillUpgrades._skills.Keys)
                {
                    SkillUpgrades.UpdateSkillState(name);
                }

                SettingsOverrides.Booleans.Clear();
                SettingsOverrides.Integers.Clear();
                SettingsOverrides.Floats.Clear();
            }

            foreach (var kvp in EnabledSkills)
            {
                SettingsOverrides.TrySetSkill(kvp.Key, kvp.Value);
            }
            foreach (var kvp in Booleans)
            {
                SettingsOverrides.SetBool(kvp.Key, kvp.Value);
            }
            foreach (var kvp in Integers)
            {
                SettingsOverrides.SetInt(kvp.Key, kvp.Value);
            }
            foreach (var kvp in Floats)
            {
                SettingsOverrides.SetFloat(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Set the value of an int field on a skill
        /// </summary>
        /// <param name="skillName">The Name of the skill</param>
        /// <param name="intName">The name of the int</param>
        /// <param name="set">The value of the int to set it to; null to remove the override</param>
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

            SettingsOverrides.SetInt(key, set);
        }
        /// <summary>
        /// Set the value of a bool field on a skill
        /// </summary>
        /// <param name="skillName">The Name of the skill</param>
        /// <param name="boolName">The name of the bool</param>
        /// <param name="set">The value of the bool to set it to; null to remove the override</param>
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

            SettingsOverrides.SetBool(key, set);
        }
        /// <summary>
        /// Set the value of a float field on a skill
        /// </summary>
        /// <param name="skillName">The Name of the skill</param>
        /// <param name="floatName">The name of the float</param>
        /// <param name="set">The value of the float to set it to; null to remove the override</param>
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

            SettingsOverrides.SetFloat(key, set);
        }

        /// <summary>
        /// Set the value of whether a skill is active.
        /// </summary>
        /// <param name="skillName">The Name of the skill to set</param>
        /// <param name="set">True or False to enable or disable the skill, null to revert to the global setting.</param>
        /// <returns>True if the skill was loaded, false otherwise.</returns>
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

            SettingsOverrides.TrySetSkill(skillName, set);
        }

    }
}
