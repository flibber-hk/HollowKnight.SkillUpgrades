using System;
using System.Collections.Generic;
using System.Linq;
using ItemChanger;
using ItemChanger.Modules;
using Newtonsoft.Json;
using SkillUpgrades.Skills;

namespace SkillUpgrades.IC.Items
{
    /// <summary>
    /// Module which manages unlocked skill upgrades.
    /// </summary>
    public class SkillUpgradeUnlockModule : Module
    {
        [JsonProperty]
        private Dictionary<string, bool?> ManagedSkills = new();

        public override void Initialize()
        {
            foreach (var kvp in ManagedSkills)
            {
                AbstractSkillUpgrade.OverrideSkillState(kvp.Key, kvp.Value);
            }
        }

        public override void Unload()
        {
            foreach (var kvp in ManagedSkills)
            {
                AbstractSkillUpgrade.OverrideSkillState(kvp.Key, null);
            }
        }

        public void UnlockSkill(string skillName, bool allowToggle)
        {
            if (!allowToggle)
            {
                AbstractSkillUpgrade.OverrideSkillState(skillName, true);
                ManagedSkills[skillName] = true;
            }
            else
            {
                AbstractSkillUpgrade.OverrideSkillState(skillName, null);
                ManagedSkills[skillName] = null;
            }
        }

        public void RegisterSkill(string skillName)
        {
            if (!ManagedSkills.ContainsKey(skillName))
            {
                AbstractSkillUpgrade.OverrideSkillState(skillName, false);
                ManagedSkills.Add(skillName, false);
            }
        }
    }
}
