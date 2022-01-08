using RandomizerCore;
using RandomizerMod;
using RandomizerMod.RC;

namespace SkillUpgrades.RM
{
    public static class RandomizerInterop
    {
        private static RandoSettings _randoSettings;
        public static RandoSettings RandoSettings
        {
            get
            {
                if (_randoSettings == null)
                {
                    _randoSettings = new();
                }
                return _randoSettings;
            }
        }

        public static void HookRandomizer()
        {
            MenuHolder.Hook();
            LogicPatcher.Hook();
            RequestMaker.Hook();
        }
    }
}
