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

        /// <summary>
        /// The name to show in the menu
        /// </summary>
        public abstract string UIName { get; }
        /// <summary>
        /// The name of the skill (by default, the type name)
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// The description to show in the menu
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// If this is true, ensures that the HeroRotation module is loaded, allowing the knight to rotate without moving its hitbox
        /// </summary>
        public virtual bool InvolvesHeroRotation => false;

        public bool IsUnloadable => GetType().GetMethod(nameof(AbstractSkillUpgrade.Unload))?.DeclaringType != typeof(AbstractSkillUpgrade);
        /// <summary>
        /// Initialize the skill upgrade if it was properly unloaded earlier
        /// </summary>
        public void ReInitialize() { if (IsUnloadable) Initialize(); }

        public bool SkillUpgradeActive { get; set; } = true;
    }
}
