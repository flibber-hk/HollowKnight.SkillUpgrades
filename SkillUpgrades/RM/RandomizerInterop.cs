using Newtonsoft.Json;
using RandomizerCore;
using RandomizerMod;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using System.IO;

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
            RequestMaker.HookRequestBuilder();

            SettingsLog.AfterLogSettings += AddSettingsToLog;
        }

        private static void AddSettingsToLog(LogArguments args, TextWriter tw)
        {
            tw.WriteLine("Logging SkillUpgrades settings:");
            using JsonTextWriter jtw = new(tw) { CloseOutput = false, };
            RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jtw, RandoSettings);
            tw.WriteLine();
        }
    }
}
