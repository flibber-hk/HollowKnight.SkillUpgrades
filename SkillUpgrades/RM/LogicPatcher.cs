using RandomizerMod.RC;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkillUpgrades.RM
{
    public static class LogicPatcher
    {
        public static void Hook()
        {
            RCData.RuntimeLogicOverride.Subscribe(13.72f, AddTermsAndItems);
            RCData.RuntimeLogicOverride.Subscribe(13.72f, EnableLocalLogicEdits);
        }

        private static void EnableLocalLogicEdits(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!RandomizerInterop.RandoSettings.Any) return;

            string directory = Path.Combine(Path.GetDirectoryName(typeof(LogicPatcher).Assembly.Location), "Logic");
            try
            {
                DirectoryInfo di = new(directory);
                if (di.Exists)
                {
                    List<FileInfo> macros = new();
                    List<FileInfo> logic = new();

                    foreach (FileInfo fi in di.EnumerateFiles())
                    {
                        if (!fi.Extension.ToLower().EndsWith("json")) continue;
                        else if (fi.Name.ToLower().StartsWith("macro")) macros.Add(fi);
                        else logic.Add(fi);
                    }
                    foreach (FileInfo fi in macros)
                    {
                        using FileStream fs = fi.OpenRead();
                        lmb.DeserializeJson(LogicManagerBuilder.JsonType.MacroEdit, fs);
                    }
                    foreach (FileInfo fi in logic)
                    {
                        using FileStream fs = fi.OpenRead();
                        lmb.DeserializeJson(LogicManagerBuilder.JsonType.LogicEdit, fs);
                    }
                }
            }
            catch (Exception e)
            {
                SkillUpgrades.instance.LogError("Error fetching local logic changes:\n" + e);
            }
        }

        private static void AddTermsAndItems(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!RandomizerInterop.RandoSettings.Any)
            {
                return;
            }

            foreach (string skillName in SkillUpgrades.SkillNames)
            {
                Term t = lmb.GetOrAddTerm("SKILLUPGRADE_" + skillName);
                lmb.AddItem(new SingleItem(skillName, new(t, 1)));
            }
        }
    }
}
