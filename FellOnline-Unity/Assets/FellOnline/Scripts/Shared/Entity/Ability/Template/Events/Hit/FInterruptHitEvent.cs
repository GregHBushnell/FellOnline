using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Interrupt Hit Event", menuName = "FellOnline/Character/Ability/Hit Event/Interrupt", order = 1)]
	public sealed class FInterruptHitEvent : FHitEvent
	{
		public override int Invoke(Character attacker, Character defender, FTargetInfo hitTarget, GameObject abilityObject)
		{
			if (defender != null && defender.AbilityController != null)
			{
				defender.AbilityController.Interrupt(attacker);
			}
			// interrupt doesn't count as a hit
			return 0;
		}
	}
}