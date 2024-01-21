using FishNet.Transporting;
using UnityEngine;

namespace FellOnline.Shared
{
	[RequireComponent(typeof(Character))]
	public class BankController : FItemContainer
	{
		public Character Character;

		public long Currency = 0;

		private void Awake()
		{
			AddSlots(null, 100);
		}

#if !UNITY_SERVER
		public override void OnStartClient()
		{
			base.OnStartClient();

			if (!base.IsOwner)
			{
				enabled = false;
				return;
			}

			ClientManager.RegisterBroadcast<BankSetItemBroadcast>(OnClientBankSetItemBroadcastReceived);
			ClientManager.RegisterBroadcast<BankSetMultipleItemsBroadcast>(OnClientBankSetMultipleItemsBroadcastReceived);
			ClientManager.RegisterBroadcast<BankRemoveItemBroadcast>(OnClientBankRemoveItemBroadcastReceived);
			ClientManager.RegisterBroadcast<BankSwapItemSlotsBroadcast>(OnClientBankSwapItemSlotsBroadcastReceived);
		}

		public override void OnStopClient()
		{
			base.OnStopClient();

			if (base.IsOwner)
			{
				ClientManager.UnregisterBroadcast<BankSetItemBroadcast>(OnClientBankSetItemBroadcastReceived);
				ClientManager.UnregisterBroadcast<BankSetMultipleItemsBroadcast>(OnClientBankSetMultipleItemsBroadcastReceived);
				ClientManager.UnregisterBroadcast<BankRemoveItemBroadcast>(OnClientBankRemoveItemBroadcastReceived);
				ClientManager.UnregisterBroadcast<BankSwapItemSlotsBroadcast>(OnClientBankSwapItemSlotsBroadcastReceived);
			}
		}

		/// <summary>
		/// Server sent a set item broadcast. Item slot is set to the received item details.
		/// </summary>
		private void OnClientBankSetItemBroadcastReceived(BankSetItemBroadcast msg, Channel channel)
		{
			FItem newItem = new FItem(msg.instanceID, msg.seed, msg.templateID, msg.stackSize);
			SetItemSlot(newItem, msg.slot);
		}

		/// <summary>
		/// Server sent a multiple set item broadcast. Item slot is set to the received item details.
		/// </summary>
		private void OnClientBankSetMultipleItemsBroadcastReceived(BankSetMultipleItemsBroadcast msg, Channel channel)
		{
			foreach (BankSetItemBroadcast subMsg in msg.items)
			{
				FItem newItem = new FItem(subMsg.instanceID, subMsg.seed, subMsg.templateID, subMsg.stackSize);
				SetItemSlot(newItem, subMsg.slot);
			}
		}

		/// <summary>
		/// Server sent a remove item from slot broadcast. Item is removed from the received slot with server authority.
		/// </summary>
		private void OnClientBankRemoveItemBroadcastReceived(BankRemoveItemBroadcast msg, Channel channel)
		{
			RemoveItem(msg.slot);
		}

		/// <summary>
		/// Server sent a swap slot broadcast. Both slots are swapped with server authority.
		/// </summary>
		/// <param name="msg"></param>
		private void OnClientBankSwapItemSlotsBroadcastReceived(BankSwapItemSlotsBroadcast msg, Channel channel)
		{
			switch (msg.fromInventory)
			{
				case InventoryType.Inventory:
					if (Character.InventoryController != null &&
						Character.InventoryController.TryGetItem(msg.from, out FItem inventoryItem))
					{
						if (TryGetItem(msg.to, out FItem bankItem))
						{
							Character.InventoryController.SetItemSlot(bankItem, msg.from);
						}
						else
						{
							Character.InventoryController.SetItemSlot(null, msg.from);
						}

						SetItemSlot(inventoryItem, msg.to);
					}
					break;
				case InventoryType.Equipment:
					break;
				case InventoryType.Bank:
					SwapItemSlots(msg.from, msg.to);
					break;
				default: return;
			}
		}
#endif

		public override bool CanManipulate()
		{
			if (!base.CanManipulate())
			{
				return false;
			}

			/*if ((character.State == CharacterState.Idle ||
				  character.State == CharacterState.Moving) &&
				  character.State != CharacterState.UsingObject &&
				  character.State != CharacterState.IsFrozen &&
				  character.State != CharacterState.IsStunned &&
				  character.State != CharacterState.IsMesmerized) return true;
			*/
			return true;
		}

		public void SendSwapItemSlotsRequest(int from, int to, InventoryType fromInventory)
		{
			if (fromInventory == InventoryType.Bank &&
				from == to)
			{
				return;
			}
            ClientManager.Broadcast(new BankSwapItemSlotsBroadcast()
			{
				from = from,
				to = to,
				fromInventory = fromInventory,
			}, Channel.Reliable);
		}
	}
}