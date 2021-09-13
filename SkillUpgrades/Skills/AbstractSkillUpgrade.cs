using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillUpgrades.Skills
{
    public abstract class AbstractSkillUpgrade
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

        public bool skillUpgradeActive = true;
    }
}
