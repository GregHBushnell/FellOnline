using UnityEngine;

namespace FellOnline.Shared
{
	public abstract class HitEvent : AbilityEvent
	{
		public HitTargetType TargetType;

		/// <summary>
		/// Returns the number of hits the event has issued,
		/// </summary>
		public abstract int Invoke(Character attacker, Character defender,Mob mob, TargetInfo hitTarget, GameObject abilityObject);
	}
}