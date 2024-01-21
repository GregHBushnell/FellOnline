using FishNet.Broadcast;
using System.Collections.Generic;

namespace FellOnline.Shared
{
	/// <summary>
	/// Sends the ID of the interactable object to the server as a request to use.
	/// </summary>
	public struct FInteractableBroadcast : IBroadcast
	{
		public int interactableID;
	}

	public struct FAbilityCrafterBroadcast : IBroadcast
	{
		public int interactableID;
	}

	public struct FAbilityCraftBroadcast : IBroadcast
	{
		public int templateID;
		public List<int> events;
	}

	public struct BankerBroadcast : IBroadcast
	{
		public int interactableID;
	}

	public struct FMerchantBroadcast : IBroadcast
	{
		public int id;
	}

	public struct FMerchantPurchaseBroadcast : IBroadcast
	{
		public int id;
		public int index;
		public MerchantTabType type;
	}

	public struct WorldItemBroadcast : IBroadcast
	{
		public int interactableID;
		public int templateID;
	}
	
	public struct WorldItemPickupBroadcast : IBroadcast
	{
		public int interactableID;
		public int templateID;
	}
	
}