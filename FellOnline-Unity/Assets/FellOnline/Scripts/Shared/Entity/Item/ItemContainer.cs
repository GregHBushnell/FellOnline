﻿using System;
using System.Collections.Generic;

namespace FellOnline.Shared
{
	public abstract class ItemContainer : CharacterBehaviour
	{
		public readonly List<Item> Items = new List<Item>();

		public event Action<ItemContainer, Item, int> OnSlotUpdated;

		public override void OnDestroying()
		{
			OnSlotUpdated = null;
		}

		/// <summary>
		/// base.CanManipulate will check if the items list is null.
		/// </summary>
		public virtual bool CanManipulate()
		{
			return Items.Count > 0;
		}

		/// <summary>
		/// Checks if the item slot exists.
		/// </summary>
		public bool IsValidSlot(int slot)
		{
			return slot > -1 &&
				  slot < Items.Count;
		}

		/// <summary>
		/// Checks if the slot is empty.
		/// </summary>
		public bool IsSlotEmpty(int slot)
		{
			return IsValidSlot(slot) &&
				   Items[slot] == null;
		}

		/// <summary>
		/// Validates the item slot exists and returns whatever is in the slot. Returns false if the item doesn't exist.
		/// </summary>
		public bool TryGetItem(int slot, out Item item)
		{
			if (IsValidSlot(slot))
			{
				item = Items[slot];
				return item != null;
			}
			item = null;
			return false;
		}

		/// <summary>
		/// Adds items or sets empty slots.
		/// </summary>
		public void AddSlots(List<Item> items, int amount)
		{
			if (items != null)
			{
				for (int i = 0; i < items.Count; ++i)
				{
					this.Items.Add(items[i]);
				}
				return;
			}
			for (int i = 0; i < amount; ++i)
			{
				this.Items.Add(null);
			}
		}

		public bool HasFreeSlot()
		{
			for (int i = 0; i < Items.Count; ++i)
			{
				if (IsSlotEmpty(i))
				{
					return true;
				}
			}
			return false;
		}

		public int FreeSlots()
		{
			int count = 0;
			for (int i = 0; i < Items.Count; ++i)
			{
				if (IsSlotEmpty(i))
				{
					++count;
				}
			}
			return count;
		}

		public int FilledSlots()
		{
			int count = 0;
			for (int i = 0; i < Items.Count; ++i)
			{
				if (!IsSlotEmpty(i))
				{
					++count;
				}
			}
			return count;
		}

		public bool CanAddItem(Item item)
		{
			if (!CanManipulate())
			{
				return false;
			}

			// we can't add an item with a stack size of 0.. a 0 stack size means the item doesn't exist!
			if (item == null) return false;

			uint amountRemaining = item.IsStackable ? item.Stackable.Amount : 1;
			for (int i = 0; i < Items.Count; ++i)
			{
				// if we find an empty slot we return instantly
				if (IsSlotEmpty(i))
				{
					return true;
				}

				// if we find another item of the same type and it's stack is not full
				if (Items[i].IsStackable &&
					!Items[i].Stackable.IsStackFull &&
					Items[i].IsMatch(item))
				{
					uint remainingCapacity = Items[i].Template.MaxStackSize - Items[i].Stackable.Amount;

					amountRemaining = remainingCapacity.AbsoluteSubtract(amountRemaining);
				}

				if (amountRemaining < 1) return true;
			}
			return false;
		}

		/// <summary>
		/// Attempts to add an item to the container. Returns true if the entire stack size of the item has been successfully added.
		/// All modified items are returned.
		/// </summary>
		public bool TryAddItem(Item item, out List<Item> modifiedItems)
		{
			modifiedItems = new List<Item>();

			// ensure we can add the entire item to the container
			if (!CanAddItem(item))
			{
				return false;
			}

			uint amount = item.IsStackable ? item.Stackable.Amount : 1;
			for (int i = 0; i < Items.Count; ++i)
			{
				// add the item to the current slot
				if (Items[i] != null &&
					Items[i].IsStackable &&
					Items[i].Stackable.AddToStack(item))
				{
					// set the remaining amount to the items stack size
					amount = item.Stackable.Amount;

					// add the modified items to the list
					modifiedItems.Add(Items[i]);
					modifiedItems.Add(item);

					OnSlotUpdated?.Invoke(this, item, i);
				}

				// we added the item to the container
				if (amount < 1) return true;
			}
			for (int i = 0; i < Items.Count; ++i)
			{
				// find the first slot to put the remaining item in
				if (IsSlotEmpty(i))
				{
					// set the item slot to the item, presume it succeeded..
					SetItemSlot(item, i);

					// add the modified item to the list
					modifiedItems.Add(item);

					// successfully added the entire item
					return true;
				}
			}
			// we should never reach this...
			// should probably throw an exception instead of just returning false.
			// if we get here then we have a race condition for some reason
			return false;
		}

		/// <summary>
		/// Sets the item slot directly. Previous item will be lost if we don't have a reference elsewhere.
		/// </summary>
		public bool SetItemSlot(Item item, int slot)
		{
			if (!IsValidSlot(slot))
			{
				// setting the slot failed
				return false;
			}

			Items[slot] = item;
			if (item != null)
			{
				item.Slot = slot;
			}
			OnSlotUpdated?.Invoke(this, item, slot);
			return true;
		}

		public bool SwapItemSlots(int from, int to)
		{
			return SwapItemSlots(from, to, out Item fromItem, out Item toItem);
		}

		public bool SwapItemSlots(int from, int to, out Item fromItem, out Item toItem)
		{
			if (!CanManipulate() ||
				from < 0 ||
				to < 0 ||
				from > Items.Count ||
				to > Items.Count)
			{
				fromItem = null;
				toItem = null;

				// swapping the items failed
				return false;
			}

			fromItem = Items[from];
			toItem = Items[to];

			Items[from] = toItem;
			if (toItem != null)
			{
				toItem.Slot = from;
			}

			Items[to] = fromItem;
			if (fromItem != null)
			{
				fromItem.Slot = to;
			}

			OnSlotUpdated?.Invoke(this, toItem, from);
			OnSlotUpdated?.Invoke(this, fromItem, to);
			return true;
		}

		/// <summary>
		/// Removes an item from the specified slot and returns it. Returns null if the slot was empty.
		/// </summary>
		public Item RemoveItem(int slot)
		{
			if (!CanManipulate() ||
				!IsValidSlot(slot))
			{
				return null;
			}

			Item item = Items[slot];
			item.Slot = -1;
			SetItemSlot(null, slot);
			return item;
		}
	}
}