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
        public GlobalSettings globalSettings { get; set; } = new GlobalSettings();
        public override ModSettings GlobalSettings
        {
            get => globalSettings = globalSettings ?? new GlobalSettings();
            set => globalSettings = value is GlobalSettings gSettings ? gSettings : globalSettings;
        }


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
