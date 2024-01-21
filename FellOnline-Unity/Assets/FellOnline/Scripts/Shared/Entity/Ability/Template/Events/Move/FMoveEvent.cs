using UnityEngine;

namespace FellOnline.Shared
{
	public abstract class FMoveEvent : FAbilityEvent
	{
		public abstract void Invoke(FAbility ability, Transform abilityObject, float deltaTime);
	}
}