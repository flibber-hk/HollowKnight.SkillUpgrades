using System.Collections.Generic;
using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using MenuChanger.Extensions;
using RandomizerMod.Menu;
using UnityEngine.SceneManagement;

namespace SkillUpgrades.RM
{
    public class MenuHolder
    {
        internal MenuPage MainPage;
        internal List<IValueElement> suElements;
        internal VerticalItemPanel suVIP;

        internal SmallButton JumpToDRPage;

        private static MenuHolder _instance = null;
        internal static MenuHolder Instance => _instance ?? (_instance = new MenuHolder());

        public static void OnExitMenu()
        {
            _instance = null;
        }

        public static void Hook()
        {
            RandomizerMenuAPI.AddMenuPage(Instance.ConstructMenu, Instance.HandleButton);
            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        private bool HandleButton(MenuPage landingPage, out SmallButton button)
        {
            JumpToDRPage = new(landingPage, "SkillUpgrades");
            JumpToDRPage.AddHideAndShowEvent(landingPage, MainPage);
            button = JumpToDRPage;
            return true;
        }

        private void ConstructMenu(MenuPage landingPage)
        {
            MainPage = new MenuPage("SkillUpgrades", landingPage);
            
            suElements = new();
            foreach (var kvp in RandomizerInterop.RandoSettings.SkillSettings)
            {
                ToggleButton button = new(MainPage, kvp.Key);
                button.SetValue(kvp.Value);
                button.SelfChanged += b => RandomizerInterop.RandoSettings.SkillSettings[kvp.Key] = (bool)b.Value;
                suElements.Add(button);
            }

            suVIP = new(MainPage, new(0, 300), 50f, false, suElements.ToArray());
        }
    }
}