using System;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using SkillUpgrades.Components;
using SkillUpgrades.FsmStateActions;
using UnityEngine;
using Vasi;
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
            Spawn.InsertAction(3, new ExecuteLambda(() => { if (this.SkillUpgradeActive) SpawnShockwave(); }));
        }

        private void SpawnShockwave()
        {
            GameObject clone = UObject.Instantiate(_greatSlashPrefab);
            clone.name = ShockwaveGameObjectName;
            // Setting parent so the fsm handles setting position and scale
            clone.transform.SetParent(HeroController.instance.transform.Find("Attacks"));

            // Keep the shockwave alive for 2.5x the lifetime of the regular GSlash
            PlayMakerFSM controlCollider = clone.LocateMyFSM("Control Collider");
            controlCollider.GetState("Enable").GetAction<IntCompare>().integer2.Value = 45;
            // Deparent once the spawning is done
            controlCollider.GetState("Init").AddAction(new ExecuteLambda(() => clone.transform.SetParent(null)));
            // Destroy once we're done with the object
            FsmState disable = controlCollider.GetState("Disable");
            disable.RemoveAllOfType<FsmStateAction>();
            disable.AddAction(new ExecuteLambda(() => UObject.Destroy(clone)));

            // Move it horizontally away from the knight at speed equal to cdash speed
            float xVel = -30 * HeroController.instance.transform.localScale.x;
            clone.AddComponent<Mover>().Velocity = new Vector2(xVel, 0);

            // Pause the animation after 10 frames so it looks like a wave
            tk2dSpriteAnimationClip clip = clone.GetComponent<tk2dSpriteAnimator>().GetClipByName("NA Big Slash Effect");
            clip.frames = clip.frames.Take(10).ToArray();

            // Add rb2d so collision works
            Rigidbody2D rb = clone.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;

            clone.SetActive(true);
            InvokeUsedSkillUpgrade();
        }
    }
}
