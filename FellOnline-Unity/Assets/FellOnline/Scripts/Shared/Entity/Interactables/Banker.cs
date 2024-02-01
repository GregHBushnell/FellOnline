#if UNITY_SERVER || UNITY_EDITOR
using FishNet.Transporting;
using static FellOnline.Server.Server;
#endif
using UnityEngine;

namespace FellOnline.Shared
{
	[RequireComponent(typeof(SceneObjectNamer))]
	public class Banker : Interactable
	{
		public override bool OnInteract(Character character)
		{
			if (!base.OnInteract(character))
			{
				return false;
			}
#if UNITY_SERVER
			Broadcast(character.Owner, new BankerBroadcast()
			{
				interactableID = ID,
			}, true, Channel.Reliable);
#endif
			return true;
		}
	}
}