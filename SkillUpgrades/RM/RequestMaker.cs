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

            foreach (string skillName in SkillUpgrades._skills.Keys)
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
                    if (SkillUpgrades._skills.Keys.Contains(item))
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

            switch (RandomizerInterop.RandoSettings.MainSetting)
            {
                case MainSkillUpgradeRandoType.All:
                    foreach (string skillName in SkillUpgrades._skills.Keys)
                    {
                        rb.AddItemByName(skillName);
                    }
                    break;
                case MainSkillUpgradeRandoType.RandomSelection:
                    foreach (string skillName in SkillUpgrades._skills.Keys)
                    {
                        if (rb.rng.NextDouble() < 0.666f) rb.AddItemByName(skillName);
                    }
                    break;
                case MainSkillUpgradeRandoType.EnabledSkills:
                    foreach (string skillName in SkillUpgrades._skills.Keys)
                    {
                        if (SkillUpgrades.GS.EnabledSkills.TryGetValue(skillName, out bool enabled) && enabled)
                        {
                            rb.AddItemByName(skillName);
                        }
                    }
                    break;
            }
        }
    }
}
