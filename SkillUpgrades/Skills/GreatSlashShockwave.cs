using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using SkillUpgrades.Components;
using SkillUpgrades.FsmStateActions;
using SkillUpgrades.Util;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace SkillUpgrades.Skills
{
    public class GreatSlashShockwave : AbstractSkillUpgrade
    {
        public const string ShockwaveGameObjectName = "SkillUpgrades GSlash Shockwave";

        public override string Description => "Toggle whether Great Slash should release a shockwave.";

        protected override void StartUpInitialize()
        {
            On.HeroController.Start += ModifyNailArtFsm;
            IL.HealthManager.TakeDamage += PreventSoulGain;
        }

        private void PreventSoulGain(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            cursor.GotoNext(i => i.MatchCallOrCallvirt<HeroController>(nameof(HeroController.SoulGain)));
            cursor.GotoPrev(MoveType.After, i => i.MatchLdfld<HitInstance>(nameof(HitInstance.AttackType)));
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate<Func<AttackTypes, HitInstance, AttackTypes>>((origType, hitInstance) =>
            {
                if (hitInstance.Source.name.StartsWith(ShockwaveGameObjectName))
                {
                    return AttackTypes.Spell;
                }
                return origType;
            });
        }

        private static GameObject _greatSlashPrefab;

        private void ModifyNailArtFsm(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            _greatSlashPrefab = UObject.Instantiate(self.transform.Find("Attacks/Great Slash").gameObject);
            _greatSlashPrefab.SetActive(false);
            UObject.DontDestroyOnLoad(_greatSlashPrefab);

            PlayMakerFSM nailArtFSM = self.gameObject.LocateMyFSM("Nail Arts");

            FsmState Spawn = nailArtFSM.GetState("G Slash");
            Spawn.InsertAction(3, new ExecuteLambda(() => { if (this.SkillUpgradeActive) self.StartCoroutine(SpawnShockwave()); }));
        }

        private IEnumerator SpawnShockwave()
        {
            Log("Spawning");

            GameObject clone = UObject.Instantiate(_greatSlashPrefab);
            clone.name = ShockwaveGameObjectName;
            clone.transform.SetPosition2D(HeroController.instance.transform.Find("Attacks/Great Slash").position + Vector3.right);
            clone.transform.SetParent(HeroController.instance.transform.Find("Attacks"));

            PlayMakerFSM controlCollider = clone.LocateMyFSM("Control Collider");
            controlCollider.GetState("Enable").GetActionOfType<IntCompare>().integer2.Value = 45;

            float xVel = -30 * HeroController.instance.transform.localScale.x;
            clone.AddComponent<Mover>().Velocity = new Vector2(xVel, 0);

            clone.SetActive(true);
            
            yield return null;
            clone.transform.SetParent(null);

            tk2dSpriteAnimator anim = clone.GetComponent<tk2dSpriteAnimator>();
            yield return new WaitUntil(() => anim == null || anim.CurrentFrame >= 10);
            if (anim == null) yield break;
            anim.Pause();

            Collider2D col = clone.GetComponent<Collider2D>();
            yield return new WaitUntil(() => col == null || !col.enabled);
            if (col == null) yield break;
            UObject.Destroy(clone);
        }
    }
}
