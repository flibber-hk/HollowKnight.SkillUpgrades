using System;
using System.Linq;
using System.Reflection;
using RandomizerMod.RC;

namespace SkillUpgrades.RM
{
    public static class RequestMaker
    {
        public static void Hook()
        {
            RequestBuilder.OnUpdate.Subscribe(0.8f, AddSkillUpgradesToPool);
        }

        private static void AddSkillUpgradesToPool(RequestBuilder rb)
        {
            if (!RandomizerInterop.RandoSettings.Any) return;

            foreach (string skillName in typeof(RandoSettings)
                .GetFields()
                .Where(fi => fi.FieldType == typeof(bool) && (bool)fi.GetValue(RandomizerInterop.RandoSettings))
                .Select(x => x.Name))
            {
                rb.MainItemGroup.Items.Add(skillName);
            }
        }
    }
}
