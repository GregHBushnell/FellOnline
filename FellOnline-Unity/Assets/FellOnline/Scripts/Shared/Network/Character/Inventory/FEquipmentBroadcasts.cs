﻿using FishNet.Broadcast;
using System.Collections.Generic;

namespace FellOnline.Shared
{
	public struct EquipmentSetItemBroadcast : IBroadcast
	{
		public long instanceID;
		public int templateID;
		public int slot;
		public int seed;
		public uint stackSize;
	}

	public struct EquipmentSetMultipleItemsBroadcast : IBroadcast
	{
		public List<EquipmentSetItemBroadcast> items;
	}

	public struct EquipmentEquipItemBroadcast : IBroadcast
	{
		public int inventoryIndex;
		public byte slot;
		public InventoryType fromInventory;
	}

	public struct EquipmentUnequipItemBroadcast : IBroadcast
	{
		public byte slot;
		public InventoryType toInventory;
	}
}