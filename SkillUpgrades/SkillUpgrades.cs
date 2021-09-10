using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            List<IMenuMod.MenuEntry> menuEntries = new List<IMenuMod.MenuEntry>();

            if (globalSettings.GlobalToggle == null) return menuEntries;

            foreach (FieldInfo fi in typeof(GlobalSettings).GetFields())
            {
                if (!(fi.GetCustomAttribute<MenuToggleable>() is MenuToggleable mt))
                {
                    continue;
                }
                if ((bool?)fi.GetValue(globalSettings) == null)
                {
                    continue;
                }
                menuEntries.Add(new IMenuMod.MenuEntry()
                {
                    Name = mt.name,
                    Description = mt.description,
                    Values = new string[] { "On", "Off" },
                    Saver = opt => { fi.SetValue(globalSettings, (bool?)(opt == 0)); },
                    Loader = () => (bool)fi.GetValue(globalSettings) ? 0 : 1
                });
            }

            return menuEntries;
        }

        public bool ToggleButtonInsideMenu => false;
        #endregion

        public override string GetVersion()
        {
            return "0.2";
        }

    }
}
