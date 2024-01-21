using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Forward Move Event", menuName = "Character/Ability/Move Event/Forward Move", order = 1)]
	public sealed class FForwardMoveEvent : FMoveEvent
	{
		public override void Invoke(FAbility ability, Transform abilityObject, float deltaTime)
		{
			abilityObject.position += abilityObject.forward * ability.Speed * deltaTime;
		}
	}
}