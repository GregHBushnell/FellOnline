﻿using FishNet.Connection;
using FishNet.Transporting;
using FellOnline.Shared;
using FellOnline.Server.DatabaseServices;
using FellOnline.Database.Npgsql;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Server
{
	// Character Inventory Manager handles the players inventory
	public class CharacterInventorySystem : ServerBehaviour
	{
		private SceneServerAuthenticator loginAuthenticator;
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
			loginAuthenticator = FindObjectOfType<SceneServerAuthenticator>();
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
									   ItemContainer container,
									   int fromIndex, int toIndex,
									   Action<NpgsqlDbContext, long, Item> onDatabaseUpdateSlot)
		{
			if (container != null &&
				container.SwapItemSlots(fromIndex, toIndex, out Item fromItem, out Item toItem))
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
									   ItemContainer from, ItemContainer to,
									   int fromIndex, int toIndex,
									   Action<NpgsqlDbContext, long, Item> onDatabaseSetOldSlot = null,
									   Action<NpgsqlDbContext, long, long> onDatabaseDeleteOldSlot = null,
									   Action<NpgsqlDbContext, long, Item> onDatabaseSetNewSlot = null)
		{
			// same container... do the quick swap
			if (from == to)
			{
				return SwapContainerItems(dbContext, characterID, from, fromIndex, toIndex, onDatabaseSetOldSlot);
			}
			if (from != null &&
				to != null &&
				from.TryGetItem(fromIndex, out Item fromItem))
			{
				// check if we need to swap items
				if (to.TryGetItem(toIndex, out Item toItem))
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
				!character.IsTeleporting &&
				character.TryGet(out InventoryController inventoryController))
			{
				Item item = inventoryController.RemoveItem(msg.slot);

				// remove the item from the database
				CharacterInventoryService.Delete(dbContext, character.ID.Value, msg.slot);

				Server.Broadcast(conn, msg, true, Channel.Reliable);
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
				character.IsTeleporting ||
				!character.TryGet(out InventoryController inventoryController))
			{
				return;
			}

			switch (msg.fromInventory)
			{
				case InventoryType.Inventory:
					// swap the items in the inventory
					if (msg.to != msg.from &&
						SwapContainerItems(dbContext, character.ID.Value, inventoryController, msg.from, msg.to, (db, id, i) =>
					{
						CharacterInventoryService.Update(db, id, i);
					}))
					{
						dbTransaction.Commit();

						// tell the client we succeeded
						Server.Broadcast(conn, msg, true, Channel.Reliable);
					}
					break;
				case InventoryType.Equipment:
					break;
				case InventoryType.Bank:
					if (character.TryGet(out BankController bankController) &&
						SwapContainerItems(dbContext, character.ID.Value, bankController, inventoryController, msg.from, msg.to,
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
						CharacterInventoryService.SetSlot(db, id, b);
					}))
					{
						dbTransaction.Commit();

						// tell the client
						Server.Broadcast(conn, msg, true, Channel.Reliable);
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
				!character.TryGet(out EquipmentController equipmentController)||
				!character.TryGet(out EquipmentAppearanceController characterEquipmentAppearanceController) ||
				!character.TryGet(out WeaponCombatController weaponCombatController))
			{
				return;
			}

			switch (msg.fromInventory)
			{
				case InventoryType.Inventory:
					if (character.TryGet(out InventoryController inventoryController) &&
						inventoryController.TryGetItem(msg.inventoryIndex, out Item inventoryItem)
						)
					{
						if (!equipmentController.Equip(inventoryItem, msg.inventoryIndex, inventoryController, (ItemSlot)msg.slot))
						{
							return;
						}

						// did we replace an already equipped item?
						if (inventoryController.TryGetItem(msg.inventoryIndex, out Item prevItem))
						{
							CharacterInventoryService.SetSlot(dbContext, character.ID.Value, prevItem);
						}
						// remove the inventory item from the database
						else
						{
							CharacterInventoryService.Delete(dbContext, character.ID.Value, msg.inventoryIndex);
						}

						// set the equipment slot in the database
						CharacterEquipmentService.SetSlot(dbContext, character.ID.Value, inventoryItem);

						BaseEquipmentTemplate eItem = inventoryItem.Template as BaseEquipmentTemplate;
						EquipmentAppearanceDetails equipment = new EquipmentAppearanceDetails(eItem.ID, eItem.VisualID,eItem.WeaponEquipStyle,eItem.Slot);
						characterEquipmentAppearanceController.equipmentAppearanceDetails.Value = equipment;
						//Weapon w = new Weapon(eItem);
						//weaponCombatController.SetEquippedWeapon(w);

						dbTransaction.Commit();

						Server.Broadcast(conn, msg, true, Channel.Reliable);
					}
					break;
				case InventoryType.Equipment:
					return;
				case InventoryType.Bank:
					if (character.TryGet(out BankController bankController) &&
						bankController.TryGetItem(msg.inventoryIndex, out Item bankItem))
					{
						if (!equipmentController.Equip(bankItem, msg.inventoryIndex, bankController, (ItemSlot)msg.slot))
						{
							return;
						}

						// did we replace an already equipped item?
						if (bankController.TryGetItem(msg.inventoryIndex, out Item prevItem))
						{
							CharacterBankService.SetSlot(dbContext, character.ID.Value, prevItem);
						}
						// remove the inventory item from the database
						else
						{
							CharacterBankService.Delete(dbContext, character.ID.Value, msg.inventoryIndex);
						}

						// set the equipment slot in the database
						CharacterEquipmentService.SetSlot(dbContext, character.ID.Value, bankItem);

						BaseEquipmentTemplate eItem = bankItem.Template as BaseEquipmentTemplate;
						EquipmentAppearanceDetails equipment = new EquipmentAppearanceDetails(eItem.ID, eItem.VisualID,eItem.WeaponEquipStyle,eItem.Slot);
						characterEquipmentAppearanceController.equipmentAppearanceDetails.Value = equipment;
						//Weapon w = new Weapon(eItem);
						//weaponCombatController.SetEquippedWeapon(w);
						dbTransaction.Commit();

						Server.Broadcast(conn, msg, true, Channel.Reliable);
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
				!character.TryGet(out EquipmentController equipmentController)||
				!character.TryGet(out EquipmentAppearanceController characterEquipmentAppearanceController)||
				!character.TryGet(out WeaponCombatController weaponCombatController))
			{
				return;
			}

			switch (msg.toInventory)
			{
				case InventoryType.Inventory:
					if (character.TryGet(out InventoryController inventoryController) &&
						equipmentController.TryGetItem(msg.slot, out Item toInventory))
					{
						// save the old slot index so we can delete the item
						int oldSlot = toInventory.Slot;

						// if we found the item we should unequip it
						if (!equipmentController.Unequip(inventoryController, msg.slot, out List<Item> modifiedItems))
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
						foreach (Item item in modifiedItems)
						{
							// just in case..
							if (item == null)
							{
								continue;
							}

							// update or add the item to the database and initialize
							CharacterInventoryService.SetSlot(dbContext, character.ID.Value, item);
						}

						// delete the item from the equipment table
						CharacterEquipmentService.Delete(dbContext, character.ID.Value, oldSlot);

						EquipmentAppearanceDetails equipment = new EquipmentAppearanceDetails(0, 0, weaponEquipStyle: WeaponEquipStyle.OneHanded, equipmentSlot: ItemSlot.Primary);
						characterEquipmentAppearanceController.equipmentAppearanceDetails.Value = equipment;
						
						//weaponCombatController.SetEquippedWeapon(null);

						dbTransaction.Commit();

						Server.Broadcast(conn, msg, true, Channel.Reliable);
					}
					break;
				case InventoryType.Equipment:
					break;
				case InventoryType.Bank:
					if (character.TryGet(out BankController bankController) &&
						equipmentController.TryGetItem(msg.slot, out Item toBank))
					{
						int oldSlot = toBank.Slot;

						if (!equipmentController.Unequip(bankController, msg.slot, out List<Item> modifiedItems))
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
						foreach (Item item in modifiedItems)
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
						CharacterEquipmentService.Delete(dbContext, character.ID.Value, oldSlot);
						EquipmentAppearanceDetails equipment = new EquipmentAppearanceDetails(0, 0, weaponEquipStyle: WeaponEquipStyle.OneHanded, equipmentSlot: ItemSlot.Primary);
						characterEquipmentAppearanceController.equipmentAppearanceDetails.Value = equipment;

						//weaponCombatController.SetEquippedWeapon(null);

						dbTransaction.Commit();

						Server.Broadcast(conn, msg, true, Channel.Reliable);
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
				!character.IsTeleporting &&
				character.TryGet(out BankController bankController))
			{
				Item item = bankController.RemoveItem(msg.slot);

				// remove the item from the database
				CharacterBankService.Delete(dbContext, character.ID.Value, item.Slot);

				Server.Broadcast(conn, msg, true, Channel.Reliable);
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
				character.IsTeleporting ||
				!character.TryGet(out BankController bankController))
			{
				return;
			}

			switch (msg.fromInventory)
			{
				case InventoryType.Inventory:
					if (character.TryGet(out InventoryController inventoryController) &&
						SwapContainerItems(dbContext, character.ID.Value, inventoryController, bankController, msg.from, msg.to,
					(db, id, a) =>
					{
						CharacterInventoryService.SetSlot(db, id, a);
					},
					(db, id, s) =>
					{
						CharacterInventoryService.Delete(db, id, s);
					},
					(db, id, b) =>
					{
						CharacterBankService.SetSlot(db, id, b);
					}))
					{
						dbTransaction.Commit();

						// tell the client
						Server.Broadcast(conn, msg, true, Channel.Reliable);
					}
					break;
				case InventoryType.Equipment:
					break;
				case InventoryType.Bank:
					// swap the items in the bank
					if (msg.to != msg.from &&
						SwapContainerItems(dbContext, character.ID.Value, bankController, msg.from, msg.to, (db, id, i) =>
					{
						CharacterBankService.Update(db, id, i);
					}))
					{
						dbTransaction.Commit();

						// tell the client we succeeded
						Server.Broadcast(conn, msg, true, Channel.Reliable);
					}
					break;
				default: break;
			}
		}
	}
}