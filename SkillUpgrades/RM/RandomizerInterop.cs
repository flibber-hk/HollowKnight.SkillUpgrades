using ItemChanger;
using Modding;
using Newtonsoft.Json;
using RandomizerCore;
using RandomizerMod;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using SkillUpgrades.IC.Items;
using System.IO;

namespace SkillUpgrades.RM
{
    public static class RandomizerInterop
    {
        public static RandoSettings RandoSettings
        {
            get => SkillUpgrades.GS.RandoSettings;
            set => SkillUpgrades.GS.RandoSettings = value;
        }

        public static void HookRandomizer()
        {
            MenuHolder.Hook();
            LogicPatcher.Hook();
            RequestMaker.HookRequestBuilder();

            SettingsLog.AfterLogSettings += AddSettingsToLog;
            RandoController.OnExportCompleted += RemoveUnselectedSkillUpgrades;

            if (ModHooks.GetMod("RandoSettingsManager") is not null) RandoSettingsManagerInterop.Hook();
        }

        private static void RemoveUnselectedSkillUpgrades(RandoController rc)
        {
            if (RandoSettings.SkillUpgradeRandomization == MainSkillUpgradeRandoType.RandomSelection)
            {
                foreach (string skillName in SkillUpgrades.SkillNames)
                {
                    ItemChangerMod.Modules.GetOrAdd<SkillUpgradeUnlockModule>().RegisterSkill(skillName);
                }
            }
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
