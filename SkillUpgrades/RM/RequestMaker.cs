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
        public static void Hook()
        {
            RequestBuilder.OnUpdate.Subscribe(0.8f, AddSkillUpgradesToPool);
        }

        private static void AddSkillUpgradesToPool(RequestBuilder rb)
        {
            if (!RandomizerInterop.RandoSettings.Any) return;

            ItemGroupBuilder skillGroup = rb.GetItemGroupFor(ItemChanger.ItemNames.Mothwing_Cloak);
            List<string> randomizedSkillUpgrades = new();

            foreach (string skillName in RandomizerInterop.RandoSettings.SkillSettings
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key))
            {
                skillGroup.Items.Add(skillName);
                randomizedSkillUpgrades.Add(skillName);

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

            bool MatchSkillUpgrades(RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder gb)
            {
                if (type == RequestBuilder.ElementType.Item || type == RequestBuilder.ElementType.Unknown)
                {
                    if (randomizedSkillUpgrades.Contains(item))
                    {
                        gb = skillGroup;
                        return true;
                    }
                }
                gb = default;
                return false;
            }
        }
    }
}
