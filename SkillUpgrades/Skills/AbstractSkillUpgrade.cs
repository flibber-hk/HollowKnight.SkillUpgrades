using Modding;

namespace SkillUpgrades.Skills
{
    public abstract class AbstractSkillUpgrade : Loggable
    {
        /// <summary>
        /// Initialize the skill upgrade
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Unload the skill upgrade
        /// </summary>
        public virtual void Unload() { }

        public abstract string Name { get; }
        public abstract string Description { get; }


        public virtual bool InvolvesHeroRotation => false;

        public bool IsUnloadable => GetType().GetMethod(nameof(AbstractSkillUpgrade.Unload))?.DeclaringType != typeof(AbstractSkillUpgrade);
        /// <summary>
        /// Initialize the skill upgrade if it was properly unloaded earlier
        /// </summary>
        public void ReInitialize() { if (IsUnloadable) Initialize(); }

        protected internal bool skillUpgradeActive = true;

        #region Adjustable Fields
        protected bool GetBool(string name, bool @default)
        {
            string key = SkillUpgradeSettings.GetKey(Name, name);

            if (SkillUpgrades.localSettings.Booleans.TryGetValue(key, out bool ret)) return ret;
            else if (SkillUpgrades.globalSettings.Booleans.TryGetValue(key, out ret)) return ret;

            SkillUpgrades.globalSettings.Booleans[key] = @default; return @default;
        }
        protected int GetInt(string name, int @default)
        {
            string key = SkillUpgradeSettings.GetKey(Name, name);

            if (SkillUpgrades.localSettings.Integers.TryGetValue(key, out int ret)) return ret;
            else if (SkillUpgrades.globalSettings.Integers.TryGetValue(key, out ret)) return ret;

            SkillUpgrades.globalSettings.Integers[key] = @default; return @default;
        }
        protected float GetFloat(string name, float @default)
        {
            string key = SkillUpgradeSettings.GetKey(Name, name);

            if (SkillUpgrades.localSettings.Floats.TryGetValue(key, out float ret)) return ret;
            else if (SkillUpgrades.globalSettings.Floats.TryGetValue(key, out ret)) return ret;

            SkillUpgrades.globalSettings.Floats[key] = @default; return @default;
        }
        #endregion

        #region Logging
        // It would be nice to be able to inherit from Loggable (or SimpleLogger IG) and be able to set the prefix, but we can't, so here we are
        protected AbstractSkillUpgrade()
        {
            ReflectionHelper.SetField<Loggable, string>(this, "ClassName", $"SkillUpgrades]:[{GetType().Name}");
        }
        #endregion
    }
}
