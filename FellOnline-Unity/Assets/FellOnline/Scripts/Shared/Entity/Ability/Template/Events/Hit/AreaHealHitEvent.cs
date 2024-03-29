﻿using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Area Heal Hit Event", menuName = "FellOnline/Character/Ability/Hit Event/Area Heal", order = 1)]
	public sealed class AreaHealHitEvent : HitEvent
	{
		private Collider[] colliders = new Collider[100];

		public int HitCount;
		public int Heal;
		public float Radius;
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
						damageController.Heal(attacker, Heal);
						++hits;
					}
					Mob mobDef = colliders[i].gameObject.GetComponent<Mob>();
					if(mobDef != null && mobDef.TryGet(out MobDamageController mobDamageController))
					{
						mobDamageController.Heal(attacker, Heal);
						++hits;
					}
				}
			}
			return hits;
		}

		public override string GetFormattedDescription()
		{
			return Description.Replace("$HEAL$", "<color=#" + TinyColor.skyBlue.ToHex() + ">" + Heal + "</color>")
							  .Replace("$RADIUS$", Radius.ToString());
		}
	}
}