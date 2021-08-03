using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine;

namespace Axolotl.Tanker.BaseSkillStates
{
   public class BaseSecondaryCannon : BaseSkillState
   {

      public static float damageCoefficient = Modules.StaticValues.gunDamageCoefficient;
      public static float procCoefficient = 1f;
      public static float baseDuration = 1f; 
      public static float force = 800f;
      public static float recoil = 5f;
      public static float range = 256f;
      public static GameObject tracerEffectPrefab;

      private float duration;
      private float fireTime;
      private bool hasFired;
      private string muzzleString; 

		public override void OnEnter()
		{
			base.OnEnter();
         this.duration = BaseSecondaryCannon.baseDuration / this.attackSpeedStat;
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

            base.characterBody.AddSpreadBloom(0.1f);
				//Add MuzzleFlash Here
				//EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
				//Play sound here
				//Util.PlaySound("HenryShootPistol", base.gameObject);

				if (base.isAuthority)
				{
               Ray aimRay = base.GetAimRay();
               base.AddRecoil(-1f * BaseSecondaryCannon.recoil, -2f * BaseSecondaryCannon.recoil, -0.5f * BaseSecondaryCannon.recoil, 0.5f * BaseSecondaryCannon.recoil);

               new BulletAttack
               {
                  bulletCount = 1,
                  aimVector = aimRay.direction,
                  damage = BaseSecondaryCannon.range,
                  damageColorIndex = DamageColorIndex.Default,
                  damageType = DamageType.Generic,
                  falloffModel = BulletAttack.FalloffModel.None,
                  maxDistance = BaseSecondaryCannon.range,
                  force = BaseSecondaryCannon.force,
                  hitMask = LayerIndex.CommonMasks.bullet,
                  minSpread = 0f,
                  maxSpread = 0f,
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
                  tracerEffectPrefab = BaseSecondaryCannon.tracerEffectPrefab,
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
            this.outer.SetNextState(new EnterReload());
            return;
         }
      }

      public override InterruptPriority GetMinimumInterruptPriority()
      {
         return InterruptPriority.PrioritySkill;
      }
   }
}