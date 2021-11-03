using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SkillUpgrades.Util
{
    public static class Extensions
    {
        public static string FromCamelCase(this string s)
        {
            return Regex.Replace(s, "([A-Z])", " $1").TrimStart(' ');
        }

        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            if (!dict.TryGetValue(key, out TValue val))
            {
                val = defaultValue;
            }
            return val;
        }

        public static void EnsureInDict<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = defaultValue;
            }
        }
    }
}
