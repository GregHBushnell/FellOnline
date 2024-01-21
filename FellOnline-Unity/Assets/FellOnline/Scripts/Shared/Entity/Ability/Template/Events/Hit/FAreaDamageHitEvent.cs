﻿using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Area Damage Hit Event", menuName = "FellOnline/Character/Ability/Hit Event/Area Damage", order = 1)]
	public sealed class FAreaDamageHitEvent : FHitEvent
	{
		private Collider[] colliders = new Collider[100];

		public int HitCount;
		public int Damage;
		public float Radius;
		public FDamageAttributeTemplate DamageAttributeTemplate;
		public LayerMask CollidableLayers = -1;

		public override int Invoke(Character attacker, Character defender, FTargetInfo hitTarget, GameObject abilityObject)
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
				 	if (def != null && def.DamageController != null)
				 	{
				 		def.DamageController.Damage(attacker, Damage, DamageAttributeTemplate);
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