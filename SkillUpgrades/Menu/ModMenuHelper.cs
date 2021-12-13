using System;
using System.Collections.Generic;
using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine;
using UnityEngine.UI;

namespace SkillUpgrades.Menu
{
    public static class ModMenuHelper
    {
        /// <summary>
        /// Creates a MenuBuilder object with the default size and position data, but no content.
        /// </summary>
        /// <param name="title">The title of the menu screen.</param>
        /// <param name="returnScreen">The screen that the back button will return to.</param>
        /// <param name="backButton">The back button.</param>
        public static MenuBuilder CreateMenuBuilder(string title, MenuScreen returnScreen, out MenuButton backButton)
        {
            MenuBuilder builder = new MenuBuilder(title);
            builder.CreateTitle(title, MenuTitleStyle.vanillaStyle);
            builder.CreateContentPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 903f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -60f)
                    )
                ));
            builder.CreateControlPane(RectTransformData.FromSizeAndPos(
                new RelVector2(new Vector2(1920f, 259f)),
                new AnchoredPosition(
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -502f)
                )
            ));
            builder.SetDefaultNavGraph(new ChainedNavGraph());

            MenuButton _back = null;
            builder.AddControls(
                new SingleContentLayout(new AnchoredPosition(
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -64f)
                )),
                c => c.AddMenuButton(
                    "BackButton",
                    new MenuButtonConfig
                    {
                        Label = "Back",
                        CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(returnScreen),
                        SubmitAction = _ => UIManager.instance.UIGoToDynamicMenu(returnScreen),
                        Style = MenuButtonStyle.VanillaStyle,
                        Proceed = true
                    },
                    out _back
                ));

            backButton = _back;
            return builder;
        }

        /// <summary>
        /// Creates a Menu Screen with a list of MenuOptionHorizontals, with the default size and position data.
        /// </summary>
        /// <param name="title">The title of the menu screen.</param>
        /// <param name="returnScreen">The screen that the back button will return to.</param>
        /// <param name="entries">A list of IMenuMod.MenuEntry objects corresponding to the buttons.</param>
        /// <param name="RefreshMenu">When invoked, will refresh the menu screen according to the entries' loaders.</param>
        public static MenuScreen CreateMenuScreen(string title, MenuScreen returnScreen, List<IMenuMod.MenuEntry> entries, out Action RefreshMenu)
        {
            MenuBuilder builder = CreateMenuBuilder(title, returnScreen, out MenuButton backButton);

            RefreshMenu = null;

            if (entries.Count > 5)
            {
                Action _refresh = null;
                builder.AddContent(new NullContentLayout(), c => c.AddScrollPaneContent(
                    new ScrollbarConfig
                    {
                        CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(returnScreen),
                        Navigation = new Navigation
                        {
                            mode = Navigation.Mode.Explicit,
                            selectOnUp = backButton,
                            selectOnDown = backButton
                        },
                        Position = new AnchoredPosition
                        {
                            ChildAnchor = new Vector2(0f, 1f),
                            ParentAnchor = new Vector2(1f, 1f),
                            Offset = new Vector2(-310f, 0f)
                        }
                    },
                    new RelLength(entries.Count * 105f),
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c => AddMenuEntriesToContentArea(c, entries, returnScreen, out _refresh)
                ));
                RefreshMenu = _refresh;
            }
            else
            {
                Action _refresh = null;
                builder.AddContent(
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c => AddMenuEntriesToContentArea(c, entries, returnScreen, out _refresh)
                );
                RefreshMenu = _refresh;
            }

            return builder.Build();
        }

        /// <summary>
        /// Adds the menu entries to the content area.
        /// </summary>
        public static void AddMenuEntriesToContentArea(ContentArea c, List<IMenuMod.MenuEntry> entries, MenuScreen returnScreen, out Action RefreshMenu)
        {
            RefreshMenu = null;

            foreach (IMenuMod.MenuEntry entry in entries)
            {
                HorizontalOptionConfig config = new HorizontalOptionConfig
                {
                    ApplySetting = (_, i) => entry.Saver(i),
                    RefreshSetting = (s, _) => s.optionList.SetOptionTo(entry.Loader()),
                    CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(returnScreen),
                    Description = string.IsNullOrEmpty(entry.Description) ? null : new DescriptionInfo
                    {
                        Text = entry.Description
                    },
                    Label = entry.Name,
                    Options = entry.Values,
                    Style = HorizontalOptionStyle.VanillaStyle
                };

                c.AddHorizontalOption(entry.Name, config, out MenuOptionHorizontal option);
                option.menuSetting.RefreshValueFromGameSettings();
                RefreshMenu += () => option.menuSetting.RefreshValueFromGameSettings();
            }
        }
    }
}