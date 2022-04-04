using System;
using MonoMod.ModInterop;

namespace SkillUpgrades.Imports
{
    internal static class QoL
    {
        [ModImportName("QoL")]
        private static class QolImport
        {
            public static Action<string, bool> OverrideModuleToggle = null;
            public static Action<string, string, bool> OverrideSettingToggle = null;
            public static Action<string> RemoveModuleOverride = null;
            public static Action<string, string> RemoveSettingOverride = null;

        }
        static QoL()
        {
            // MonoMod will automatically fill in the actions in QolImport the first time they're used
            typeof(QolImport).ModInterop();
        }

        public static void OverrideModuleToggle(string type, bool enable)
            => QolImport.OverrideModuleToggle?.Invoke(type, enable);

        public static void OverrideSettingToggle(string type, string field, bool enable)
            => QolImport.OverrideSettingToggle?.Invoke(type, field, enable);

        public static void RemoveModuleOverride(string type)
            => QolImport.RemoveModuleOverride?.Invoke(type);

        public static void RemoveSettingOverride(string type, string field)
            => QolImport.RemoveSettingOverride?.Invoke(type, field);

        // TryGet... not defined because they need a custom delegate type
    }
}