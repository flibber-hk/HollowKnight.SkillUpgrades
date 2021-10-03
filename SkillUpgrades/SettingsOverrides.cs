using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace SkillUpgrades
{
    [PublicAPI]
    public static class SettingsOverrides
    {
        internal static Dictionary<string, bool> SkillLoadOverrides { get; set; } = new Dictionary<string, bool>();
        internal static Dictionary<string, bool> EnabledSkills { get; set; } = new Dictionary<string, bool>();
        internal static Dictionary<string, bool> Booleans { get; set; } = new Dictionary<string, bool>();
        internal static Dictionary<string, float> Floats { get; set; } = new Dictionary<string, float>();
        internal static Dictionary<string, int> Integers { get; set; } = new Dictionary<string, int>();

        public static bool AlreadyLoadedSkills { get; internal set; } = false;

        /// <summary>
        /// Set the value of whether a skill is active.
        /// </summary>
        /// <param name="skillName">The Name of the skill to set (note - not the type name)</param>
        /// <param name="set">True or False to enable or disable the skill, null to revert to the global setting.</param>
        /// <returns>True if the skill was loaded, false otherwise.</returns>
        public static bool TrySetSkill(string skillName, bool? set)
        {
            if (!SkillUpgrades.globalSettings.EnabledSkills.TryGetValue(skillName, out bool? value))
            {
                SkillUpgrades.instance.LogWarn($"TrySetSkill: Skill not found: {skillName}");
                return false;
            }
            else if (value == null)
            {
                SkillUpgrades.instance.LogWarn($"TrySetSkill: Skill not loaded: {skillName}");
                return false;
            }

            if (set == null)
            {
                EnabledSkills.Remove(skillName);
            }
            else
            {
                EnabledSkills[skillName] = (bool)set;
            }
            SkillUpgrades.UpdateSkillState(skillName);

            return true;
        }

        /// <summary>
        /// Set the value of an int field on a skill
        /// </summary>
        /// <param name="skillName">The Name of the skill (note - not the type name)</param>
        /// <param name="intName">The name of the int</param>
        /// <param name="set">The value of the int to set it to; null to remove the override</param>
        public static void SetInt(string skillName, string intName, int? set)
        {
            string key = SkillUpgradeSettings.GetKey(skillName, intName);

            if (set == null)
            {
                Integers.Remove(key);
            }
            else
            {
                Integers[key] = (int)set;
            }
        }
        /// <summary>
        /// Set the value of a bool field on a skill
        /// </summary>
        /// <param name="skillName">The Name of the skill (note - not the type name)</param>
        /// <param name="boolName">The name of the bool</param>
        /// <param name="set">The value of the bool to set it to; null to remove the override</param>
        public static void SetBool(string skillName, string boolName, bool? set)
        {
            string key = SkillUpgradeSettings.GetKey(skillName, boolName);

            if (set == null)
            {
                Booleans.Remove(key);
            }
            else
            {
                Booleans[key] = (bool)set;
            }
        }
        /// <summary>
        /// Set the value of a float field on a skill
        /// </summary>
        /// <param name="skillName">The Name of the skill (note - not the type name)</param>
        /// <param name="floatName">The name of the float</param>
        /// <param name="set">The value of the float to set it to; null to remove the override</param>
        public static void SetFloat(string skillName, string floatName, float? set)
        {
            string key = SkillUpgradeSettings.GetKey(skillName, floatName);

            if (set == null)
            {
                Floats.Remove(key);
            }
            else
            {
                Floats[key] = (float)set;
            }
        }

        /// <summary>
        /// Sets the values of all ints in the dictionary (keys given by (skillName, intName) pairs)
        /// </summary>
        /// <param name="intValues">
        /// Key: (skillName, intName)
        /// Value: The value of the int to set it to; null to remove the override
        /// </param>
        public static void SetInts(Dictionary<(string, string), int?> intValues)
        {
            foreach (var kvp in intValues)
            {
                SetInt(kvp.Key.Item1, kvp.Key.Item2, kvp.Value);
            }
        }
        /// <summary>
        /// Sets the values of all bools in the dictionary (keys given by (skillName, boolName) pairs)
        /// </summary>
        /// <param name="boolValues">
        /// Key: (skillName, boolName)
        /// Value: The value of the bool to set it to; null to remove the override
        /// </param>
        public static void SetBools(Dictionary<(string, string), bool?> boolValues)
        {
            foreach (var kvp in boolValues)
            {
                SetBool(kvp.Key.Item1, kvp.Key.Item2, kvp.Value);
            }
        }
        /// <summary>
        /// Sets the values of all floats in the dictionary (keys given by (skillName, floatName) pairs)
        /// </summary>
        /// <param name="floatValues">
        /// Key: (skillName, floatName)
        /// Value: The value of the float to set it to; null to remove the override
        /// </param>
        public static void SetFloats(Dictionary<(string, string), float?> floatValues)
        {
            foreach (var kvp in floatValues)
            {
                SetFloat(kvp.Key.Item1, kvp.Key.Item2, kvp.Value);
            }
        }

        /// <summary>
        /// Force a skill to initialize, or not to
        /// </summary>
        /// <param name="name">The name of the skill</param>
        /// <param name="initialize">Whether or not it should be initialized</param>
        public static void SetSkillLoadState(string name, bool initialize)
        {
            SkillLoadOverrides[name] = initialize;
            if (initialize)
            {
                EnabledSkills[name] = true;
            }
        }

        /// <summary>
        /// Get a list of all loaded skills by name
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSkillNames => SkillUpgrades._skills.Keys.ToList();
    }
}
