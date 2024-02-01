using Cysharp.Text;
using UnityEngine;
using System.Collections.Generic;

namespace FellOnline.Shared
{
	public abstract class BaseWeaponTemplate : BaseEquipmentTemplate
	{
       
	   public AbilityTemplate AbilityCategoryTemplate;
		public AbilityEvent MainAttackEvent;
		public List<AbilityEvent> ExtraAbilityEvents = new List<AbilityEvent>();
        public DamageAttributeTemplate  DamageAttribute;
        public int Damage;
		
		
		
        public uint AnimationID;
        
		public float ActivationTime;
		public float ActiveTime;
		public float Cooldown;
		public float Range;
		public float Speed;
		public AbilityResourceDictionary Resources = new AbilityResourceDictionary();
		public AbilityResourceDictionary Requirements = new AbilityResourceDictionary();

    }
}