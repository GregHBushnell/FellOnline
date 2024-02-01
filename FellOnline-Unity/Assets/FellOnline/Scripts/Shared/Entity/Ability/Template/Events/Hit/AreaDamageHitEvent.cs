﻿using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Area Damage Hit Event", menuName = "FellOnline/Character/Ability/Hit Event/Area Damage", order = 1)]
	public sealed class AreaDamageHitEvent : HitEvent
	{
		private Collider[] colliders = new Collider[10];

		public int HitCount;
		public int Damage;
		public float Radius;
		public DamageAttributeTemplate DamageAttributeTemplate;
		public LayerMask CollidableLayers = -1;

		public override int Invoke(Character attacker, Character defender,Mob mob, TargetInfo hitTarget, GameObject abilityObject)
		{
			PhysicsScene physicsScene = attacker.gameObject.scene.GetPhysicsScene();

			int overlapCount = physicsScene.OverlapSphere(//Physics.OverlapCapsuleNonAlloc(
				hitTarget.Target.transform.position,
				Radius,
				colliders,
				CollidableLayers,
				QueryTriggerInteraction.Ignore);

			int hits = 0;
			for (int i = 0; i < overlapCount && hits < HitCount; ++i)
			{
				if (colliders[i] != attacker.Motor.Capsule)
				{
					Character def = colliders[i].gameObject.GetComponent<Character>();
					if (def != null &&
						def.TryGet(out CharacterDamageController damageController))
					{
						damageController.Damage(attacker, Damage, DamageAttributeTemplate);
						++hits;
					}
					Mob mobDef = colliders[i].gameObject.GetComponent<Mob>();
					if(mobDef != null && mobDef.TryGet(out MobDamageController mobDamageController))
					{
						mobDamageController.Damage(attacker, Damage, DamageAttributeTemplate);
						++hits;
					}
				}
			}
			return hits;
		}

		public override string GetFormattedDescription()
		{
			return Description.Replace("$ELEMENT$", "<color=#" + DamageAttributeTemplate.DisplayColor.ToHex() + ">" + DamageAttributeTemplate.Name + "</color>")
							  .Replace("$DAMAGE$", "<color=#" + DamageAttributeTemplate.DisplayColor.ToHex() + ">" + Damage + "</color>")
							  .Replace("$RADIUS$", Radius.ToString());
		}
	}
}