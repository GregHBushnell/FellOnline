﻿using FishNet.Broadcast;
using System.Collections.Generic;

namespace FellOnline.Shared
{
	public struct InventorySetItemBroadcast : IBroadcast
	{
		public long instanceID;
		public int templateID;
		public int slot;
		public int seed;
		public uint stackSize;
	}

	public struct InventorySetMultipleItemsBroadcast : IBroadcast
	{
		public List<InventorySetItemBroadcast> items;
	}

	public struct InventoryRemoveItemBroadcast : IBroadcast
	{
		public int slot;
	}

	public struct InventorySwapItemSlotsBroadcast : IBroadcast
	{
		public int from;
		public int to;
		public InventoryType fromInventory;
	}
}