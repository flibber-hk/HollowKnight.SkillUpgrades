using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modding;
using SkillUpgrades.Skills;
using SkillUpgrades.Util;

namespace SkillUpgrades
{
    public class SkillUpgrades : Mod, IGlobalSettings<SkillUpgradeSettings>, IMenuMod
    {
        internal static SkillUpgrades instance;
        internal static readonly Dictionary<string, AbstractSkillUpgrade> _skills = new Dictionary<string, AbstractSkillUpgrade>();

        #region Global Settings
        public static SkillUpgradeSettings GS { get; set; } = new SkillUpgradeSettings();
        public void OnLoadGlobal(SkillUpgradeSettings s) => GS = s;
        public SkillUpgradeSettings OnSaveGlobal() => GS;
        #endregion

        public override void Initialize()
        {
            instance = this;
            instance.Log("Initializing");

            foreach (Type t in Assembly.GetAssembly(typeof(SkillUpgrades)).GetTypes().Where(x => x.IsSubclassOf(typeof(AbstractSkillUpgrade))))
            {
                AbstractSkillUpgrade skill = (AbstractSkillUpgrade)Activator.CreateInstance(t);
                _skills.Add(skill.Name, skill);
                skill.InitializeSkillUpgrade();
                skill.UpdateSkillState();
            }

            HeroRotation.Hook();
            Log("Initialization done!");
        }

        #region Menu
        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            List<IMenuMod.MenuEntry> menuEntries = new List<IMenuMod.MenuEntry>();

            IMenuMod.MenuEntry globalToggleEntry = new IMenuMod.MenuEntry()
            {
                Name = "Global Toggle",
                Description = "Turn this setting off to deactivate all skill upgrades.",
                Values = new string[] { "False", "True" },
                Saver = opt => ApplyGlobalToggle(opt == 1),
                Loader = () => GS.GlobalToggle ? 1 : 0
            };
            menuEntries.Add(globalToggleEntry);

            foreach (AbstractSkillUpgrade skill in _skills.Values)
            {
                IMenuMod.MenuEntry entry = new IMenuMod.MenuEntry()
                {
                    Name = skill.UIName,
                    Description = skill.Description,
                    Values = new string[] { "False", "True" },
                    Saver = opt => Toggle(skill, opt == 1),
                    Loader = () => GS.EnabledSkills[skill.Name] == true ? 1 : 0
                };

                menuEntries.Add(entry);
            }

            foreach (var kvp in SkillUpgradeSettings.Fields)
            {
                if (kvp.Value.GetCustomAttribute<MenuTogglableAttribute>() is MenuTogglableAttribute mt && kvp.Value.FieldType == typeof(bool))
                {
                    IMenuMod.MenuEntry entry = new IMenuMod.MenuEntry()
                    {
                        Name = mt.name ?? kvp.Value.Name.FromCamelCase(),
                        Description = mt.desc,
                        Values = new string[] { "False", "True" },
                        Saver = opt => GS.SetValue(kvp.Key, opt == 1, SkillFieldSetOptions.ApplyToGlobalSetting),
                        Loader = () => (bool)GS.GetDefaultValue(kvp.Key) ? 1 : 0,
                    };

                    menuEntries.Add(entry);
                }
            }

            foreach (AbstractSkillUpgrade skill in _skills.Values)
            {
                skill.AddToMenuList(menuEntries);
            }

            return menuEntries;
        }

        public bool ToggleButtonInsideMenu => false;
        #endregion

        internal static void ApplyGlobalToggle(bool enable)
        {
            GS.GlobalToggle = enable;

            foreach (AbstractSkillUpgrade skill in _skills.Values)
            {
                skill.UpdateSkillState();
            }
        }
        internal static void Toggle(AbstractSkillUpgrade skill, bool enable)
        {
            GS.EnabledSkills[skill.Name] = enable;
            skill.UpdateSkillState();
        }

        public override string GetVersion()
        {
            return "0.6";
        }

        public override int LoadPriority()
        {
            return 1000;
        }

    }
}
