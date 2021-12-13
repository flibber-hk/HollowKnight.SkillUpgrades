using System;

namespace SkillUpgrades
{
    /// <summary>
    /// Mark a skill field as being save-able and modifiable. Only applies to public static fields.
    /// Using SkillUpgradeSettings.SetValue is recommended to modify the value, so the mod knows that
    /// changes made e.g. through the menu shouldn't affect the current value.
    /// </summary>
    public abstract class DefaultValueAttribute : Attribute
    {
        public abstract object Value { get; }
        public bool MatchesGlobalSetting = true;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DefaultIntValueAttribute : DefaultValueAttribute
    {
        public int intValue;
        public override object Value => intValue;
        public DefaultIntValueAttribute(int value)
        {
            intValue = value;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DefaultBoolValueAttribute : DefaultValueAttribute
    {
        public bool boolValue;
        public override object Value => boolValue;
        public DefaultBoolValueAttribute(bool value)
        {
            boolValue = value;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DefaultFloatValueAttribute : DefaultValueAttribute
    {
        public float floatValue;
        public override object Value => floatValue;
        public DefaultFloatValueAttribute(float value)
        {
            floatValue = value;
        }
    }

    /// <summary>
    /// Mark a skill field as not saved (e.g. local save data). Such fields can safely be modified directly (e.g. ExtraAirDash.LocalExtraDashes++;)
    /// rather than going through SkillUpgradeSettings.SetValue. If this attribute is not applied, using SetValue is recommended.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NotSavedAttribute : Attribute { }

    /// <summary>
    /// Skill fields with this attribute will receive a toggle in the mod menu
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class MenuTogglableAttribute : Attribute
    {
        public string name;
        public string desc;

        public MenuTogglableAttribute(string name = null, string desc = "")
        {
            this.name = name;
            this.desc = desc;
        }
    }
}
