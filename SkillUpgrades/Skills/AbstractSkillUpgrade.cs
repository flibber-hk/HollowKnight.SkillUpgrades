using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public bool skillUpgradeActive = true;


        #region Logging
        // Why must I do it like this :hivescream:
        protected AbstractSkillUpgrade()
        {
            ReflectionHelper.SetField<Loggable, string>(this, "ClassName", $"SkillUpgrades]:[{GetType().Name}");
        }
        #endregion
    }
}
