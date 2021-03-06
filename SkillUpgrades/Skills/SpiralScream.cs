using System;
using HutongGames.PlayMaker;
using UnityEngine;
using SkillUpgrades.FsmStateActions;
using SkillUpgrades.Util;

namespace SkillUpgrades.Skills
{
    public class SpiralScream : AbstractSkillUpgrade
    {
        [DefaultBoolValue(true)]
        [NotSaved]
        public static bool LeftSpiralScreamAllowed;
        [DefaultBoolValue(true)]
        [NotSaved]
        public static bool RightSpiralScreamAllowed;


        public override string Description => "Toggle whether Howling Wraiths can sweep a circle around the knight";

        protected override void StartUpInitialize()
        {
            Circler.SetDirection = SetCirclerDirection;
            On.HeroController.Start += HeroController_Start;
        }

        private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            EnableSpiralScream(self);
        }

        private void EnableSpiralScream(HeroController hero)
        {
            PlayMakerFSM fsm = hero.spellControl;

            FsmState init = fsm.GetState("Init");
            init.AddAction(new ExecuteLambda(() =>
            {
                fsm.FsmVariables.GetFsmGameObject("Scr Heads").Value.AddComponent<Circler>();
                fsm.FsmVariables.GetFsmGameObject("Scr Heads 2").Value.AddComponent<Circler>();
            }));

            if (fsm.ActiveStateName != "Init" && fsm.ActiveStateName != "Pause")
            {
                fsm.FsmVariables.GetFsmGameObject("Scr Heads").Value.AddComponent<Circler>();
                fsm.FsmVariables.GetFsmGameObject("Scr Heads 2").Value.AddComponent<Circler>();
            }
        }

        public void SetCirclerDirection()
        {
            if (!SkillUpgradeActive)
            {
                Circler.direction = 0;
                return;
            }

            HeroActions ia = InputHandler.Instance.inputActions;
            if (ia.right.IsPressed && RightSpiralScreamAllowed) Circler.direction = -1;
            else if (ia.left.IsPressed && LeftSpiralScreamAllowed) Circler.direction = 1;
            else Circler.direction = 0;
        }
    }

    public class Circler : MonoBehaviour
    {
        private bool circled;
        private float angle;

        // It's kinda bad that we have to do an action like this but I want the code to run in Circler.OnEnable but depend on the
        // particular Spiral Scream instance (without having a static SpiralScream.instance or whatever)
        public static Action SetDirection;

        public static float cycleTime = 0.45f;
        /// <summary>
        /// 1 -> Counter-clockwise
        /// 0 -> No rotation
        /// -1 -> Clockwise
        /// </summary>
        public static int direction = 0;

        public void OnEnable()
        {
            ResetRotation();
            angle = 0f;
            circled = false;
            SetDirection();
        }

        public void Update()
        {
            if (circled) return;
            float rotateAngle = 360f * (Time.deltaTime / cycleTime) * direction;
            angle += Math.Abs(rotateAngle);
            gameObject.transform.RotateAround(HeroController.instance.transform.position, Vector3.forward, rotateAngle);

            if (angle >= 360f)
            {
                circled = true;
                ResetRotation();
            }
        }

        public void OnDestroy()
        {
            ResetRotation();
            angle = 0f;
            circled = false;
        }

        public void ResetRotation()
        {
            gameObject.transform.rotation = Quaternion.identity;
        }
    }
}
