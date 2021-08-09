using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;

namespace SkillUpgrades
{
    public class GlobalSettings
    {
        #region Triple Jump
        // If this is set to true, the player can use wings multiple times in the air
        public bool TripleJumpEnabled = true;
        // The number of times the player can use wings in air is this int; setting it to -1 means wings can be used infinitely
        public int DoubleJumpMax = 2;
        #endregion

        #region Bonus Air Dash
        // If this is set to true, the player can dash multiple times in the air
        public bool BonusAirDashEnabled = true;
        // The number of times the player can dash in air is this int; setting it to -1 means dash can be used infinitely
        public int AirDashMax = 2;
        #endregion

        #region Vertical Cdash
        // If this is set to false, the player cannot cdash in non-horizontal directions
        public bool VerticalSuperdashEnabled = true;
        // If this is set to true, the player can also cdash diagonally
        public bool DiagonalSuperdashEnabled = true;
        #endregion

        #region Horizontal Dive
        // If this is set to false, the player cannot dive in non-downward directions
        public bool HorizontalDiveEnbled = true;
        #endregion
    }
}
