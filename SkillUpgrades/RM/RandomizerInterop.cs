using RandomizerCore;
using RandomizerMod;
using RandomizerMod.RC;

namespace SkillUpgrades.RM
{
    public static class RandomizerInterop
    {
        public static RandoSettings RandoSettings => SkillUpgrades.GS.RandoSettings;

        public static void HookRandomizer()
        {
            MenuHolder.Hook();
            LogicPatcher.Hook();
            RequestMaker.Hook();
        }
    }
}
