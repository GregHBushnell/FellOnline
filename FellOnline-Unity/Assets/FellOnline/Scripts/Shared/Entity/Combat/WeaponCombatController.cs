#if !UNITY_SERVER
using FellOnline.Client;
#endif
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	public class WeaponCombatController : CharacterBehaviour
	{
        public const long NO_Weapon = 0;

		private Animator animator;

		public WeaponTemplate weaponTemplate;
        public override void OnAwake()
        {
           animator = GetComponentInChildren<Animator>();
        }




	



		public void SetEquippedWeapon()
		{
			
#if !UNITY_SERVER
			
		
#endif
		}

public void SetAnimation(WeaponTemplate _weaponTemplate)
		{
#if !UNITY_SERVER
			if (_weaponTemplate != null)
			{
				weaponTemplate = _weaponTemplate;
				animator.SetFloat("WeaponChoice", _weaponTemplate.AnimationID);
				
				animator.SetLayerWeight(animator.GetLayerIndex("Weapons"),1);

			}else{
				weaponTemplate = null;
				animator.SetLayerWeight(animator.GetLayerIndex("Weapons"),0);
				animator.SetLayerWeight(1,0);
			}
#endif
		}


		public void SendAnimationEvent()
		{
#if !UNITY_SERVER
			if (weaponTemplate != null)
			{
				float randomAnimation = UnityEngine.Random.Range(0.0f, 1.0f);
				animator.SetFloat("RandomAttack", randomAnimation);
				animator.SetTrigger("Attack");
			}
#endif
		}


		

		
		
	}
}