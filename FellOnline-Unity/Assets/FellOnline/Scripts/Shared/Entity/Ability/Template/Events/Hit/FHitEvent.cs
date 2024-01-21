using UnityEngine;

namespace FellOnline.Shared
{
	public abstract class FHitEvent : FAbilityEvent
	{
		public HitTargetType TargetType;

		/// <summary>
		/// Returns the number of hits the event has issued,
		/// </summary>
		public abstract int Invoke(Character attacker, Character defender, FTargetInfo hitTarget, GameObject abilityObject);
	}
}