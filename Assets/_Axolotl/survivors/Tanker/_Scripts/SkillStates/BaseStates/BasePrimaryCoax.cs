using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine;

namespace Axolotl.Tanker.BaseSkillStates
{
   public class BasePrimaryCoax : BaseSkillState
   {

      public static float damageCoefficient = Modules.StaticValues.gunDamageCoefficient;
      public static float procCoefficient = 0.3f;
      public static float baseDuration = .1f;
      public static float force = 100f;
      public static float recoil = 0f;
      public static float range = 256f;
      public static GameObject tracerEffectPrefab;

      private float duration;
      private float fireTime;
      private bool hasFired;
      private string muzzleString;

      public override void OnEnter()
      {
         base.OnEnter();
         this.duration = BasePrimaryCoax.baseDuration / this.attackSpeedStat;
         this.fireTime = (.2f) * this.duration;
         base.characterBody.SetAimTimer(2f);
         this.muzzleString = "Muzzle";

         //base.PlayAnimation(layerName, animationState, playbackRateParam, duration);
      }

      public override void OnExit()
      {
         base.OnExit();
      }

      private void Fire()
      {
         if (!this.hasFired)
         {
            this.hasFired = true;

            base.characterBody.AddSpreadBloom(1.5f);
            //Add MuzzleFlash Here
            //EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
            //Play sound here
            //Util.PlaySound("HenryShootPistol", base.gameObject);

            if (base.isAuthority)
            {
               Ray aimRay = base.GetAimRay();
               base.AddRecoil(-1f * BasePrimaryCoax.recoil, -2f * BasePrimaryCoax.recoil, -0.5f * BasePrimaryCoax.recoil, 0.5f * BasePrimaryCoax.recoil);

               new BulletAttack
               {
                  bulletCount = 1,
                  aimVector = aimRay.direction,
                  damage = BasePrimaryCoax.range,
                  damageColorIndex = DamageColorIndex.Default,
                  damageType = DamageType.Generic,
                  falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                  maxDistance = BasePrimaryCoax.range,
                  force = BasePrimaryCoax.force,
                  hitMask = LayerIndex.CommonMasks.bullet,
                  minSpread = 0f,
                  maxSpread = 1.5f,
                  isCrit = base.RollCrit(),
                  owner = base.gameObject,
                  muzzleName = muzzleString,
                  smartCollision = false,
                  procChainMask = default(ProcChainMask),
                  procCoefficient = procCoefficient,
                  radius = .75f,
                  sniper = false,
                  stopperMask = LayerIndex.CommonMasks.bullet,
                  weapon = null,
                  tracerEffectPrefab = BasePrimaryCoax.tracerEffectPrefab,
                  spreadPitchScale = 0f,
                  spreadYawScale = 0f,
                  queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                  hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
               }.Fire();
            }
         }
      }

      public override void FixedUpdate()
      {
         base.FixedUpdate();

         if (base.fixedAge >= this.fireTime)
         {
            this.Fire();
         }
         if (base.fixedAge >= this.duration && base.isAuthority)
         {
            this.outer.SetNextStateToMain();
            return;
         }
      }

      public override InterruptPriority GetMinimumInterruptPriority()
      {
         return InterruptPriority.PrioritySkill;
      }
   }
}