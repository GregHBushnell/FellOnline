using FishNet.Connection;
using FishNet.Transporting;
using FellOnline.Shared;
using FellOnline.Server.DatabaseServices;
using System.Collections.Generic;
using FellOnline.Database.Npgsql;
using System;
using UnityEngine;


namespace FellOnline.Server
{
	// Character Inventory Manager handles the players inventory
	public class FCharacterInventorySystem : FServerBehaviour
	{
		private FSceneServerAuthenticator loginAuthenticator;
		private LocalConnectionState serverState;

		/*public float saveRate = 60.0f;
		private float nextSave = 0.0f;

		public Dictionary<string, InventoryController> inventories = new Dictionary<string, InventoryController>();*/

		public override void InitializeOnce()
		{
			//nextSave = saveRate;

			if (ServerManager != null)
			{
				ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
				ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
			}
			else
			{
				enabled = false;
			}
		}

		/*void LateUpdate()
		{
			if (serverState == LocalConnectionState.Started)
			{
				if (nextSave < 0)
				{
					nextSave = saveRate;
					
					Debug.Log("CharacterInventoryManager: Save");

					// all characters inventories are periodically saved
					// TODO: create an InventoryService with a save inventories function
					//Database.Instance.SaveInventories(new List<Character>(characters.Values));
				}
				nextSave -= Time.deltaTime;
			}
		}*/

		private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs args)
		{
			loginAuthenticator = FindObjectOfType<FSceneServerAuthenticator>();
			if (loginAuthenticator == null)
				return;

			serverState = args.ConnectionState;

			if (args.ConnectionState == LocalConnectionState.Started)
			{
				loginAuthenticator.OnClientAuthenticationResult += Authenticator_OnClientAuthenticationResult;

				ServerManager.RegisterBroadcast<InventoryRemoveItemBroadcast>(OnServerInventoryRemoveItemBroadcastReceived, true);
				ServerManager.RegisterBroadcast<InventorySwapItemSlotsBroadcast>(OnServerInventorySwapItemSlotsBroadcastReceived, true);

				ServerManager.RegisterBroadcast<EquipmentEquipItemBroadcast>(OnServerEquipmentEquipItemBroadcastReceived, true);
				ServerManager.RegisterBroadcast<EquipmentUnequipItemBroadcast>(OnServerEquipmentUnequipItemBroadcastReceived, true);

				ServerManager.RegisterBroadcast<BankRemoveItemBroadcast>(OnServerBankRemoveItemBroadcastReceived, true);
				ServerManager.RegisterBroadcast<BankSwapItemSlotsBroadcast>(OnServerBankSwapItemSlotsBroadcastReceived, true);
			}
			else if (args.ConnectionState == LocalConnectionState.Stopped)
			{
				loginAuthenticator.OnClientAuthenticationResult -= Authenticator_OnClientAuthenticationResult;

				ServerManager.UnregisterBroadcast<InventoryRemoveItemBroadcast>(OnServerInventoryRemoveItemBroadcastReceived);
				ServerManager.UnregisterBroadcast<InventorySwapItemSlotsBroadcast>(OnServerInventorySwapItemSlotsBroadcastReceived);

				ServerManager.UnregisterBroadcast<EquipmentEquipItemBroadcast>(OnServerEquipmentEquipItemBroadcastReceived);
				ServerManager.UnregisterBroadcast<EquipmentUnequipItemBroadcast>(OnServerEquipmentUnequipItemBroadcastReceived);

				ServerManager.UnregisterBroadcast<BankRemoveItemBroadcast>(OnServerBankRemoveItemBroadcastReceived);
				ServerManager.UnregisterBroadcast<BankSwapItemSlotsBroadcast>(OnServerBankSwapItemSlotsBroadcastReceived);
			}
		}

		private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
		{
		}

		private void Authenticator_OnClientAuthenticationResult(NetworkConnection conn, bool authenticated)
		{
		}

		public bool SwapContainerItems(NpgsqlDbContext dbContext,
									   long characterID,
									   FItemContainer container,
									   int fromIndex, int toIndex,
									   Action<NpgsqlDbContext, long, FItem> onDatabaseUpdateSlot)
		{
			if (container != null &&
				container.SwapItemSlots(fromIndex, toIndex, out FItem fromItem, out FItem toItem))
			{
				// we can update the item slots in the database easily
				onDatabaseUpdateSlot?.Invoke(dbContext, characterID, fromItem);
				onDatabaseUpdateSlot?.Invoke(dbContext, characterID, toItem);
				return true;
			}
			return false;
		}

		public bool SwapContainerItems(NpgsqlDbContext dbContext,
									   long characterID,
									   FItemContainer from, FItemContainer to,
									   int fromIndex, int toIndex,
									   Action<NpgsqlDbContext, long, FItem> onDatabaseSetOldSlot = null,
									   Action<NpgsqlDbContext, long, long> onDatabaseDeleteOldSlot = null,
									   Action<NpgsqlDbContext, long, FItem> onDatabaseSetNewSlot = null)
		{
			// same container... do the quick swap
			if (from == to)
			{
				return SwapContainerItems(dbContext, characterID, from, fromIndex, toIndex, onDatabaseSetOldSlot);
			}
			if (from != null &&
				to != null &&
				from.TryGetItem(fromIndex, out FItem fromItem))
			{
				// check if we need to swap items
				if (to.TryGetItem(toIndex, out FItem toItem))
				{
					// put the target item in the old container
					from.SetItemSlot(toItem, fromIndex);
					onDatabaseSetOldSlot?.Invoke(dbContext, characterID, toItem);
				}
				// the slot we want to move the item to is empty
				else
				{
					// remove the item from the old container
					from.SetItemSlot(null, fromIndex);
					onDatabaseDeleteOldSlot?.Invoke(dbContext, characterID, fromItem.Slot);
				}
				// put the item in the new container
				to.SetItemSlot(fromItem, toIndex);
				onDatabaseSetNewSlot?.Invoke(dbContext, characterID, fromItem);
				return true;
			}
			return false;
		}



		private void OnServerInventoryRemoveItemBroadcastReceived(NetworkConnection conn, InventoryRemoveItemBroadcast msg, Channel channel)
		{
			if (conn == null ||
				conn.FirstObject == null)
			{
				return;
			}

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			if (dbContext == null)
			{
				return;
			}

			Character character = conn.FirstObject.GetComponent<Character>();
			if (character != null &&
				!character.IsTeleporting)
			{
				FItem item = character.InventoryController.RemoveItem(msg.slot);

				// remove the item from the database
				FCharacterInventoryService.Delete(dbContext, character.ID.Value, msg.slot);

				conn.Broadcast(msg, true, Channel.Reliable);
			}
		}

		private void OnServerInventorySwapItemSlotsBroadcastReceived(NetworkConnection conn, InventorySwapItemSlotsBroadcast msg, Channel channel)
		{
			if (conn == null ||
				conn.FirstObject == null)
			{
				return;
			}

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			if (dbContext == null)
			{
				return;
			}

			using var dbTransaction = dbContext.Database.BeginTransaction();
			if (dbTransaction == null)
			{
				return;
			}


			Character character = conn.FirstObject.GetComponent<Character>();
			if (character == null ||
				character.IsTeleporting)
			{
				return;
			}

			switch (msg.fromInventory)
			{
				case InventoryType.Inventory:
					// swap the items in the inventory
					if (msg.to != msg.from &&
						SwapContainerItems(dbContext, character.ID.Value, character.InventoryController, msg.from, msg.to, (db, id, i) =>
					{
						FCharacterInventoryService.Update(db, id, i);
					}))
										{
						dbTransaction.Commit();

						// tell the client we succeeded
						conn.Broadcast(msg, true, Channel.Reliable);
					}
					break;
				case InventoryType.Equipment:
					break;
				case InventoryType.Bank:
					if (SwapContainerItems(dbContext, character.ID.Value, character.BankController, character.InventoryController, msg.from, msg.to,
					(db, id, a) =>
					{
						CharacterBankService.SetSlot(db, id, a);
					},
					(db, id, s) =>
					{
						CharacterBankService.Delete(db, id, s);
					},
					(db, id, b) =>
					{
						FCharacterInventoryService.SetSlot(db, id, b);
					}))
					{
						dbTransaction.Commit();
						// tell the client
						conn.Broadcast(msg, true, Channel.Reliable);
					}
					break;
				default: break;
			}
		}

		private void OnServerEquipmentEquipItemBroadcastReceived(NetworkConnection conn, EquipmentEquipItemBroadcast msg, Channel channel)
		{
			if (conn == null ||
				conn.FirstObject == null)
			{
				return;
			}

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			if (dbContext == null)
			{
				return;
			}

			using var dbTransaction = dbContext.Database.BeginTransaction();
			if (dbTransaction == null)
			{
				return;
			}

			Character character = conn.FirstObject.GetComponent<Character>();
			if (character == null ||
				character.IsTeleporting ||
				character.EquipmentController == null)
			{
				return;
			}

			switch (msg.fromInventory)
			{
				case InventoryType.Inventory:
					if (character.InventoryController != null &&
						character.InventoryController.TryGetItem(msg.inventoryIndex, out FItem inventoryItem))
					{
						if (!character.EquipmentController.Equip(inventoryItem, msg.inventoryIndex, character.InventoryController, (ItemSlot)msg.slot))
						{
							return;
						}

						// did we replace an already equipped item?
						if (character.InventoryController.TryGetItem(msg.inventoryIndex, out FItem prevItem))
						{
							FCharacterInventoryService.SetSlot(dbContext, character.ID.Value, prevItem);
						}
						else
						{
							// remove the item from the database
							FCharacterInventoryService.Delete(dbContext, character.ID.Value, msg.inventoryIndex);
						}
						// set the equipment slot in the database
						FCharacterEquipmentService.SetSlot(dbContext, character.ID.Value, inventoryItem);
						dbTransaction.Commit();
						conn.Broadcast(msg, true, Channel.Reliable);
					}
					break;
				case InventoryType.Equipment:
					return;
				case InventoryType.Bank:
					if (character.BankController != null &&
						character.BankController.TryGetItem(msg.inventoryIndex, out FItem bankItem))
					{
						if (!character.EquipmentController.Equip(bankItem, msg.inventoryIndex, character.BankController, (ItemSlot)msg.slot))
						{
							return;
						}

						// did we replace an already equipped item?
						if (character.BankController.TryGetItem(msg.inventoryIndex, out FItem prevItem))
						{
							CharacterBankService.SetSlot(dbContext, character.ID.Value, prevItem);
						}
						// remove the inventory item from the database
						else
						{
							// remove the item from the database
							CharacterBankService.Delete(dbContext, character.ID.Value, msg.inventoryIndex);
						}
						// set the equipment slot in the database
						FCharacterEquipmentService.SetSlot(dbContext, character.ID.Value, bankItem);
						dbTransaction.Commit();

						conn.Broadcast(msg, true, Channel.Reliable);
					}
					break;
				default: return;
			}
		}

		private void OnServerEquipmentUnequipItemBroadcastReceived(NetworkConnection conn, EquipmentUnequipItemBroadcast msg, Channel channel)
		{
			if (conn == null ||
				conn.FirstObject == null)
			{
				return;
			}

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			if (dbContext == null)
			{
				return;
			}

			using var dbTransaction = dbContext.Database.BeginTransaction();
			if (dbTransaction == null)
			{
				return;
			}



			Character character = conn.FirstObject.GetComponent<Character>();
			if (character == null ||
				character.IsTeleporting ||
				character.EquipmentController == null)
			{
				return;
			}

			switch (msg.toInventory)
			{
				case InventoryType.Inventory:
					if (character.EquipmentController.TryGetItem(msg.slot, out FItem toInventory))
					{
						// save the old slot index so we can delete the item
						int oldSlot = toInventory.Slot;

						// if we found the item we should unequip it
						if (!character.EquipmentController.Unequip(character.InventoryController, msg.slot, out List<FItem> modifiedItems))
						{
							return;
						}
						// see if we have successfully added the item
						if (modifiedItems == null ||
							modifiedItems.Count < 1)
						{
							return;
						}

						// update all of the modified slots
						foreach (FItem item in modifiedItems)
						{
							// just in case..
							if (item == null)
							{
								continue;
							}

							// update or add the item to the database and initialize
							FCharacterInventoryService.SetSlot(dbContext, character.ID.Value, item);
						}



						// delete the item from the equipment table
						FCharacterEquipmentService.Delete(dbContext, character.ID.Value, oldSlot);

					
						dbTransaction.Commit();

						conn.Broadcast(msg, true, Channel.Reliable);
					}
					break;
				case InventoryType.Equipment:
					break;
				case InventoryType.Bank:
					if (character.EquipmentController.TryGetItem(msg.slot, out FItem toBank))
					{
						int oldSlot = toBank.Slot;

						if (!character.EquipmentController.Unequip(character.BankController, msg.slot, out List<FItem> modifiedItems))
						{
							return;
						}
						if (modifiedItems == null ||
							modifiedItems.Count < 1)
						{
							return;
						}

							// update all of the modified slots
							foreach (FItem item in modifiedItems)
							{
							// just in case..
							if (item == null)
							{
								continue;
							}
							// update or add the item to the database and initialize
							CharacterBankService.SetSlot(dbContext, character.ID.Value, item);
						}

						// delete the item from the equipment table
						FCharacterEquipmentService.Delete(dbContext, character.ID.Value, oldSlot);

						

						conn.Broadcast(msg, true, Channel.Reliable);
					}
					break;
				default: return;
			}
		}

		private void OnServerBankRemoveItemBroadcastReceived(NetworkConnection conn, BankRemoveItemBroadcast msg, Channel channel)
		{
			if (conn == null ||
				conn.FirstObject == null)
			{
				return;
			}

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			if (dbContext == null)
			{
				return;
			}

			Character character = conn.FirstObject.GetComponent<Character>();
			if (character != null &&
				!character.IsTeleporting)
			{
				FItem item = character.BankController.RemoveItem(msg.slot);

				// remove the item from the database
				CharacterBankService.Delete(dbContext, character.ID.Value, item.Slot);

				conn.Broadcast(msg, true, Channel.Reliable);
			}
		}

		private void OnServerBankSwapItemSlotsBroadcastReceived(NetworkConnection conn, BankSwapItemSlotsBroadcast msg, Channel channel)
		{
			if (conn == null ||
				conn.FirstObject == null)
			{
				return;
			}

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			if (dbContext == null)
			{
				return;
			}
			using var dbTransaction = dbContext.Database.BeginTransaction();
			if (dbTransaction == null)
			{
				return;
			}

			Character character = conn.FirstObject.GetComponent<Character>();
			if (character == null ||
				character.IsTeleporting)
			{
				return;
			}

			switch (msg.fromInventory)
			{
				case InventoryType.Inventory:
					if (SwapContainerItems(dbContext, character.ID.Value, character.InventoryController, character.BankController, msg.from, msg.to,
					(db, id, a) =>					
					{
						FCharacterInventoryService.SetSlot(db, id, a);
					},
					(db, id, s) =>
					{
						FCharacterInventoryService.Delete(db, id, s);
					},
					(db, id, b) =>
					{
						CharacterBankService.SetSlot(db, id, b);
					}))
					{
						dbTransaction.Commit();
					
						// tell the client
						conn.Broadcast(msg, true, Channel.Reliable);
					}
					break;
				case InventoryType.Equipment:
					break;
				case InventoryType.Bank:
					// swap the items in the bank
					if (msg.to != msg.from &&
						SwapContainerItems(dbContext, character.ID.Value, character.BankController, msg.from, msg.to, (db, id, i) =>
					{
						
						CharacterBankService.Update(db, id, i);
					}))
					{
						dbTransaction.Commit();

						// tell the client we succeeded
						conn.Broadcast(msg, true, Channel.Reliable);
					}
					break;
				default: break;
			}
		}
	}
}