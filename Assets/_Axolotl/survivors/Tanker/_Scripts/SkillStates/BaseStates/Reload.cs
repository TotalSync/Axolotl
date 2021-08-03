using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EntityStates;

namespace Axolotl.Tanker.BaseSkillStates
{
	public class Reload : BaseState
	{

		/*
		This reload skill applies to the main cannon only.
		This reload skill refills the magazine of the secondary cannon from the reserves.
		 */

		//public static float enterSoundPitch;
		//public static float exitSoundPitch;
		//public static string enterSoundString;
		//public static string exitSoundString;

		public static GameObject reloadEffectPrefab;
		public static string reloadEffectMuzzleString;
		public static float baseDuration;
		private bool hasGivenStock;

		private float duration
		{
			get
			{
				return Reload.baseDuration / this.attackSpeedStat;
			}
		}

		public override void OnEnter()
		{
			base.OnEnter();
			//base.PlayAnimation("Gesture, Addative", (base.characterBody.isSprinting && base.characterMotor && base.characterMotor.isGrounded) ? "ReloadSimple" : "Reload", "Reload.playackRate", this.duration);
			//Util.PlayAttackSpeedSound(Reload.enterSoundString, base.gameObject, Reload.enterSoundPitch);
			//EffectManager.SimpleMuzzleFlash(Reload.reloadEffectPrefab, base.gameObject, Reload.reloadEffectMuzzleString, false);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration)
			{
				this.RefreshStock();
			}
			if (!base.isAuthority || base.fixedAge < this.duration) { return; }
			//Util.PlayAttackSpeedSound(Reload.exitSoundString, base.gameObject, Reload.exitSoundPitch);
			this.outer.SetNextStateToMain();
		}

		private void RefreshStock()
		{
			if (this.hasGivenStock) { return; }
			base.skillLocator.secondary.stock = base.skillLocator.secondary.maxStock;
			this.hasGivenStock = true;
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

	} 
}
