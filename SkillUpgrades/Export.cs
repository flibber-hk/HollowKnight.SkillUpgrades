using System;
using MonoMod.ModInterop;
using SkillUpgrades.Skills;

namespace SkillUpgrades
{
    [ModExportName("SkillUpgrades")]
    public static class Export
    {
        public static void OverrideSkillState(string Name, bool? state)
            => AbstractSkillUpgrade.OverrideSkillState(Name, state);

        public static void OverrideFieldValue(string SkillName, string FieldName, object value)
            => SkillUpgrades.GS.SetValue(SkillName, FieldName, value, SkillFieldSetOptions.ApplyToFieldValue);

        public static void ClearFieldOverride(string SkillName, string FieldName)
            => SkillUpgrades.GS.SetValue(SkillName, FieldName, null, SkillFieldSetOptions.Clear);
    }
}
