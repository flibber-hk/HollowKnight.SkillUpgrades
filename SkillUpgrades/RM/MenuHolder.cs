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
        internal MenuElementFactory<RandoSettings> suMEF;
        internal VerticalItemPanel suVIP;

        internal SmallButton JumpToDRPage;

        private static MenuHolder _instance = null;
        internal static MenuHolder Instance => _instance ?? (_instance = new MenuHolder());

        public static void OnExitMenu(Scene from, Scene to)
        {
            if (from.name == ItemChanger.SceneNames.Menu_Title) _instance = null;
        }

        public static void Hook()
        {
            RandomizerMenuAPI.AddMenuPage(Instance.ConstructMenu, Instance.HandleButton);
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnExitMenu;
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
            suMEF = new(MainPage, SkillUpgrades.GS.RandoSettings);
            suVIP = new(MainPage, new(0, 300), 50f, false, suMEF.Elements);
        }
    }
}