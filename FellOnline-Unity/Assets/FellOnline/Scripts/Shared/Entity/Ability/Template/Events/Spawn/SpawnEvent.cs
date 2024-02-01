﻿using System.Collections.Generic;

namespace FellOnline.Shared
{
	public abstract class SpawnEvent : AbilityEvent
	{
		public SpawnEventType SpawnEventType = SpawnEventType.OnSpawn;

		public abstract void Invoke(Character self, TargetInfo targetInfo, AbilityObject initialObject, ref int nextID, Dictionary<int, AbilityObject> abilityObjects);
	}
}