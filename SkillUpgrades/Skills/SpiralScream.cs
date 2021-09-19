using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HutongGames.PlayMaker;
using UnityEngine;
using SkillUpgrades.Util;

namespace SkillUpgrades.Skills
{
    internal class SpiralScream : AbstractSkillUpgrade
    {
        public override string Name => "Spiral Scream";
        public override string Description => "Toggle whether Howling Wraiths can sweep a circle around the knight";

        public override void Initialize()
        {
            On.HeroController.Start += HeroController_Start;
            if (HeroController.instance is HeroController hero)
            {
                EnableSpiralScream(hero);
            }
        }

        public override void Unload()
        {
            On.HeroController.Start -= HeroController_Start;
            if (HeroController.instance is HeroController hero)
            {
                DisableSpiralScream(hero);
            }
            Circler.direction = 0;
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

            FsmState screamGet = fsm.GetState("Scream Get?");
            screamGet.AddFirstAction(new ExecuteLambda(() =>
            {
                if (!skillUpgradeActive)
                {
                    Circler.direction = 0;
                    return;
                }

                HeroActions ia = InputHandler.Instance.inputActions;
                if (ia.right.IsPressed) Circler.direction = -1;
                else if (ia.left.IsPressed) Circler.direction = 1;
                else Circler.direction = 0;
            }));
        }

        private void DisableSpiralScream(HeroController hero)
        {
            PlayMakerFSM fsm = hero.spellControl;

            fsm.GetState("Init").RemoveActionsOfType<ExecuteLambda>();
            fsm.GetState("Scream Get?").RemoveActionsOfType<ExecuteLambda>();

            foreach (Circler c in Resources.FindObjectsOfTypeAll<Circler>())
            {
                UnityEngine.Object.Destroy(c);
            }
        }
    }

    public class Circler : MonoBehaviour
    {
        private bool circled;
        private float angle;
        
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
