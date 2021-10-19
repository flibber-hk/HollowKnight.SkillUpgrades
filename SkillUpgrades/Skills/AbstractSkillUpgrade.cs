using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Modding;

namespace SkillUpgrades.Skills
{
    public abstract partial class AbstractSkillUpgrade : ILogger
    {
        /// <summary>
        /// Initialization that is done once
        /// </summary>
        protected virtual void StartUpInitialize() { }
        /// <summary>
        /// Initialization that can be undone in Unload()
        /// </summary>
        protected virtual void RepeatableInitialize() { }
        /// <summary>
        /// Unload the skill upgrade
        /// </summary>
        protected virtual void Unload() { }

        /// <summary>
        /// The name of the skill (by default, the type name)
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// The name to show in the menu
        /// </summary>
        public virtual string UIName => Regex.Replace(Name, "([A-Z])", " $1").TrimEnd();

        /// <summary>
        /// The description to show in the menu
        /// </summary>
        public virtual string Description => string.Empty;

        /// <summary>
        /// If this is true, ensures that the HeroRotation module is loaded, allowing the knight to rotate without moving its hitbox
        /// </summary>
        public virtual bool InvolvesHeroRotation => false;

        public bool SkillUpgradeActive { get; private set; } = true;
        public bool SkillUpgradeInitialized { get; private set; } = false;


        #region Skill States
        // Initialize skill if it should be initialized
        public void InitializeSkillUpgrade()
        {
            // Make sure skill is in dictionary
            if (!SkillUpgrades.GlobalSettings.EnabledSkills.ContainsKey(Name))
            {
                SkillUpgrades.GlobalSettings.EnabledSkills[Name] = true;
            }

            if (SkillUpgradeInitialized)
            {
                LogError("Skill Upgrade already initialized!");
                return;
            }

            if (!SkillSettingOverrides.SkillLoadOverrides.TryGetValue(Name, out bool shouldInitialize))
            {
                shouldInitialize = SkillUpgrades.GlobalSettings.EnabledSkills[Name] != null;
            }
            if (shouldInitialize)
            {
                Log("Initializing skill");
                StartUpInitialize();
                RepeatableInitialize();
                SkillUpgradeInitialized = true;
            }
            else
            {
                LogDebug("Not initializing skill");
            }
        }
        public void UpdateSkillState()
        {
            // Skills get one chance to initialize
            if (!SkillUpgradeInitialized) return;

            // Make sure skill is in dictionary
            if (!SkillUpgrades.GlobalSettings.EnabledSkills.ContainsKey(Name))
            {
                SkillUpgrades.GlobalSettings.EnabledSkills[Name] = true;
            }

            if (SkillUpgrades.LocalSaveData.EnabledSkills.TryGetValue(Name, out bool shouldBeActive))
            {
                SetState(shouldBeActive);
                return;
            }
            else if (!SkillUpgrades.GlobalSettings.GlobalToggle)
            {
                SetState(false);
                return;
            }
            else
            {
                SetState(SkillUpgrades.GlobalSettings.EnabledSkills[Name] ?? false);
                return;
            }
        }
        private void SetState(bool set)
        {
            if (set && !SkillUpgradeActive)
            {
                RepeatableInitialize();
                SkillUpgradeActive = true;
            }
            else if (!set && SkillUpgradeActive)
            {
                SkillUpgradeActive = false;
                Unload();
            }
        }
        #endregion


        #region Adjustable Fields
        // Global settings with potential local overrides
        protected bool GetBool(bool @default, [CallerMemberName] string name = null)
        {
            if (name == null) return default;

            string key = SkillUpgradeSettings.GetKey(Name, name);
            if (!SkillUpgrades.GlobalSettings.Booleans.ContainsKey(key)) SkillUpgrades.GlobalSettings.Booleans[key] = @default;

            if (SkillUpgrades.LocalSaveData.Booleans.TryGetValue(key, out bool ret)) return ret;
            else return SkillUpgrades.GlobalSettings.Booleans[key];
        }
        protected int GetInt(int @default, [CallerMemberName] string name = null)
        {
            if (name == null) return default;

            string key = SkillUpgradeSettings.GetKey(Name, name);
            if (!SkillUpgrades.GlobalSettings.Integers.ContainsKey(key)) SkillUpgrades.GlobalSettings.Integers[key] = @default;

            if (SkillUpgrades.LocalSaveData.Integers.TryGetValue(key, out int ret)) return ret;
            else return SkillUpgrades.GlobalSettings.Integers[key];
        }
        protected float GetFloat(float @default, [CallerMemberName] string name = null)
        {
            if (name == null) return default;

            string key = SkillUpgradeSettings.GetKey(Name, name);
            if (!SkillUpgrades.GlobalSettings.Floats.ContainsKey(key)) SkillUpgrades.GlobalSettings.Floats[key] = @default;

            if (SkillUpgrades.LocalSaveData.Floats.TryGetValue(key, out float ret)) return ret;
            else return SkillUpgrades.GlobalSettings.Floats[key];
        }

        // Local-only settings
        protected bool GetBoolLocal(bool @default, [CallerMemberName] string name = null)
        {
            if (name == null) return default;

            string key = SkillUpgradeSettings.GetKey(Name, name);
            if (SkillUpgrades.LocalSaveData.Booleans.TryGetValue(key, out bool ret)) return ret;
            else return @default;
        }
        protected int GetIntLocal(int @default, [CallerMemberName] string name = null)
        {
            if (name == null) return default;

            string key = SkillUpgradeSettings.GetKey(Name, name);
            if (SkillUpgrades.LocalSaveData.Integers.TryGetValue(key, out int ret)) return ret;
            else return @default;
        }
        protected float GetFloatLocal(float @default, [CallerMemberName] string name = null)
        {
            if (name == null) return default;

            string key = SkillUpgradeSettings.GetKey(Name, name);
            if (SkillUpgrades.LocalSaveData.Floats.TryGetValue(key, out float ret)) return ret;
            else return @default;
        }

        // Setters accessible to the skill upgrades
        protected void SetBoolLocal(bool value, [CallerMemberName] string name = null)
        {
            if (name == null) return;

            string key = SkillUpgradeSettings.GetKey(Name, name);
            SkillUpgrades.LocalSaveData.Booleans[key] = value;
        }
        protected void SetIntLocal(int value, [CallerMemberName] string name = null)
        {
            if (name == null) return;

            string key = SkillUpgradeSettings.GetKey(Name, name);
            SkillUpgrades.LocalSaveData.Integers[key] = value;
        }
        protected void SetFloatLocal(float value, [CallerMemberName] string name = null)
        {
            if (name == null) return;

            string key = SkillUpgradeSettings.GetKey(Name, name);
            SkillUpgrades.LocalSaveData.Floats[key] = value;
        }
        #endregion

        protected AbstractSkillUpgrade()
        {
            Name = GetType().Name;
        }

        #region Logging
        // If they improve the access levels of the loggable class in the mapi then I don't need to do this garbage
        protected virtual string LogPrefix => $"[SkillUpgrades]:[{Name}]";

        public void LogFine(string message) => Logger.LogFine(FormatLogMessage(message));
        public void LogFine(object message) => Logger.LogFine(FormatLogMessage(message));
        public void LogDebug(string message) => Logger.LogDebug(FormatLogMessage(message));
        public void LogDebug(object message) => Logger.LogDebug(FormatLogMessage(message));
        public void Log(string message) => Logger.Log(FormatLogMessage(message));
        public void Log(object message) => Logger.Log(FormatLogMessage(message));
        public void LogWarn(string message) => Logger.LogWarn(FormatLogMessage(message));
        public void LogWarn(object message) => Logger.LogWarn(FormatLogMessage(message));
        public void LogError(string message) => Logger.LogError(FormatLogMessage(message));
        public void LogError(object message) => Logger.LogError(FormatLogMessage(message));

        protected virtual string FormatLogMessage(string message)
        {
            return $"{LogPrefix} - {message}".Replace("\n", $"\n{LogPrefix} - ");
        }
        private string FormatLogMessage(object message) => FormatLogMessage(message?.ToString() ?? "null");
        #endregion
    }
}
