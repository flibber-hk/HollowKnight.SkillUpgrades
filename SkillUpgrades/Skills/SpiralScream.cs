using System;
using HutongGames.PlayMaker;
using UnityEngine;
using SkillUpgrades.FsmStateActions;
using Vasi;
using Modding.Utils;

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
                fsm.FsmVariables.GetFsmGameObject("Scr Heads").Value.GetOrAddComponent<Circler>().OnGetDirection += SetCirclerDirection;
                fsm.FsmVariables.GetFsmGameObject("Scr Heads 2").Value.GetOrAddComponent<Circler>().OnGetDirection += SetCirclerDirection;
            }));

            if (fsm.ActiveStateName != "Init" && fsm.ActiveStateName != "Pause")
            {
                fsm.FsmVariables.GetFsmGameObject("Scr Heads").Value.GetOrAddComponent<Circler>().OnGetDirection += SetCirclerDirection;
                fsm.FsmVariables.GetFsmGameObject("Scr Heads 2").Value.GetOrAddComponent<Circler>().OnGetDirection += SetCirclerDirection;
            }
        }

        public int SetCirclerDirection()
        {
            if (!SkillUpgradeActive)
            {
                return 0;
            }

            HeroActions ia = InputHandler.Instance.inputActions;
            if (ia.right.IsPressed && RightSpiralScreamAllowed) return -1;
            else if (ia.left.IsPressed && LeftSpiralScreamAllowed) return 1;
            else return 0;
        }
    }

    public class Circler : MonoBehaviour
    {
        private static bool madeWarning = false;

        private bool circled;
        private float angle;

        // It's kinda bad that we have to do an action like this but I want the code to run in Circler.OnEnable but depend on the
        // particular Spiral Scream instance (without having a static SpiralScream.instance or whatever)
        public event Func<int> OnGetDirection;

        private int GetDirection() 
        {
            if (OnGetDirection != null)
            {
                return OnGetDirection();
            }

            if (!madeWarning)
            {
                SkillUpgrades.skills[nameof(SpiralScream)].LogWarn("No GetDirection subscriber; not rotating...");
            }
            
            return 0;
        }

        public static float cycleTime = 0.45f;
        /// <summary>
        /// 1 -> Counter-clockwise
        /// 0 -> No rotation
        /// -1 -> Clockwise
        /// </summary>
        public int direction = 0;

        void OnEnable()
        {
            ResetRotation();
            angle = 0f;
            circled = false;
            direction = GetDirection();
        }

        void Update()
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

        void OnDestroy()
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
