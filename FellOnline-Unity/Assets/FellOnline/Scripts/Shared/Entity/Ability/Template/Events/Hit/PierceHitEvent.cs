using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Pierce Hit Event", menuName = "FellOnline/Character/Ability/Hit Event/Pierce", order = 1)]
	public sealed class PierceHitEvent : HitEvent
	{
		public override int Invoke(Character attacker, Character defender,Mob  mob, TargetInfo hitTarget, GameObject abilityObject)
		{
			return 1;
		}
	}
}