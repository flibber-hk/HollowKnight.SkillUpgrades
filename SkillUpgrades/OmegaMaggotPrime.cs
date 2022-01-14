using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SkillUpgrades
{
    internal static class OmegaMaggotPrime
    {
        internal static IEnumerable<Type> _GetTypesSafely(this Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (Type type in ex.Types.Where(x => x is not null))
                {
                    SkillUpgrades.instance.Log(type.FullName);
                }
                return ex.Types.Where(x => x is not null);
            }
        }
    }
}
