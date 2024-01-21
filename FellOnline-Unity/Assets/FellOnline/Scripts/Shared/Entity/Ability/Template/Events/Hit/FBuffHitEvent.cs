using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Buff Hit Event", menuName = "FellOnline/Character/Ability/Hit Event/Buff", order = 1)]
	public sealed class FBuffHitEvent : FHitEvent
	{
		public int Stacks;
		public FBuffTemplate BuffTemplate;

		public override int Invoke(Character attacker, Character defender, FTargetInfo hitTarget, GameObject abilityObject)
		{
			if (defender != null && defender.BuffController != null)
			{
				defender.BuffController.Apply(BuffTemplate);
			}

			// a buff or debuff does not count as a hit so we return 0
			return 0;
		}

		public override string GetFormattedDescription()
		{
		return Description.Replace("$BUFF$", BuffTemplate.Name)
							  .Replace("$STACKS$", Stacks.ToString());
		}
	}
}