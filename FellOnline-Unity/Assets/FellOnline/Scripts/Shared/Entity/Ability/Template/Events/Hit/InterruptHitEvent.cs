using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Interrupt Hit Event", menuName = "FellOnline/Character/Ability/Hit Event/Interrupt", order = 1)]
	public sealed class InterruptHitEvent : HitEvent
	{
		public override int Invoke(Character attacker, Character defender,Mob mob, TargetInfo hitTarget, GameObject abilityObject)
		{
			if (defender != null && defender.TryGet(out AbilityController abilityController))
			{
				abilityController.Interrupt(attacker);
			}

			// if(mob != null && mob.TryGet(out AbilityController mobAbilityController))
			// {
			// 	mobAbilityController.Interrupt(attacker);
			// }
			// interrupt doesn't count as a hit
			return 0;
		}
	}
}