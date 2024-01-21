using System.Collections.Generic;

namespace FellOnline.Shared
{
	public abstract class FSpawnEvent : FAbilityEvent
	{
		public SpawnEventType SpawnEventType = SpawnEventType.OnSpawn;

		public abstract void Invoke(Character self, FTargetInfo targetInfo, FAbilityObject initialObject, ref int nextID, Dictionary<int, FAbilityObject> abilityObjects);
	}
}