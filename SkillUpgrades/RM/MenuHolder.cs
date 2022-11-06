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

        internal static MenuHolder Instance { get; private set; }

        public static void OnExitMenu()
        {
            Instance = null;
        }

        public static void Hook()
        {
            RandomizerMenuAPI.AddMenuPage(ConstructMenu, HandleButton);
            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        private static bool HandleButton(MenuPage landingPage, out SmallButton button)
        {
            button = Instance.JumpToSUPage;
            return true;
        }

        private void SetTopLevelButtonColor()
        {
            if (JumpToSUPage != null)
            {
                JumpToSUPage.Text.color = RandomizerInterop.RandoSettings.Any ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
        }

        private static void ConstructMenu(MenuPage landingPage) => Instance = new(landingPage);
        
        private MenuHolder(MenuPage landingPage)
        {
            MainPage = new MenuPage("SkillUpgrades", landingPage);
            suMEF = new(MainPage, RandomizerInterop.RandoSettings);
            suVIP = new(MainPage, new(0, 300), 50f, true, suMEF.Elements);
            foreach (IValueElement e in suMEF.Elements)
            {
                e.SelfChanged += obj => SetTopLevelButtonColor();
            }

            Localize(suMEF);

            JumpToSUPage = new(landingPage, "SkillUpgrades");
            JumpToSUPage.AddHideAndShowEvent(landingPage, MainPage);
            SetTopLevelButtonColor();
        }

        internal void ResetMenu()
        {
            suMEF.SetMenuValues(RandomizerInterop.RandoSettings);
        }
    }
}