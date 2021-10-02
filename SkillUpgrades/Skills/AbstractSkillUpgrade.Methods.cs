using System.Runtime.CompilerServices;
using Modding;

namespace SkillUpgrades.Skills
{
    public abstract partial class AbstractSkillUpgrade : ILogger
    {
        #region Adjustable Fields
        protected bool GetBool(bool @default, [CallerMemberName] string name = null)
        {
            if (name == null) return default;

            string key = SkillUpgradeSettings.GetKey(Name, name);

            if (SettingsOverrides.Booleans.TryGetValue(key, out bool ret)) return ret;
            else if (SkillUpgrades.globalSettings.Booleans.TryGetValue(key, out ret)) return ret;

            SkillUpgrades.globalSettings.Booleans[key] = @default; return @default;
        }
        protected int GetInt(int @default, [CallerMemberName] string name = null)
        {
            if (name == null) return default;

            string key = SkillUpgradeSettings.GetKey(Name, name);

            if (SettingsOverrides.Integers.TryGetValue(key, out int ret)) return ret;
            else if (SkillUpgrades.globalSettings.Integers.TryGetValue(key, out ret)) return ret;

            SkillUpgrades.globalSettings.Integers[key] = @default; return @default;
        }
        protected float GetFloat(float @default, [CallerMemberName] string name = null)
        {
            if (name == null) return default;

            string key = SkillUpgradeSettings.GetKey(Name, name);

            if (SettingsOverrides.Floats.TryGetValue(key, out float ret)) return ret;
            else if (SkillUpgrades.globalSettings.Floats.TryGetValue(key, out ret)) return ret;

            SkillUpgrades.globalSettings.Floats[key] = @default; return @default;
        }
        #endregion

        protected AbstractSkillUpgrade()
        {
            Name = GetType().Name;
        }

        // If they improve the access levels of the loggable class in the mapi then I don't need to do this garbage
        #region Logging
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
