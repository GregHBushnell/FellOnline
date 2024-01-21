using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Pierce Hit Event", menuName = "FellOnline/Character/Ability/Hit Event/Pierce", order = 1)]
	public sealed class FPierceHitEvent : FHitEvent
	{
		public override int Invoke(Character attacker, Character defender, FTargetInfo hitTarget, GameObject abilityObject)
		{
			return 1;
		}
	}
}