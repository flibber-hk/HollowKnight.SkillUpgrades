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

        internal SmallButton JumpToSUPage;

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
            JumpToSUPage = new(landingPage, "SkillUpgrades");
            JumpToSUPage.AddHideAndShowEvent(landingPage, MainPage);
            SetTopLevelButtonColor();

            button = JumpToSUPage;
            return true;
        }

        private void SetTopLevelButtonColor()
        {
            if (JumpToSUPage != null)
            {
                JumpToSUPage.Text.color = RandomizerInterop.RandoSettings.Any ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
        }

        private void ConstructMenu(MenuPage landingPage)
        {
            MainPage = new MenuPage("SkillUpgrades", landingPage);
            suMEF = new(MainPage, RandomizerInterop.RandoSettings);
            suVIP = new(MainPage, new(0, 300), 50f, true, suMEF.Elements);
            foreach (IValueElement e in suMEF.Elements)
            {
                e.SelfChanged += obj => SetTopLevelButtonColor();
            }

            Localize(suMEF);
        }

        internal void ResetMenu()
        {
            suMEF.SetMenuValues(RandomizerInterop.RandoSettings);
        }
    }
}