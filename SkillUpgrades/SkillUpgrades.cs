using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;

namespace SkillUpgrades
{
    public class SkillUpgrades : Mod
    {
        internal static SkillUpgrades instance;

        public override void Initialize()
        {
            instance = this;
            instance.Log("Initializing");


        }

        public override string GetVersion()
        {
            return "0.1";
        }

    }
}
