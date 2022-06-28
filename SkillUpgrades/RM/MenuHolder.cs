using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using MenuChanger.Extensions;
using RandomizerMod.Menu;
using static RandomizerMod.Localization;

namespace SkillUpgrades.RM
{
    public class MenuHolder
    {
        internal MenuPage MainPage;
        internal VerticalItemPanel suVIP;
        internal MenuElementFactory<RandoSettings> suMEF;

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
            suMEF = new(MainPage, RandomizerInterop.RandoSettings);
            suVIP = new(MainPage, new(0, 300), 50f, true, suMEF.Elements);
            Localize(suMEF);
        }
    }
}