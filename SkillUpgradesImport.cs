using System;
using MonoMod.ModInterop;

// Replace the namespace with your project's root namespace
namespace ...
{
    internal static class SkillUpgrades
    {
        [ModImportName("SkillUpgrades")]
        private static class SkillUpgradesImport
        {
            public static Action<string, bool?> OverrideSkillState = null;
            public static Action<string, string, object> OverrideFieldValue = null;
			public static Action<string, string> ClearFieldOverride = null;
        }
        static SkillUpgrades()
        {
            // MonoMod will automatically fill in the actions in DebugImport the first time they're used
            typeof(SkillUpgradesImport).ModInterop();
        }

        public static void OverrideSkillState(string Name, bool? state)
            => SkillUpgradesImport.OverrideSkillState?.Invoke(Name, state);

        public static void OverrideFieldValue(string SkillName, string FieldName, object value)
            => SkillUpgradesImport.OverrideFieldValue?.Invoke(SkillName, FieldName, value);

        public static void ClearFieldOverride(string SkillName, string FieldName)
            => SkillUpgradesImport.ClearFieldOverride?.Invoke(SkillName, FieldName);
    }
}