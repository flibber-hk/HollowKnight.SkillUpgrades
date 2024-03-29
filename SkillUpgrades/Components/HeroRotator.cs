﻿using System;
using Modding;
using UnityEngine;
using SkillUpgrades.Util;

namespace SkillUpgrades.Components
{
    public class HeroRotator : StableRotator
    {
        public static void Hook()
        {
            On.HeroController.SetupGameRefs += (orig, self) =>
            {
                orig(self);
                self.gameObject.AddComponent<HeroRotator>();
            };
        }

        /// <summary>
        /// Event invoked when the hero's rotation is reset.
        /// </summary>
        public static event Action OnHeroResetRotation;
        /// <summary>
        /// Event invoked when the hero is rotated. The total rotation angle (not the angle of this rotation) is passed in as the parameter.
        /// </summary>
        public static event Action<float> OnHeroRotate;

        public static HeroRotator Instance { get; private set; }

        public override void Awake()
        {
            // This component should only be added to the knight
            if (GetComponent<HeroController>() != HeroController.instance)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            base.Awake();

            ReflectionHelper.SetField<HeroController, Collider2D>(HeroController.instance, "col2d", _collider);
            Ref.HeroCollider = _collider;
        }

        public override void Rotate(float angle, bool respectFacingDirection = true)
        {
            Transform t = HeroController.instance.vignette.transform.parent;
            HeroController.instance.vignette.transform.SetParent(null);

            base.Rotate(angle, respectFacingDirection);

            HeroController.instance.vignette.transform.SetParent(t);

            OnHeroRotate?.Invoke(transform.rotation.z);
        }

        public override void ResetRotation()
        {
            base.ResetRotation();

            HeroController.instance.wallPuffPrefab.transform.rotation = Quaternion.identity;
            HeroController.instance.vignette.transform.rotation = Quaternion.identity;

            OnHeroResetRotation?.Invoke();
        }
    }
}
