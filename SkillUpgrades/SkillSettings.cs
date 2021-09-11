using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Modding;

namespace SkillUpgrades
{
     [Serializable]
    public class GlobalSettings
    {
        private readonly Assembly _asm = Assembly.GetAssembly(typeof(GlobalSettings));

        internal readonly Dictionary<FieldInfo, Type> Fields = new Dictionary<FieldInfo, Type>();

        public Dictionary<string, bool?> EnabledSkills { get; set; } = new Dictionary<string, bool?>();

        public Dictionary<string, bool> Booleans { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, float> Floats { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, int> Integers { get; set; } = new Dictionary<string, int>();

        public bool GlobalToggle = true; // TODO: Implement this

        public GlobalSettings()
        {
            foreach (Type t in _asm.GetTypes())
            {
                foreach (FieldInfo fi in t.GetFields().Where(x => Attribute.IsDefined(x, typeof(SerializeToSetting))))
                {
                    Fields.Add(fi, t);
                }
            }
        }

        [OnSerializing]
        public void OnBeforeSerialize(StreamingContext _)
        {
            foreach (var (fi, type) in Fields)
            {
                if (fi.FieldType == typeof(bool))
                {
                    Booleans[$"{type.Name}:{fi.Name}"] = (bool)fi.GetValue(null);
                }
                else if (fi.FieldType == typeof(float))
                {
                    Floats[$"{type.Name}:{fi.Name}"] = (float)fi.GetValue(null);
                }
                else if (fi.FieldType == typeof(int))
                {
                    Integers[$"{type.Name}:{fi.Name}"] = (int)fi.GetValue(null);
                }
            }
        }


        [OnDeserialized]
        public void OnAfterDeserialize(StreamingContext _)
        {
            foreach (var (fi, type) in Fields)
            {
                if (fi.FieldType == typeof(bool))
                {
                    if (Booleans.TryGetValue($"{type.Name}:{fi.Name}", out bool val)) fi.SetValue(null, val);
                }
                else if (fi.FieldType == typeof(float))
                {
                    if (Floats.TryGetValue($"{type.Name}:{fi.Name}", out float val)) fi.SetValue(null, val);
                }
                else if (fi.FieldType == typeof(int))
                {
                    if (Integers.TryGetValue($"{type.Name}:{fi.Name}", out int val)) fi.SetValue(null, val);
                }
            }
        }
    }

    public class SerializeToSetting : Attribute { }
}
