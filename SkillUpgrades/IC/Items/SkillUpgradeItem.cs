using ItemChanger;

namespace SkillUpgrades.IC.Items
{
    /// <summary>
    /// Item which unlocks the named skill upgrade
    /// </summary>
    public class SkillUpgradeItem : AbstractItem
    {
        public string SkillName;
        /// <summary>
        /// If this is true, unlocking the skill will set its state to null
        /// </summary>
        public bool AllowToggle;

        protected override void OnLoad()
        {
            ItemChangerMod.Modules.GetOrAdd<SkillUpgradeUnlockModule>().RegisterSkill(SkillName);
        }

        public override void GiveImmediate(GiveInfo info)
        {
            ItemChangerMod.Modules.Get<SkillUpgradeUnlockModule>().UnlockSkill(SkillName, AllowToggle);
        }
    }
}
