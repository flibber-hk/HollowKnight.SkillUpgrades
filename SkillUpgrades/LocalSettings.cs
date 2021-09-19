using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillUpgrades
{
    [Serializable]
    public class LocalSettings
    {
        public Dictionary<string, bool?> EnabledSkills { get; set; } = new Dictionary<string, bool?>();

        /*
        public Dictionary<string, bool?> Booleans { get; set; } = new Dictionary<string, bool?>();
        public Dictionary<string, float?> Floats { get; set; } = new Dictionary<string, float?>();
        public Dictionary<string, int?> Integers { get; set; } = new Dictionary<string, int?>();
        */


        public LocalSettings()
        {
            foreach (string skill in SkillUpgrades.globalSettings.EnabledSkills.Keys)
            {
                EnabledSkills.Add(skill, null);
            }
        }

        /// <summary>
        /// Set the value of whether a skill is active.
        /// </summary>
        /// <param name="skillName">The Name of the skill to set (note - not the class name)</param>
        /// <param name="set">True or False to enable or disable the skill, null to revert to the global setting.</param>
        /// <returns>True if the skill was loaded, false otherwise.</returns>
        public bool SetSkill(string skillName, bool set)
        {
            if (!EnabledSkills.ContainsKey(skillName))
            {
                SkillUpgrades.instance.LogError($"SetSkill: Unable to find skill: {skillName}");
                return false;
            }

            if (SkillUpgrades.globalSettings.EnabledSkills[skillName] == null)
            {
                EnabledSkills[skillName] = set;
                SkillUpgrades.instance.LogWarn($"SetSkill: Skill not loaded: {skillName}");
                return false;
            }

            EnabledSkills[skillName] = set;
            SkillUpgrades.UpdateSkillState(skillName);

            return true;
        }
    }
}
