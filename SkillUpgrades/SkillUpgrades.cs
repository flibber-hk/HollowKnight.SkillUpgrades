using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modding;
using MonoMod.ModInterop;
using UnityEngine.UI;
using SkillUpgrades.Menu;
using SkillUpgrades.Skills;
using SkillUpgrades.Util;

namespace SkillUpgrades
{
    public class SkillUpgrades : Mod, IGlobalSettings<SkillUpgradeSettings>, ICustomMenuMod
    {
        internal static SkillUpgrades instance;
        internal static readonly Dictionary<string, AbstractSkillUpgrade> _skills = new Dictionary<string, AbstractSkillUpgrade>();

        #region Global Settings
        public static SkillUpgradeSettings GS { get; set; } = new SkillUpgradeSettings();
        public void OnLoadGlobal(SkillUpgradeSettings s) => GS = s;
        public SkillUpgradeSettings OnSaveGlobal() => GS;
        #endregion

        public SkillUpgrades() : base(null)
        {
            instance = this;
            typeof(Export).ModInterop();
        }


        public override void Initialize()
        {
            instance.Log("Initializing");
            DebugMod.AddActionToKeyBindList(() => { ApplyGlobalToggle(!GS.GlobalToggle); RefreshMainMenu(); }, "Global Toggle", "SkillUpgrades");

            foreach (Type t in Assembly.GetAssembly(typeof(SkillUpgrades)).GetTypes().Where(x => x.IsSubclassOf(typeof(AbstractSkillUpgrade))))
            {
                AbstractSkillUpgrade skill = (AbstractSkillUpgrade)Activator.CreateInstance(t);
                _skills.Add(skill.Name, skill);
                skill.InitializeSkillUpgrade();
                skill.UpdateSkillState();
                DebugMod.AddActionToKeyBindList(() => Toggle(skill), skill.Name, "SkillUpgrades");
            }

            HeroRotation.Hook();

            // All ItemChanger/Randomizer dependence should be kept to the IC and RM namespaces
            // ItemChanger Interop
            if (ModHooks.GetMod("ItemChangerMod") is Mod)
            {
                Log("Hooking ItemChanger");
                IC.ItemChangerInterop.HookItemChanger();
            }
            // Rando Interop
            if (ModHooks.GetMod("Randomizer 4") is Mod
                && ModHooks.GetMod("MenuChanger") is Mod)
            {
                RM.RandomizerInterop.HookRandomizer();
            }

            Log("Initialization done!");
        }

        #region Menu
        internal static Action RefreshSkillMenu = null;
        internal static Action RefreshSkillSettingMenu = null;
        /// <summary>
        /// Force each menu to refresh
        /// </summary>
        internal static void RefreshAllMenus()
        {
            instance.RefreshMainMenu();
            RefreshSkillMenu?.Invoke();
            RefreshSkillSettingMenu?.Invoke();
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            ModMenuScreenBuilder builder = new(Name, modListMenu);
            builder.AddHorizontalOption(new IMenuMod.MenuEntry()
            {
                Name = "Global Toggle",
                Description = "Turn this setting off to deactivate all skill upgrades.",
                Values = new string[] { "False", "True" },
                Saver = opt => ApplyGlobalToggle(opt == 1),
                Loader = () => GS.GlobalToggle ? 1 : 0
            });

            List<IMenuMod.MenuEntry> skillMenuEntries = new();
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

                skillMenuEntries.Add(entry);
            }
            builder.AddSubpage("Enabled Skills", "Enable and disable skill upgrades", skillMenuEntries, out RefreshSkillMenu);

            List<IMenuMod.MenuEntry> skillSettingEntries = new();
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

                    skillSettingEntries.Add(entry);
                }
            }

            foreach (AbstractSkillUpgrade skill in _skills.Values)
            {
                skill.AddToMenuList(skillSettingEntries);
            }
            builder.AddSubpage("Skill Preferences", "Toggle settings specific to each skill", skillSettingEntries, out RefreshSkillSettingMenu);

            return builder.CreateMenuScreen();
        }

        public bool ToggleButtonInsideMenu => false;

        public void RefreshMainMenu()
        {
            MenuScreen screen = ModHooks.BuiltModMenuScreens[this];
            if (screen != null)
            {
                foreach (MenuOptionHorizontal option in screen.GetComponentsInChildren<MenuOptionHorizontal>())
                {
                    option.menuSetting.RefreshValueFromGameSettings();
                }
            }
        }
        #endregion

        internal static void ApplyGlobalToggle(bool enable)
        {
            GS.GlobalToggle = enable;

            foreach (AbstractSkillUpgrade skill in _skills.Values)
            {
                skill.UpdateSkillState();
            }
        }
        // Used when toggling the skill upgrade outside the menu
        internal static void Toggle(AbstractSkillUpgrade skill)
        {
            Toggle(skill, !GS.EnabledSkills[skill.Name]);
            RefreshSkillMenu?.Invoke();
            DebugMod.LogToConsole((GS.EnabledSkills[skill.Name] ? "Enabled " : "Disabled ") + skill.Name);
        }
        internal static void Toggle(AbstractSkillUpgrade skill, bool enable)
        {
            GS.EnabledSkills[skill.Name] = enable;
            skill.UpdateSkillState();
        }

        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }

        public override int LoadPriority()
        {
            return 0;
        }

    }
}
