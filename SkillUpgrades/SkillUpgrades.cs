using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modding;
using SkillUpgrades.Skills;
using SkillUpgrades.Util;

namespace SkillUpgrades
{
    public class SkillUpgrades : Mod, IGlobalSettings<SkillUpgradeSettings>, ILocalSettings<SkillUpgradeSaveData>, IMenuMod
    {
        internal static SkillUpgrades instance;
        internal static readonly Dictionary<string, AbstractSkillUpgrade> _skills = new Dictionary<string, AbstractSkillUpgrade>();

        #region Global Settings
        public static SkillUpgradeSettings GlobalSettings { get; set; } = new SkillUpgradeSettings();
        public void OnLoadGlobal(SkillUpgradeSettings s) => GlobalSettings = s;
        public SkillUpgradeSettings OnSaveGlobal() => GlobalSettings;
        #endregion

        #region Local Settings
        public static SkillUpgradeSaveData LocalSaveData { get; set; } = new SkillUpgradeSaveData();
        public void OnLoadLocal(SkillUpgradeSaveData s) => LocalSaveData = s;
        public SkillUpgradeSaveData OnSaveLocal() => LocalSaveData;
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

            if (_skills.Values.Any(skill => skill.InvolvesHeroRotation && skill.SkillUpgradeInitialized)) HeroRotation.Hook();

            SkillSettingOverrides.AlreadyLoadedSkills = true;
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
                Loader = () => GlobalSettings.GlobalToggle ? 1 : 0
            };
            if (_skills.Values.Where(x => x.SkillUpgradeInitialized).Where(x => LocalSaveData.EnabledSkills.TryGetValue(x.Name, out bool state) && !state).Any())
            {
                globalToggleEntry.Description = "Some skills might not be affected by the global toggle for this save file";
            }
            menuEntries.Add(globalToggleEntry);

            foreach (AbstractSkillUpgrade skill in _skills.Values)
            {
                if (!skill.SkillUpgradeInitialized) continue;

                IMenuMod.MenuEntry entry = new IMenuMod.MenuEntry()
                {
                    Name = skill.UIName,
                    Description = skill.Description,
                    Values = new string[] { "False", "True" },
                    Saver = opt => Toggle(skill, opt == 1),
                    Loader = () => GlobalSettings.EnabledSkills[skill.Name] == true ? 1 : 0
                };
                if (LocalSaveData.EnabledSkills.ContainsKey(skill.Name))
                {
                    entry.Description = "Changes to this setting won't affect this save file";
                }

                menuEntries.Add(entry);
            }

            foreach (AbstractSkillUpgrade skill in _skills.Values)
            {
                if (!skill.SkillUpgradeInitialized) continue;
                skill.AddTogglesToMenu(menuEntries);
            }

            return menuEntries;
        }

        public bool ToggleButtonInsideMenu => false;
        #endregion

        internal static void ApplyGlobalToggle(bool enable)
        {
            GlobalSettings.GlobalToggle = enable;

            foreach (AbstractSkillUpgrade skill in _skills.Values)
            {
                skill.UpdateSkillState();
            }
        }
        internal static void Toggle(AbstractSkillUpgrade skill, bool enable)
        {
            GlobalSettings.EnabledSkills[skill.Name] = enable;
            skill.UpdateSkillState();
        }

        public override string GetVersion()
        {
            return "0.5";
        }

        public override int LoadPriority()
        {
            return 1000;
        }

    }
}
