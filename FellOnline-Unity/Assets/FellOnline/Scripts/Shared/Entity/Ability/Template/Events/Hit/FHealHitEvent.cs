using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Heal Hit Event", menuName = "FellOnline/Character/Ability/Hit Event/Heal", order = 1)]
	public sealed class FHealHitEvent : FHitEvent
	{
		public int Heal;

		public override int Invoke(Character attacker, Character defender, FTargetInfo hitTarget, GameObject abilityObject)
		{
			if (defender != null && defender.DamageController != null)
			{
				defender.DamageController.Heal(attacker, Heal);
			}
			return 1;
		}

		public override string GetFormattedDescription()
		{
			return Description.Replace("$HEAL$", Heal.ToString());
		}
	}
}