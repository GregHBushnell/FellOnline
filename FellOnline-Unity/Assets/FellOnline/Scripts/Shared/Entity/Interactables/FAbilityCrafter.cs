#if UNITY_SERVER || UNITY_EDITOR
using FishNet.Transporting;
#endif

using UnityEngine;

namespace FellOnline.Shared
{
	[RequireComponent(typeof(FSceneObjectNamer))]
	public class FAbilityCrafter : FInteractable
	{
		public override bool OnInteract(Character character)
		{
			if (!base.OnInteract(character))
			{
				return false;
			}

#if UNITY_SERVER
			character.Owner.Broadcast(new FAbilityCrafterBroadcast()
			{
				interactableID = ID,
			}, true, Channel.Reliable);
#endif
			return true;
		}
	}
}