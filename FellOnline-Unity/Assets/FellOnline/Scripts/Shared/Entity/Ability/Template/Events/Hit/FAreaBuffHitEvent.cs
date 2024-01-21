using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Area Buff Hit Event", menuName = "FellOnline/Character/Ability/Hit Event/Area Buff", order = 1)]
	public sealed class FAreaBuffHitEvent : FHitEvent
	{
		private Collider[] colliders = new Collider[100];

		public int HitCount;
		public int Stacks;
		public float Radius;
		public FBuffTemplate BuffTemplate;
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
				 		def.BuffController.Apply(BuffTemplate);
				 		++hits;
				 	}
				 }
			}
			return hits;
		}

		public override string Tooltip()
		{
			return base.Tooltip().Replace("$BUFF$", BuffTemplate.Name)
								 .Replace("$STACKS$", Stacks.ToString())
								 .Replace("$RADIUS$", Radius.ToString());
		}
	}
}