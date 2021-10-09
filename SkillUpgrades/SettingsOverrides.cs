using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace SkillUpgrades
{
    [PublicAPI]
    public static class SettingsOverrides
    {
        internal static Dictionary<string, bool> SkillLoadOverrides { get; set; } = new Dictionary<string, bool>();
        public static bool AlreadyLoadedSkills { get; internal set; } = false;

        /// <summary>
        /// Set the value of whether a skill is active.
        /// </summary>
        /// <param name="skillName">The Name of the skill to set</param>
        /// <param name="set">True or False to enable or disable the skill, null to revert to the global setting.</param>
        public static void SetSkill(string skillName, bool? set)
        {
            SkillUpgrades.localOverrides.SetSkill(skillName, set);
        }

        /// <summary>
        /// Set the value of an int field on a skill
        /// </summary>
        /// <param name="skillName">The Name of the skill</param>
        /// <param name="intName">The name of the int</param>
        /// <param name="set">The value of the int to set it to; null to remove the override</param>
        public static void SetInt(string skillName, string intName, int? set)
        {
            SkillUpgrades.localOverrides.SetInt(skillName, intName, set);
        }
        /// <summary>
        /// Set the value of a bool field on a skill
        /// </summary>
        /// <param name="skillName">The Name of the skill</param>
        /// <param name="boolName">The name of the bool</param>
        /// <param name="set">The value of the bool to set it to; null to remove the override</param>
        public static void SetBool(string skillName, string boolName, bool? set)
        {
            SkillUpgrades.localOverrides.SetBool(skillName, boolName, set);
        }
        /// <summary>
        /// Set the value of a float field on a skill
        /// </summary>
        /// <param name="skillName">The Name of the skill</param>
        /// <param name="floatName">The name of the float</param>
        /// <param name="set">The value of the float to set it to; null to remove the override</param>
        public static void SetFloat(string skillName, string floatName, float? set)
        {
            SkillUpgrades.localOverrides.SetFloat(skillName, floatName, set);
        }


        /// <summary>
        /// Force a skill to initialize, or not to. Must be run before SkillUpgrades is initialized.
        /// </summary>
        /// <param name="name">The name of the skill</param>
        /// <param name="initialize">Whether or not it should be initialized</param>
        /// <returns>Whether or not the set was successful</returns>
        public static bool TrySetSkillLoadState(string name, bool initialize)
        {
            if (AlreadyLoadedSkills) return false;

            SkillLoadOverrides[name] = initialize;
            return true;
        }

        /// <summary>
        /// Get a list of all loaded skills by name
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSkillNames() => SkillUpgrades._skills.Keys.ToList();
    }
}
