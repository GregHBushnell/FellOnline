using System.Collections.Generic;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Ability Multiply Event", menuName = "FellOnline/Character/Ability/Spawn Event/Multiply", order = 1)]
	public sealed class FMultiplyEvent : FSpawnEvent
	{
		public int SpawnCount;

		public override void Invoke(Character self, FTargetInfo targetInfo, FAbilityObject initialObject, ref int nextID, Dictionary<int, FAbilityObject> abilityObjects)
		{
			if (abilityObjects != null)
			{
				for (int i = 0; i < SpawnCount; ++i)
				{
					// create/fetch from pool
					GameObject go = new GameObject(initialObject.name);
					SceneManager.MoveGameObjectToScene(go, self.gameObject.scene);
					go.SetActive(false);

					// construct additional ability objects
					FAbilityObject abilityObject = go.GetComponent<FAbilityObject>();
					if (abilityObject == null)
					{
						abilityObject = go.AddComponent<FAbilityObject>();
					}
					go.transform.SetPositionAndRotation(initialObject.transform.position, initialObject.transform.rotation);
					abilityObject.ContainerID = initialObject.ContainerID;
					abilityObject.Ability = initialObject.Ability;
					abilityObject.Caster = initialObject.Caster;
					abilityObject.HitCount = initialObject.HitCount;
					abilityObject.RemainingActiveTime = initialObject.RemainingActiveTime;
					abilityObjects.Add(++nextID, abilityObject);
				}
			}
		}

		public override string GetFormattedDescription()
		{
			return Description.Replace("$SPAWNCOUNT$", SpawnCount.ToString());
		}
	}
}