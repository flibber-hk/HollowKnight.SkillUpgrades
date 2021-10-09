namespace SkillUpgrades.Skills
{
    public abstract partial class AbstractSkillUpgrade
    {
        /// <summary>
        /// Initialize the skill upgrade
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Unload the skill upgrade
        /// </summary>
        public virtual void Unload() { }

        public abstract string UIName { get; }
        public string Name { get; protected set; }
        public abstract string Description { get; }


        public virtual bool InvolvesHeroRotation => false;

        public bool IsUnloadable => GetType().GetMethod(nameof(AbstractSkillUpgrade.Unload))?.DeclaringType != typeof(AbstractSkillUpgrade);
        /// <summary>
        /// Initialize the skill upgrade if it was properly unloaded earlier
        /// </summary>
        public void ReInitialize() { if (IsUnloadable) Initialize(); }

        public bool SkillUpgradeActive { get; set; } = true;
    }
}
