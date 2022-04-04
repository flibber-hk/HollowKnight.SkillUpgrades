using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace SkillUpgrades.RM
{
    public static class RequestMaker
    {
        public static void HookRequestBuilder()
        {
            // Early hook; do not mutate state, simply make the info about the items and their group available
            RequestBuilder.OnUpdate.Subscribe(-499.2f, SetupRefs);

            // Late-ish hook (shortly after base rando adds regular items); randomized items are added to the
            // randomization group we set earlier
            RequestBuilder.OnUpdate.Subscribe(0.8f, AddSkillUpgradesToPool);
        }

        private static void SetupRefs(RequestBuilder rb)
        {
            if (!RandomizerInterop.RandoSettings.Any) return;

            foreach (string skillName in GetActiveSettings())
            {
                rb.EditItemRequest(skillName, info =>
                {
                    info.getItemDef = () => new ItemDef()
                    {
                        Name = skillName,
                        Pool = "SkillUpgrades",
                        MajorItem = false, // true if progressive
                        PriceCap = 500,
                    };
                });
            }

            rb.OnGetGroupFor.Subscribe(0.8f, MatchSkillUpgrades);
            static bool MatchSkillUpgrades(RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder gb)
            {
                if (type == RequestBuilder.ElementType.Item || type == RequestBuilder.ElementType.Unknown)
                {
                    if (RandomizerInterop.RandoSettings.SkillSettings.Keys.Contains(item))
                    {
                        gb = rb.GetItemGroupFor(ItemChanger.ItemNames.Mothwing_Cloak);
                        return true;
                    }
                }
                gb = default;
                return false;
            }
        }

        private static void AddSkillUpgradesToPool(RequestBuilder rb)
        {
            if (!RandomizerInterop.RandoSettings.Any) return;

            foreach (string skillName in GetActiveSettings())
            {
                rb.AddItemByName(skillName);
            }
        }

        internal static IEnumerable<string> GetActiveSettings()
        {
            return RandomizerInterop.RandoSettings.SkillSettings
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .OrderBy(x => x);
        }
    }
}
