using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;

namespace SkillUpgrades
{
    public class GlobalSettings
    {
        [MenuToggleable("Global Toggle", "Turn this setting off to deactivate all skill upgrades.")]
        public bool GlobalToggle = true;

        #region Triple Jump
        // If this is set to true, the player can use wings multiple times in the air
        [MenuToggleable("Multiple Wings", "Toggle whether wings can be used more than once before landing.")]
        public bool TripleJumpEnabled = true;
        // The number of times the player can use wings in air is this int; setting it to -1 means wings can be used infinitely
        public int DoubleJumpMax = 2;
        #endregion

        #region Bonus Air Dash
        // If this is set to true, the player can dash multiple times in the air
        [MenuToggleable("Multiple Air Dash", "Toggle whether dash can be used more than once before landing.")]
        public bool BonusAirDashEnabled = true;
        // The number of times the player can dash in air is this int; setting it to -1 means dash can be used infinitely
        public int AirDashMax = 2;
        #endregion

        #region Vertical Cdash
        // If this is set to false, the player cannot cdash in non-horizontal directions
        [MenuToggleable("Vertical Superdash", "Toggle whether Crystal Heart can be used in non-horizontal directions.")]
        public bool VerticalSuperdashEnabled = true;
        // If this is set to true, the player can also cdash diagonally
        public bool DiagonalSuperdashEnabled = true;
        #endregion

        #region Horizontal Dive
        // If this is set to false, the player cannot dive in non-downward directions
        [MenuToggleable("Horizontal Dive", "Toggle whether Desolate Dive can be used horizontally.")]
        public bool HorizontalDiveEnabled = true;
        #endregion
    }

    public class MenuToggleable : Attribute
    {
        public string name;
        public string description;

        public MenuToggleable(string name, string description = "")
        {
            this.name = name;
            this.description = description;
        }
    }
}
