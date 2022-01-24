using System;
using System.Collections.Generic;
using GlobalEnums;
using ItemChanger;
using ItemChanger.Modules;
using ItemChanger.UIDefs;
using SkillUpgrades.IC.Items;

namespace SkillUpgrades.IC
{
    public class ToggleString : IString
    {
        public string boolName;
        public ToggleString(string boolName)
        {
            this.boolName = boolName;
        }
        public IString Clone() => (IString)MemberwiseClone();
        public string Value => PlayerData.instance.GetBool(boolName)
            ? "Enable this skill upgrade through the mod menu."
            : "Enable this skill upgrade through the mod menu once you find the base ability.";
    }
    public class SpellToggleString : IString
    {
        public string fieldName;
        public SpellToggleString(string fieldName)
        {
            this.fieldName = fieldName;
        }
        public IString Clone() => (IString)MemberwiseClone();
        public string Value => PlayerData.instance.GetInt(fieldName) > 0
            ? "Enable this skill upgrade through the mod menu."
            : "Enable this skill upgrade through the mod menu once you find the base ability.";
    }

    public static class ItemChangerInterop
    {
        public static void HookItemChanger()
        {
            DefineSkillUpgradeUnlockItems();
            Events.AfterStartNewGame += ReflectOneways;
        }

        /// <summary>
        /// If x -> y is a one way, and the one way transitions are coupled, then also have y -> x.
        /// Check for coupled through the CompletionPercentOverride module; this is enabled in rando,
        /// and in non-rando we can expect users to override these transitions themselves if they want
        /// </summary>
        private static void ReflectOneways()
        {
            if (ItemChangerMod.Modules.Get<CompletionPercentOverride>() is CompletionPercentOverride cpo && cpo.CoupledTransitions)
            {
                List<(string scene, string gate)> OneWayTransitions = new()
                {
                    (SceneNames.Cliffs_02, "bot2"),
                    (SceneNames.Cliffs_02, "right1"),
                    (SceneNames.Mines_34, "bot2"),
                    (SceneNames.Mines_34, "left1"),
                    (SceneNames.Deepnest_East_07, "bot1"),
                    (SceneNames.Fungus2_30, "bot1"),
                    (SceneNames.Deepnest_01, "bot2"),
                    (SceneNames.Mines_28, "bot1"),
                };

                foreach ((string scene, string gate) in OneWayTransitions)
                {
                    Transition source = new(scene, gate);

                    if (ItemChanger.Internal.Ref.Settings.TransitionOverrides.TryGetValue(source, out ITransition t) 
                        && t is Transition target
                        && !ItemChanger.Internal.Ref.Settings.TransitionOverrides.ContainsKey(target))
                    {
                        ItemChangerMod.AddTransitionOverride(target, source);
                        cpo.SetTransitionWeight(target, 0);
                    }
                }
            }

            
        }

        /// <summary>
        /// Define the skill upgrade unlock items. Tags will not be applied here, but rather through a
        /// subscriber to ItemChanger's Finder GetItem hook.
        /// </summary>
        public static void DefineSkillUpgradeUnlockItems()
        {
            List<AbstractItem> items = new();

            IString ShopDesc = new BoxedString("This isn't in the vanilla game!");

            void CreateSkillUpgrade(string skillName, string uiname, string template, HeroActionButton? action, string desc, string boolName, bool spell)
            {
                BigUIDef def = Finder.GetItem(template).UIDef.Clone() as BigUIDef;
                def.name = new BoxedString(uiname);
                if (action.HasValue)
                {
                    def.buttonSkin = new HeroActionButtonSkin(action.Value);
                    def.press ??= new LanguageString("Prompts", "BUTTON_DESC_PRESS");
                }
                else
                {
                    def.buttonSkin = null;
                    def.press = null;
                }
                def.descOne = new BoxedString(desc);
                def.descTwo = spell ? new SpellToggleString(boolName) : new ToggleString(boolName);
                def.take = new LanguageString("Prompts", "GET_ITEM_INTRO1");
                def.shopDesc = new BoxedString("This isn't in the vanilla game!");

                items.Add(new SkillUpgradeItem()
                {
                    name = skillName,
                    SkillName = skillName,
                    AllowToggle = true,
                    UIDef = def
                });
            }

            CreateSkillUpgrade(nameof(Skills.DirectionalDash), "Directional Dash", ItemNames.Mothwing_Cloak, HeroActionButton.DASH,
                "while holding a direction to dash in that direction.", nameof(PlayerData.hasDash), false);
            CreateSkillUpgrade(nameof(Skills.ExtraAirDash), "Extra Air Dash", ItemNames.Mothwing_Cloak, HeroActionButton.DASH,
                "to dash a second time in the air.", nameof(PlayerData.hasDash), false);
            CreateSkillUpgrade(nameof(Skills.WallClimb), "Wall Climb", ItemNames.Mantis_Claw, null,
                "Grab onto walls without slipping.", nameof(PlayerData.hasWalljump), false);
            CreateSkillUpgrade(nameof(Skills.VerticalSuperdash), "Vertical Superdash", ItemNames.Crystal_Heart, HeroActionButton.SUPER_DASH,
                "to superdash upwards.", nameof(PlayerData.hasSuperDash), false);
            CreateSkillUpgrade(nameof(Skills.TripleJump), "Triple Jump", ItemNames.Monarch_Wings, HeroActionButton.JUMP,
                "to jump a second time in the air.", nameof(PlayerData.hasDoubleJump), false);
            CreateSkillUpgrade(nameof(Skills.DownwardFireball), "Downward Fireball", ItemNames.Vengeful_Spirit, HeroActionButton.QUICK_CAST,
                "without holding a direction to shoot fireballs downwards.", nameof(PlayerData.fireballLevel), true);
            CreateSkillUpgrade(nameof(Skills.HorizontalDive), "Horizontal Dive", ItemNames.Desolate_Dive, HeroActionButton.QUICK_CAST,
                "while holding left or right to dive horizontally.", nameof(PlayerData.quakeLevel), true);
            CreateSkillUpgrade(nameof(Skills.SpiralScream), "Spiral Scream", ItemNames.Howling_Wraiths, HeroActionButton.QUICK_CAST,
                "while holding left or right to scream in a circle.", nameof(PlayerData.screamLevel), true);

            foreach (AbstractItem item in items)
            {
                Finder.DefineCustomItem(item);
            }
        }
    }
}
