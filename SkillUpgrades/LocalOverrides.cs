using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SkillUpgrades
{
    [PublicAPI]
    public static class LocalOverrides
    {
        /// <summary>
        /// Set the value of whether a skill is active.
        /// </summary>
        /// <param name="skillName">The Name of the skill to set (note - not the type name)</param>
        /// <param name="set">True or False to enable or disable the skill, null to revert to the global setting.</param>
        /// <returns>True if the skill was loaded, false otherwise.</returns>
        public static bool SetSkill(string skillName, bool? set)
        {
            return SkillUpgrades.localSettings.SetSkill(skillName, set);
        }

        /// <summary>
        /// Set the value of an int field on a skill
        /// </summary>
        /// <param name="skillName">The Name of the skill (note - not the type name)</param>
        /// <param name="intName">The name of the int</param>
        /// <param name="set">The value of the int to set it to; null to remove the override</param>
        public static void SetInt(string skillName, string intName, int? set)
        {
            SkillUpgrades.localSettings.SetInt(skillName, intName, set);
        }
        /// <summary>
        /// Set the value of a bool field on a skill
        /// </summary>
        /// <param name="skillName">The Name of the skill (note - not the type name)</param>
        /// <param name="boolName">The name of the bool</param>
        /// <param name="set">The value of the bool to set it to; null to remove the override</param>
        public static void SetBool(string skillName, string boolName, bool? set)
        {
            SkillUpgrades.localSettings.SetBool(skillName, boolName, set);
        }
        /// <summary>
        /// Set the value of a float field on a skill
        /// </summary>
        /// <param name="skillName">The Name of the skill (note - not the type name)</param>
        /// <param name="floatName">The name of the float</param>
        /// <param name="set">The value of the float to set it to; null to remove the override</param>
        public static void SetFloat(string skillName, string floatName, float? set)
        {
            SkillUpgrades.localSettings.SetFloat(skillName, floatName, set);
        }

        /// <summary>
        /// Get a list of all loaded skills by name
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSkillNames => SkillUpgrades._skills.Keys.ToList();
    }
}
