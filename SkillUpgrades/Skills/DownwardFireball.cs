using System;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using SkillUpgrades.FsmStateActions;
using SkillUpgrades.Util;
using Vasi;

namespace SkillUpgrades.Skills
{
    public class DownwardFireball : AbstractSkillUpgrade
    {
        public override string Description => "Toggle whether Vengeful Spirit can be used downward.";

        protected override void StartUpInitialize()
        {
            On.HeroController.Start += ModifyFireballFSM;
        }


        public static bool FireballDown = false;

        private static GameObject _fireballCast;
        private static GameObject _fireballCast2;
        private static GameObject _fireballObject;

        private static void SetFireballStats(PlayMakerFSM fireballFsm, bool down, bool spirit)
        {
            FsmState castR = fireballFsm.GetState("Cast Right");
            FsmState castL = fireballFsm.GetState("Cast Left");

            FsmState flukeR = fireballFsm.GetState("Fluke R");
            FsmState flukeL = fireballFsm.GetState("Fluke L");

            FsmState dungR = fireballFsm.GetState("Dung R");
            FsmState dungL = fireballFsm.GetState("Dung L");

            if (down)
            {
                castR.GetAction<SetFsmFloat>().setValue = 270f;
                castL.GetAction<SetFsmFloat>().setValue = 270f;
                castR.GetAction<SetVelocityAsAngle>().angle = 270f;
                castL.GetAction<SetVelocityAsAngle>().angle = 270f;

                flukeL.Actions.OfType<SetFloatValue>().ElementAt(0).floatValue = 250f;
                flukeL.Actions.OfType<SetFloatValue>().ElementAt(1).floatValue = 290f;
                flukeR.Actions.OfType<SetFloatValue>().ElementAt(0).floatValue = 250f;
                flukeR.Actions.OfType<SetFloatValue>().ElementAt(1).floatValue = 290f;

                dungL.GetAction<FlingObject>().angleMin = 265f;
                dungR.GetAction<FlingObject>().angleMin = 265f;
                dungL.GetAction<FlingObject>().angleMax = 275f;
                dungR.GetAction<FlingObject>().angleMax = 275f;
                dungL.GetAction<SetAngularVelocity2d>().angularVelocity = 10f;
                dungR.GetAction<SetAngularVelocity2d>().angularVelocity = -10f;
                dungL.GetAction<SetRotation>().zAngle = -296f;
                dungR.GetAction<SetRotation>().zAngle = 296f;
            }
            else
            {
                castR.GetAction<SetFsmFloat>().setValue = 0f;
                castL.GetAction<SetFsmFloat>().setValue = 180f;
                castR.GetAction<SetVelocityAsAngle>().angle = 0f;
                castL.GetAction<SetVelocityAsAngle>().angle = 180f;

                flukeL.Actions.OfType<SetFloatValue>().ElementAt(0).floatValue = 20f;
                flukeL.Actions.OfType<SetFloatValue>().ElementAt(1).floatValue = 60f;
                flukeR.Actions.OfType<SetFloatValue>().ElementAt(0).floatValue = 120f;
                flukeR.Actions.OfType<SetFloatValue>().ElementAt(1).floatValue = 160f;

                dungL.GetAction<FlingObject>().angleMin = 140;
                dungR.GetAction<FlingObject>().angleMin = 30;
                dungL.GetAction<FlingObject>().angleMax = 150;
                dungR.GetAction<FlingObject>().angleMax = 40;
                dungL.GetAction<SetAngularVelocity2d>().angularVelocity = 100f;
                dungR.GetAction<SetAngularVelocity2d>().angularVelocity = -100f;
                dungL.GetAction<SetRotation>().zAngle = -26f;
                dungR.GetAction<SetRotation>().zAngle = 26f;
            }
            if (!spirit) return;

            ExecuteLambda spawnR = new ExecuteLambda(() =>
            {
                GameObject value = UnityEngine.Object.Instantiate(_fireballObject);
                Vector3 v = fireballFsm.transform.position;
                v += new Vector3(0.5427618f, 0, -0.002f);
                value.transform.position = v;
                value.transform.localRotation = Quaternion.Euler(fireballFsm.transform.eulerAngles);
                PlayMakerFSM fsm = value.LocateMyFSM("Fireball Control");
                FixSpiritFSM(fsm, false);
                value.SetActive(true);
                fireballFsm.FsmVariables.GetFsmGameObject("Fireball").Value = value;
            });
            ExecuteLambda spawnL = new ExecuteLambda(() =>
            {
                GameObject value = UnityEngine.Object.Instantiate(_fireballObject);
                Vector3 v = fireballFsm.transform.position;
                v += new Vector3(-0.5427618f, 0, -0.002f);
                value.transform.position = v;
                value.transform.localRotation = Quaternion.Euler(fireballFsm.transform.eulerAngles);
                PlayMakerFSM fsm = value.LocateMyFSM("Fireball Control");
                FixSpiritFSM(fsm, true);
                value.SetActive(true);
                fireballFsm.FsmVariables.GetFsmGameObject("Fireball").Value = value;
            });

            // :hivescream:
            castR.Actions[7] = spawnR;
            castL.Actions[4] = spawnL;
        }

        private static void FixSpiritFSM(PlayMakerFSM fsm, bool left)
        {
            FsmState init = fsm.GetState("Init");
            init.RemoveAllOfType<FsmStateAction>();
            init.AddAction(new ExecuteLambda(() =>
            {
                fsm.FsmVariables.GetFsmFloat("Velocity").Value = fsm.GetComponent<Rigidbody2D>().velocity.y;
                if (left)
                {
                    fsm.SendEvent("LEFT");
                }
                else
                {
                    fsm.SendEvent("RIGHT");
                }
            }));

            FsmState idle = fsm.GetState("Idle");
            idle.GetAction<SetVelocity2d>().SwapXandY();
            idle.GetAction<GetVelocity2d>().SwapXandY();

            // We need to modify the state added by QoL
            if (fsm.GetState("Idle (No Collision)") is FsmState idleNoCollision)
            {
                idleNoCollision.GetAction<SetVelocity2d>().SwapXandY();
            }
        }

        private void ModifyFireballFSM(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            PlayMakerFSM fsm = self.spellControl;
            FsmState spirit = fsm.GetState("Fireball 1");
            FsmState soul = fsm.GetState("Fireball 2");

            #region Cache fireball objects
            if (_fireballCast == null)
            {
                _fireballCast = spirit.GetAction<SpawnObjectFromGlobalPool>().gameObject.Value;
            }
            if (_fireballCast2 == null)
            {
                _fireballCast2 = soul.GetAction<SpawnObjectFromGlobalPool>().gameObject.Value;
            }
            if (_fireballObject == null)
            {
                _fireballObject = _fireballCast.LocateMyFSM("Fireball Cast").GetState("Cast Right").GetAction<SpawnObjectFromGlobalPool>().gameObject.Value;
            }
            #endregion

            #region Set/Unset down fireball bool
            fsm.GetState("Level Check").InsertMethod(0, () =>
            {
                if (SkillUpgradeActive && !InputHandler.Instance.inputActions.left.IsPressed && !InputHandler.Instance.inputActions.right.IsPressed)
                {
                    FireballDown = true;
                }
                else
                {
                    FireballDown = false;
                }
            });
            #endregion

            #region Rotate fireball on emit
            ExecuteLambda doRotate = new ExecuteLambda(() =>
            {
                float scale = HeroController.instance.cState.facingRight ? -1 : 1;
                if (FireballDown) HeroController.instance.transform.Rotate(0, 0, 90 * scale);
            });

            ExecuteLambda unRotate = new ExecuteLambda(() =>
            {
                float scale = HeroController.instance.cState.facingRight ? -1 : 1;
                if (FireballDown) HeroController.instance.transform.Rotate(0, 0, -90 * scale);
            });

            ExecuteLambda spawnSpirit = new ExecuteLambda(() =>
            {
                if (FireballDown)
                {
                    GameObject value = UnityEngine.Object.Instantiate(_fireballCast);
                    value.transform.position = HeroController.instance.transform.position;
                    value.transform.localRotation = Quaternion.Euler(HeroController.instance.transform.eulerAngles);
                    SetFireballStats(value.LocateMyFSM("Fireball Cast"), true, spirit: true);
                    value.SetActive(true);
                    fsm.FsmVariables.GetFsmGameObject("Fireball Top").Value = value;
                }
                else
                {
                    GameObject prefab = _fireballCast;
                    GameObject value = prefab.Spawn(HeroController.instance.transform.position, Quaternion.Euler(HeroController.instance.transform.eulerAngles));
                    fsm.FsmVariables.GetFsmGameObject("Fireball Top").Value = value;
                }
            });
            ExecuteLambda spawnSoul = new ExecuteLambda(() =>
            {
                if (FireballDown)
                {
                    GameObject value = UnityEngine.Object.Instantiate(_fireballCast2);
                    value.transform.position = HeroController.instance.transform.position;
                    value.transform.localRotation = Quaternion.Euler(HeroController.instance.transform.eulerAngles);
                    SetFireballStats(value.LocateMyFSM("Fireball Cast"), true, spirit: false);
                    value.SetActive(true);
                    fsm.FsmVariables.GetFsmGameObject("Fireball Top").Value = value;
                }
                else
                {
                    GameObject prefab = _fireballCast2;
                    GameObject value = prefab.Spawn(HeroController.instance.transform.position, Quaternion.Euler(HeroController.instance.transform.eulerAngles));
                    fsm.FsmVariables.GetFsmGameObject("Fireball Top").Value = value;
                }
            });

            spirit.Actions = new FsmStateAction[]
            {
                spirit.Actions[0],
                spirit.Actions[1],
                spirit.Actions[2],
                doRotate,
                spawnSpirit, // Spawn from global pool
                unRotate,
                spirit.Actions[4],
                spirit.Actions[5],
            };

            soul.Actions = new FsmStateAction[]
            {
                soul.Actions[0],
                soul.Actions[1],
                soul.Actions[2],
                doRotate,
                spawnSoul, // Spawn from global pool
                unRotate,
                soul.Actions[4],
                soul.Actions[5],
            };
            #endregion

            #region Rotate recoil vector
            FsmState recoil = fsm.GetState("Fireball Recoil");

            FsmFloat yRecoil = fsm.AddFsmFloat("FB Recoil Current Y DF");

            ExecuteLambda setParams = new(() =>
            {
                float val = fsm.FsmVariables.GetFsmFloat("Fireball Recoil Distance").Value;
                val *= HeroController.instance.cState.facingRight ? -1 : 1;
                if (FireballDown)
                {
                    fsm.FsmVariables.GetFsmVector3("Fireball Recoil Vector").Value = new Vector3(0, Math.Abs(val), 0);
                    yRecoil.Value = 2 * Math.Abs(val);
                    fsm.FsmVariables.GetFsmFloat("Fireball Recoil Current").Value = 0f;
                }
                else
                {
                    fsm.FsmVariables.GetFsmVector3("Fireball Recoil Vector").Value = new Vector3(val, 0, 0);
                    fsm.FsmVariables.GetFsmFloat("Fireball Recoil Current").Value = 2 * val;
                    yRecoil.Value = 0f;
                }
            });
            SetVelocity2d setVel = recoil.Actions.OfType<SetVelocity2d>().ElementAt(1);
            setVel.y = yRecoil;

            recoil.Actions = new FsmStateAction[]
            {
                recoil.Actions[0],
                recoil.Actions[1],
                recoil.Actions[2],
                recoil.Actions[3],
                recoil.Actions[4],
                setParams,
                setVel,
                recoil.Actions[10]
            };
            #endregion

        }
    }
}
