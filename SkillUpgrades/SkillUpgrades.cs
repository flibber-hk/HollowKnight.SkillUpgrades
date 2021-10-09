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
        public static SkillUpgradeSettings globalSettings { get; set; } = new SkillUpgradeSettings();
        public void OnLoadGlobal(SkillUpgradeSettings s) => globalSettings = s;
        public SkillUpgradeSettings OnSaveGlobal() => globalSettings;
        #endregion

        #region Local Settings
        public static SkillUpgradeSaveData localOverrides { get; set; } = new SkillUpgradeSaveData();
        public void OnLoadLocal(SkillUpgradeSaveData s) => localOverrides = s;
        public SkillUpgradeSaveData OnSaveLocal() => localOverrides;
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

                if (enabled == null && skill.IsUnloadable)
                {
                    enabled = false;
                    globalSettings.EnabledSkills[skill.Name] = enabled;
                }

                if (!SettingsOverrides.SkillLoadOverrides.TryGetValue(skill.Name, out bool shouldInitializeSkill))
                {
                    shouldInitializeSkill = enabled != null;
                }

                if (shouldInitializeSkill)
                {
                    _skills[skill.Name] = skill;

                    skill.Log("Loading skill upgrade");

                    skill.Initialize();
                    skill.SkillUpgradeActive = true;

                    UpdateSkillState(skill.Name);
                }

            }

            if (_skills.Values.Any(skill => skill.InvolvesHeroRotation)) HeroRotation.Hook();

            SettingsOverrides.AlreadyLoadedSkills = true;
            Log("Initialization done!");
        }

        #region Menu
        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            List<IMenuMod.MenuEntry> menuEntries = new List<IMenuMod.MenuEntry>();

            menuEntries.Add(new IMenuMod.MenuEntry()
            {
                Name = "Global Toggle",
                Description = "Turn this setting off to deactivate all skill upgrades.",
                Values = new string[] { "False", "True" },
                Saver = opt => ApplyGlobalToggle(opt == 1),
                Loader = () => globalSettings.GlobalToggle ? 1 : 0
            });

            foreach (var kvp in _skills)
            {
                string name = kvp.Key;

                if (globalSettings.EnabledSkills[name] == null) continue;

                AbstractSkillUpgrade skill = kvp.Value;
                IMenuMod.MenuEntry entry = new IMenuMod.MenuEntry()
                {
                    Name = skill.UIName,
                    Description = skill.Description,
                    Values = new string[] { "False", "True" },
                    Saver = opt => Toggle(name, opt == 1),
                    Loader = () => globalSettings.EnabledSkills[name] == true ? 1 : 0
                };

                menuEntries.Add(entry);

            }

            return menuEntries;
        }

        public bool ToggleButtonInsideMenu => false;
        #endregion

        internal static void UpdateSkillState(string name)
        {
            if (!_skills.TryGetValue(name, out AbstractSkillUpgrade skill))
            {
                instance.LogError($"UpdateSkillState: skill not found: {name}");
                return;
            }

            bool shouldEnable;
            if (localOverrides.EnabledSkills.TryGetValue(name, out bool overrideValue))
            {
                shouldEnable = overrideValue;
            }
            else
            {
                shouldEnable = globalSettings.EnabledSkills[name] ?? false;
                shouldEnable &= globalSettings.GlobalToggle;
            }

            if (shouldEnable && !skill.SkillUpgradeActive)
            {
                skill.ReInitialize();
                skill.SkillUpgradeActive = true;
            }
            else if (!shouldEnable && skill.SkillUpgradeActive)
            {
                skill.SkillUpgradeActive = false;
                skill.Unload();
            }
        }
        internal static void ApplyGlobalToggle(bool enable)
        {
            globalSettings.GlobalToggle = enable;

            foreach (string name in _skills.Keys)
            {
                UpdateSkillState(name);
            }
        }
        internal static void Toggle(string name, bool enable)
        {
            globalSettings.EnabledSkills[name] = enable;

            UpdateSkillState(name);
        }

        public override string GetVersion()
        {
            return "0.4";
        }

        public override int LoadPriority()
        {
            return 25;
        }

    }
}
