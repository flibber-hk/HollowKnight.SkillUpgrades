using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SkillUpgrades.Util
{
    internal static class GameObjectExtensions
    {
        /// <summary>
        /// Rotate the hero counterclockwise by angle degrees
        /// </summary>
        /// <param name="hero">HeroController.instance</param>
        /// <param name="angle">Angle to rotate</param>
        /// <param name="respectDirection">If true, instead rotate clockwise when the hero is facing right</param>
        public static void RotateHero(this HeroController hero, float angle, bool respectDirection = true)
        {
            float rotation = angle * (respectDirection ? hero.transform.localScale.x : 1);
            hero.transform.Rotate(0, 0, rotation);
        }
    }
}
