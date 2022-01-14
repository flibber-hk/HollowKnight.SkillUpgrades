using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Modding;
using SkillUpgrades.Util;

namespace SkillUpgrades.Skills
{
    public abstract class AbstractSkillUpgrade : ILogger
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
        /// The name of the skill (the type name)
        /// </summary>
        public string Name => GetType().Name;
        /// <summary>
        /// The name to show in the menu
        /// </summary>
        public virtual string UIName => Name.FromCamelCase();

        /// <summary>
        /// The description to show in the menu
        /// </summary>
        public virtual string Description => string.Empty;

        /// <summary>
        /// Any buttons the skill wants to add to the menu can be done here
        /// </summary>
        public virtual void AddToMenuList(List<IMenuMod.MenuEntry> entries) { }

        public bool SkillUpgradeActive { get; private set; } = true;
        public bool MatchesGlobalSetting { get; private set; } = true;

        #region Skill States
        // Initialize skill if it should be initialized
        internal void InitializeSkillUpgrade()
        {
            // Make sure skill is in dictionary
            SkillUpgrades.GS.EnabledSkills.EnsureInDict(Name, false);

            Log("Initializing skill");
            StartUpInitialize();
            RepeatableInitialize();
        }
        public void UpdateSkillState()
        {
            if (!MatchesGlobalSetting) return;
            // Skill must be in dictionary at this point because it gets initialized first
            if (!SkillUpgrades.GS.GlobalToggle)
            {
                SetState(false);
            }
            else
            {
                SetState(SkillUpgrades.GS.EnabledSkills[Name]);
            }
        }
        /// <summary>
        /// Set the skill state in such a way that the global setting is ignored
        /// </summary>
        /// <param name="Name">The name of the skill</param>
        /// <param name="state">True or false to set the state, or null to clear the override</param>
        [PublicAPI]
        public static void OverrideSkillState(string Name, bool? state)
        {
            if (!SkillUpgrades._skills.TryGetValue(Name, out AbstractSkillUpgrade skill))
            {
                SkillUpgrades.instance.LogWarn($"Could not find skill {Name}");
                return;
            }

            skill.OverrideSkillState(state);
        }
        /// <summary>
        /// Set the skill state in such a way that the global setting is ignored
        /// </summary>
        /// <param name="state">True or false to set the state, or null to clear the override</param>
        internal void OverrideSkillState(bool? state)
        {
            if (state == null)
            {
                MatchesGlobalSetting = true;
                UpdateSkillState();
            }
            else
            {
                MatchesGlobalSetting = false;
                SetState((bool)state);
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

        /// <summary>
        /// Returns an enumerable over the Skill Upgrades in this assembly.
        /// </summary>
        public static IEnumerable<Type> GetAvailableSkillUpgradeTypes()
        {
            Assembly asm = typeof(AbstractSkillUpgrade).Assembly;
            // TODO - take GetTypesSafely from MAPI when I can
            try
            {
                return asm.GetTypes().Where(type => type.IsSubclassOf(typeof(AbstractSkillUpgrade)) && !type.IsAbstract);
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type is not null && type.IsSubclassOf(typeof(AbstractSkillUpgrade)) && !type.IsAbstract);
            }
        }
    }
}
