using Hkmp.Api.Client;
using Hkmp.Api.Server;

namespace SkillUpgrades.HKMP
{
    internal static class HkmpInterop
    {
        public static void HookHkmp()
        {
            SkillUpgradesClientAddon.Instance = new();
            ClientAddon.RegisterAddon(SkillUpgradesClientAddon.Instance);

            SkillUpgradesServerAddon.Instance = new();
            ServerAddon.RegisterAddon(SkillUpgradesServerAddon.Instance);
        }
    }
}
