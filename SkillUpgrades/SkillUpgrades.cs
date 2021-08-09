using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;

namespace SkillUpgrades
{
    public class SkillUpgrades : Mod, IGlobalSettings<GlobalSettings>, IMenuMod
    {
        internal static SkillUpgrades instance;

        #region Global Settings
        public static GlobalSettings globalSettings { get; set; } = new GlobalSettings();
        public void OnLoadGlobal(GlobalSettings s) => globalSettings = s;
        public GlobalSettings OnSaveGlobal() => globalSettings;
        #endregion

        public override void Initialize()
        {
            instance = this;
            instance.Log("Initializing");

            Skills.Skills.HookSkillUpgrades();
        }

        #region Menu

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            return new List<IMenuMod.MenuEntry>()
            {
                new IMenuMod.MenuEntry
                {
                    Name = "Multiple Wings",
                    Description = "Toggle whether wings can be used more than once before landing.",
                    Values = new string[]{ "On", "Off" },
                    Saver = opt => globalSettings.TripleJumpEnabled = opt==0,
                    Loader = () => globalSettings.TripleJumpEnabled ? 0 : 1,
                },
                new IMenuMod.MenuEntry
                {
                    Name = "Multiple Air Dash",
                    Description = "Toggle whether dash can be used more than once before landing.",
                    Values = new string[]{ "On", "Off" },
                    Saver = opt => globalSettings.BonusAirDashEnabled = opt==0,
                    Loader = () => globalSettings.BonusAirDashEnabled ? 0 : 1,
                },
                new IMenuMod.MenuEntry
                {
                    Name = "Vertical Superdash",
                    Description = "Toggle whether Crystal Heart can be used in non-horizontal directions.",
                    Values = new string[]{ "On", "Off" },
                    Saver = opt => globalSettings.VerticalSuperdashEnabled = opt==0,
                    Loader = () => globalSettings.VerticalSuperdashEnabled ? 0 : 1,
                },
                new IMenuMod.MenuEntry
                {
                    Name = "Horizontal Dive",
                    Description = "Toggle whether Desolate Dive can be used horizontally.",
                    Values = new string[]{ "On", "Off" },
                    Saver = opt => globalSettings.HorizontalDiveEnbled = opt==0,
                    Loader = () => globalSettings.HorizontalDiveEnbled ? 0 : 1,
                },
            };
        }

        public bool ToggleButtonInsideMenu => false;
        #endregion

        public override string GetVersion()
        {
            return "0.1";
        }

    }
}
