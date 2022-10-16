using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Modding;
using SkillUpgrades.Skills;

namespace SkillUpgrades
{
    public enum SkillFieldSetOptions
    {
        /// <summary>
        /// Force the field to match the global setting. In this case, the `value` argument is ignored.
        /// </summary>
        Clear = 0,
        /// <summary>
        /// Modify the global setting. Also modify the field value if that's unchanged.
        /// If this option is not applied, mark the field as changed (unless Clear is passed).
        /// </summary>
        ApplyToGlobalSetting = 1,
        /// <summary>
        /// Modify the field value, but not the global setting.
        /// </summary>
        ApplyToFieldValue = 2,
    }

    [Serializable]
    public class SkillUpgradeSettings
    {
        #region API

        /// <summary>
        /// Modify the value of a skill according to options.
        /// Clear: revert the field value to the global setting, or the default value if that's unset, and mark it as unmodified.
        /// ApplyToGlobalSetting: modify the global setting value, and modify the field value unless it's marked as modified.
        /// ApplyToFieldValue: modify the field value, and mark it as modified.
        /// </summary>
        [PublicAPI]
        public void SetValue(string skillName, string fieldName, object value, SkillFieldSetOptions options = SkillFieldSetOptions.ApplyToFieldValue)
        {
            string key = GetKey(skillName, fieldName);
            SetValue(key, value, options);
        }
        public void SetValue(string key, object value, SkillFieldSetOptions options)
        {
            if (!Fields.TryGetValue(key, out FieldInfo fi))
            {
                SkillUpgrades.instance.LogError($"SetValue: key {key} not found.");
            }

            if (options == SkillFieldSetOptions.Clear)
            {
                fi.SetValue(null, GetDefaultValue(key));
                fi.GetCustomAttribute<DefaultValueAttribute>().MatchesGlobalSetting = true;
                return;
            }

            else if (options == SkillFieldSetOptions.ApplyToGlobalSetting)
            {
                if (fi.GetCustomAttribute<NotSavedAttribute>() is not null) return;

                if (fi.FieldType == typeof(int)) Integers[key] = (int)value;
                else if (fi.FieldType == typeof(bool)) Booleans[key] = (bool)value;
                else if (fi.FieldType == typeof(float)) Floats[key] = (float)value;

                if (fi.GetCustomAttribute<DefaultValueAttribute>().MatchesGlobalSetting)
                {
                    fi.SetValue(null, value);
                }
            }
            else
            {
                fi.GetCustomAttribute<DefaultValueAttribute>().MatchesGlobalSetting = false;
                fi.SetValue(null, value);
            }
        }

        internal object GetDefaultValue(string skillName, string fieldName)
        {
            return GetDefaultValue(GetKey(skillName, fieldName));
        }
        internal object GetDefaultValue(string key)
        {
            if (Integers.TryGetValue(key, out int intVal)) return intVal;
            else if (Booleans.TryGetValue(key, out bool boolVal)) return boolVal;
            else if (Floats.TryGetValue(key, out float floatVal)) return floatVal;

            return Fields[key].GetCustomAttribute<DefaultValueAttribute>().Value;
        }
        #endregion

        internal static Dictionary<string, FieldInfo> Fields;
        static SkillUpgradeSettings()
        {
            Fields = new Dictionary<string, FieldInfo>();

            foreach (Type t in AbstractSkillUpgrade.GetAvailableSkillUpgradeTypes())
            {
                foreach (FieldInfo fi in t.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (fi.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute dva)
                    {
                        Fields.Add(GetKey(fi), fi);
                        fi.SetValue(null, dva.Value);
                    }
                }
            }
        }

        internal static string GetKey(string skillName, string fieldName)
        {
            return $"{skillName}:{fieldName}";
        }
        internal static string GetKey(FieldInfo fi)
        {
            string skillName = fi.DeclaringType.Name;
            string fieldName = fi.Name;
            return GetKey(skillName, fieldName);
        }

        public bool GlobalToggle = true;   
        public Dictionary<string, bool> EnabledSkills { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, bool> Booleans { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, float> Floats { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, int> Integers { get; set; } = new Dictionary<string, int>();

        // If at least three skills are turned off, new skills will be disabled by default.
        // This means that for a new global settings file, skills are *enabled* by default.
        public bool DefaultSkillSetting => EnabledSkills.Where(x => !x.Value).Count() < 3;

        public RM.RandoSettings RandoSettings { get; set; } = new();

        private IEnumerable<(string, object)> AllSkillData()
        {
            foreach (var kvp in Booleans)
            {
                yield return (kvp.Key, kvp.Value);
            }
            foreach (var kvp in Integers)
            {
                yield return (kvp.Key, kvp.Value);
            }
            foreach (var kvp in Floats)
            {
                yield return (kvp.Key, kvp.Value);
            }
        }

        // Serialize default values if they're not present in the dictionary.
        // We can choose whether a particular set will affect the global settings (e.g. menu) or not (file data)
        [OnSerializing]
        public void OnBeforeSerialize(StreamingContext _)
        {
            foreach (var kvp in Fields)
            {
                if (kvp.Value.GetCustomAttribute<NotSavedAttribute>() is not null) continue;

                if (kvp.Value.FieldType == typeof(int))
                {
                    if (Integers.ContainsKey(kvp.Key)) continue;
                    Integers[kvp.Key] = kvp.Value.GetCustomAttribute<DefaultIntValueAttribute>().intValue;
                }
                else if (kvp.Value.FieldType == typeof(bool))
                {
                    if (Booleans.ContainsKey(kvp.Key)) continue;
                    Booleans[kvp.Key] = kvp.Value.GetCustomAttribute<DefaultBoolValueAttribute>().boolValue;
                }
                else if (kvp.Value.FieldType == typeof(float))
                {
                    if (Floats.ContainsKey(kvp.Key)) continue;
                    Floats[kvp.Key] = kvp.Value.GetCustomAttribute<DefaultFloatValueAttribute>().floatValue;
                }
            }
        }

        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext _)
        {
            foreach ((string key, object value) in AllSkillData())
            {
                if (Fields.TryGetValue(key, out FieldInfo fi))
                {
                    fi.SetValue(null, value);
                }
            }
        }
    }
}
