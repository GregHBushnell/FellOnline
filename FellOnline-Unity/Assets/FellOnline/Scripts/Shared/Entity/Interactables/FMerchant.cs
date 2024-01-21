#if UNITY_SERVER 
using FishNet.Transporting;
#endif

using UnityEngine;

namespace FellOnline.Shared
{
	public enum MerchantTabType : byte
	{
		None = 0,
		Ability,
		AbilityEvent,
		Item,
	}

	[RequireComponent(typeof(FSceneObjectNamer))]
	public class FMerchant : FInteractable
	{
		public FMerchantTemplate Template;

		public override bool OnInteract(Character character)
		{
			if (Template == null ||
				!base.OnInteract(character))
			{
				return false;
			}

#if UNITY_SERVER
			character.Owner.Broadcast(new FMerchantBroadcast()
			{
				id = Template.ID,
			}, true, Channel.Reliable);
#endif
			return true;
		}
	}
}