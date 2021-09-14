﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modding;
using UnityEngine;

namespace SkillUpgrades.Util
{
    internal static class HeroRotation
    {
        public static void Hook()
        {
            On.HeroController.SetupGameRefs += CreatePolygonColliderForHero;
        }

        private static PolygonCollider2D HeroCollider;
        
        // Data for resetting
        private static Vector2[] OriginalPoints;
        private static Vector2 OriginalOffset;


        private static void CreatePolygonColliderForHero(On.HeroController.orig_SetupGameRefs orig, HeroController self)
        {
            orig(self);

            // Get Data
            Collider2D col2d = self.GetComponent<Collider2D>();
            Vector2 heropos = self.transform.position;
            Vector2 center = col2d.bounds.center;
            Vector2 extents = col2d.bounds.extents;
            OriginalOffset = col2d.offset;

            // Set Data
            PolygonCollider2D newCol2d =  self.gameObject.AddComponent<PolygonCollider2D>();
            OriginalPoints = new Vector2[]
            {
                new Vector2(center.x + extents.x - heropos.x - OriginalOffset.x, center.y + extents.y - heropos.y - OriginalOffset.y),
                new Vector2(center.x + extents.x - heropos.x - OriginalOffset.x, center.y - extents.y - heropos.y - OriginalOffset.y),
                new Vector2(center.x - extents.x - heropos.x - OriginalOffset.x, center.y - extents.y - heropos.y - OriginalOffset.y),
                new Vector2(center.x - extents.x - heropos.x - OriginalOffset.x, center.y + extents.y - heropos.y - OriginalOffset.y)
            };
            newCol2d.SetPath(0, OriginalPoints);
            newCol2d.offset = OriginalOffset;

            // Switch to our new collider
            ReflectionHelper.SetField<HeroController, Collider2D>(self, "col2d", newCol2d);
            HeroCollider = newCol2d;
            UnityEngine.Object.Destroy(col2d);
        }

        /// <summary>
        /// Rotate the hero counterclockwise by angle degrees
        /// </summary>
        /// <param name="hero">HeroController.instance</param>
        /// <param name="angle">Angle to rotate</param>
        /// <param name="respectFacingDirection">If true, instead rotate clockwise when the hero is facing right</param>
        public static void RotateHero(this HeroController hero, float angle, bool respectFacingDirection = true)
        {
            Transform t = hero.wallPuffPrefab.transform.parent;
            hero.wallPuffPrefab.transform.parent = null;

            Vector2[] colliderBounds = HeroCollider.GetPath(0);
            float rotation = angle * (respectFacingDirection ? hero.transform.localScale.x : 1);
            hero.transform.Rotate(0, 0, rotation);
            HeroCollider.SetPath(0, ApplyRotationToPoints(colliderBounds, -rotation * hero.transform.localScale.x));

            HeroController.instance.wallPuffPrefab.transform.parent = t;
        }

        /// <summary>
        /// Reset the knight's rotation
        /// </summary>
        public static void ResetHero()
        {
            if (HeroController.instance == null) return;

            Transform t = HeroController.instance.wallPuffPrefab.transform.parent;
            HeroController.instance.wallPuffPrefab.transform.parent = null;

            HeroController.instance.transform.rotation = Quaternion.identity;
            HeroCollider.SetPath(0, OriginalPoints);
            HeroCollider.offset = OriginalOffset;

            HeroController.instance.wallPuffPrefab.transform.parent = t;
        }

        private static Vector2[] ApplyRotationToPoints(Vector2[] points, float rotation)
        {
            float radians = rotation * Mathf.PI / 180f;

            return points
                .Select(v => v + OriginalOffset)
                .Select(v => new Vector2(Mathf.Cos(radians)*v.x - Mathf.Sin(radians)*v.y, Mathf.Sin(radians) * v.x + Mathf.Cos(radians) * v.y))
                .Select(v => v - OriginalOffset)
                .ToArray();
        }
    }
}