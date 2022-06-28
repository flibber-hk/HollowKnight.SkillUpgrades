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
        public static RandoSettings RandoSettings => SkillUpgrades.GS.RandoSettings;

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
