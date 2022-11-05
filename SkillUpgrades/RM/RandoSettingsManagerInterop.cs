using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace SkillUpgrades.RM
{
    internal static class RandoSettingsManagerInterop
    {
        public static void Hook()
        {
            RandoSettingsManagerMod.Instance.RegisterConnection(new RandoPlusSettingsProxy());
        }
    }

    internal class RandoPlusSettingsProxy : RandoSettingsProxy<RandoSettings, string>
    {
        public override string ModKey => SkillUpgrades.instance.GetName();

        public override VersioningPolicy<string> VersioningPolicy { get; }
            = new EqualityVersioningPolicy<string>(SkillUpgrades.instance.GetVersion());

        public override void ReceiveSettings(RandoSettings settings)
        {
            settings ??= new();
            MenuHolder.Instance.suMEF.SetMenuValues(settings);
        }

        public override bool TryProvideSettings(out RandoSettings settings)
        {
            settings = RandomizerInterop.RandoSettings;
            return settings.Any;
        }
    }
}
