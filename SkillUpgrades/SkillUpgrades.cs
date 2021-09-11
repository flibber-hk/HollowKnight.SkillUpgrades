using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Modding;
using SkillUpgrades.Skills;

namespace SkillUpgrades
{
    public class SkillUpgrades : Mod, IGlobalSettings<GlobalSettings>, IMenuMod
    {
        internal static SkillUpgrades instance;
        private static readonly Dictionary<string, AbstractSkillUpgrade> _skills = new Dictionary<string, AbstractSkillUpgrade>();

        #region Global Settings
        public static GlobalSettings globalSettings { get; set; } = new GlobalSettings();
        public void OnLoadGlobal(GlobalSettings s) => globalSettings = s;
        public GlobalSettings OnSaveGlobal() => globalSettings;
        #endregion

        public override void Initialize()
        {
            instance = this;
            instance.Log("Initializing");

            foreach (Type t in Assembly.GetAssembly(typeof(SkillUpgrades)).GetTypes().Where(x => x.IsSubclassOf(typeof(AbstractSkillUpgrade))))
            {
                AbstractSkillUpgrade skill = (AbstractSkillUpgrade)Activator.CreateInstance(t);

                if (!globalSettings.EnabledSkills.TryGetValue(skill.Name, out bool? enabled))
                {
                    enabled = true;
                    globalSettings.EnabledSkills[skill.Name] = enabled;
                }
                

                if (enabled != null)
                {
                    _skills[skill.Name] = skill;

                    skill.Initialize();
                    skill.skillUpgradeActive = true;

                    if (enabled == false)
                    {
                        skill.skillUpgradeActive = false;
                        skill.Unload();
                    }
                }    

            }

        }

        #region Menu
        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            List<IMenuMod.MenuEntry> menuEntries = new List<IMenuMod.MenuEntry>();

            // TODO: Global toggle

            foreach (var kvp in _skills)
            {
                string name = kvp.Key;
                AbstractSkillUpgrade skill = kvp.Value;
                IMenuMod.MenuEntry entry = new IMenuMod.MenuEntry()
                {
                    Name = name,
                    Description = skill.Description,
                    Values = new string[] { "False", "True" },
                    Saver = opt => Toggle(name, opt == 1),
                    Loader = () => globalSettings.EnabledSkills[name] == true ? 1 : 0,
                };

                menuEntries.Add(entry);

            }

            return menuEntries;
        }

        public bool ToggleButtonInsideMenu => false;
        #endregion

        // TODO: Global Toggle

        internal static void Toggle(string name, bool enable)
        {
            if (globalSettings.EnabledSkills[name] == null) return;

            if (globalSettings.EnabledSkills[name] == enable) return;

            AbstractSkillUpgrade skill = _skills[name];
            if (enable)
            {
                if (skill.IsUnloadable) skill.Initialize();
                skill.skillUpgradeActive = true;
            }
            else
            {
                skill.skillUpgradeActive = false;
                skill.Unload();
            }

            globalSettings.EnabledSkills[name] = enable;
        }

        public override string GetVersion()
        {
            return "0.2";
        }

    }
}
