using FishNet.Connection;
using FishNet.Transporting;
using FellOnline.Shared;
using FellOnline.Server.DatabaseServices;
using System.Collections.Generic;
using FellOnline.Database.Npgsql;
using System.Diagnostics;
using Unity.VisualScripting;

namespace FellOnline.Server
{
	/// <summary>
	/// This service helps the server validate clients interacting with Interactable objects in scenes.
	/// </summary>
	public class FInteractableSystem : FServerBehaviour
	{
		public FWorldSceneDetailsCache WorldSceneDetailsCache;
		public int MaxAbilityCount = 25;

		public override void InitializeOnce()
		{
			if (ServerManager != null)
			{
				ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
			}
			else
			{
				enabled = false;
			}
		}

		private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs args)
		{
			if (args.ConnectionState == LocalConnectionState.Started)
			{
				ServerManager.RegisterBroadcast<FInteractableBroadcast>(OnServerInteractableBroadcastReceived, true);
				ServerManager.RegisterBroadcast<FMerchantPurchaseBroadcast>(OnServerMerchantPurchaseBroadcastReceived, true);
				ServerManager.RegisterBroadcast<FAbilityCraftBroadcast>(OnServerAbilityCraftBroadcastReceived, true);
				ServerManager.RegisterBroadcast<WorldItemPickupBroadcast>(OnServerWorldItemPickupBroadcastReceived, true);
			}
			else if (args.ConnectionState == LocalConnectionState.Stopped)
			{
				ServerManager.UnregisterBroadcast<FInteractableBroadcast>(OnServerInteractableBroadcastReceived);
				ServerManager.UnregisterBroadcast<FMerchantPurchaseBroadcast>(OnServerMerchantPurchaseBroadcastReceived);
				ServerManager.UnregisterBroadcast<FAbilityCraftBroadcast>(OnServerAbilityCraftBroadcastReceived);
				ServerManager.UnregisterBroadcast<WorldItemPickupBroadcast>(OnServerWorldItemPickupBroadcastReceived);
			}
		}

		/// <summary>
		/// Interactable broadcast received from a character.
		/// </summary>
		private void OnServerInteractableBroadcastReceived(NetworkConnection conn, FInteractableBroadcast msg, Channel channel)
		{
			if (conn == null)
			{
				return;
			}

			// validate connection character
			if (conn.FirstObject == null)
			{
				return;
			}
			Character character = conn.FirstObject.GetComponent<Character>();
			if (character == null)
			{
				return;
			}

			// valid scene object
			if (!WorldSceneDetailsCache.Scenes.TryGetValue(character.SceneName.Value, out FWorldSceneDetails details))
			{
				UnityEngine.Debug.Log("Missing Scene:" + character.SceneName.Value);
				return;
			}
			if (!FSceneObjectUID.IDs.TryGetValue(msg.interactableID, out FSceneObjectUID sceneObject))
			{
				if (sceneObject == null)
				{
					UnityEngine.Debug.Log("Missing SceneObject");
				}
				else
				{
					UnityEngine.Debug.Log("Missing ID:" + msg.interactableID);
				}
				return;
			}

			FIInteractable interactable = sceneObject.GetComponent<FIInteractable>();
			interactable?.OnInteract(character);
		}

		private void OnServerWorldItemPickupBroadcastReceived(NetworkConnection conn, WorldItemPickupBroadcast msg, Channel channel){
			if (conn == null)
			{
				return;
			}
			// validate connection character
			if (conn.FirstObject == null)
			{
				return;
			}
			Character character = conn.FirstObject.GetComponent<Character>();
			if (character == null)
			{
				return;
			}
			
			// validate request
			
			
			if (!FSceneObjectUID.IDs.TryGetValue(msg.interactableID, out FSceneObjectUID sceneObject))
			{
				if (sceneObject == null)
				{
					UnityEngine.Debug.Log("Missing SceneObject");
				}
				else
				{
					UnityEngine.Debug.Log("Missing ID:" + msg.interactableID);
				}
				return;
			}
			WorldItemTemplate worldItemTemplate = WorldItemTemplate.Get<WorldItemTemplate>(msg.templateID);
			WorldItem worldItem = sceneObject.GetComponent<WorldItem>();
			
			if (worldItemTemplate == null)
			{
				return;
			}
			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			if (dbContext == null)
			{
				return;
			}

			if (worldItemTemplate.Item != null )
					{
						UnityEngine.Debug.Log("Reaching broadcast stage");
						FItem newItem = new FItem(worldItemTemplate.Item, worldItemTemplate.amount);
						if (newItem == null)
						{
							return;
						}
						
						List<InventorySetItemBroadcast> modifiedItemBroadcasts = new List<InventorySetItemBroadcast>();

						// see if we have successfully added the item
						if (character.InventoryController.TryAddItem(newItem, out List<FItem> modifiedItems) &&
							modifiedItems != null &&
							modifiedItems.Count > 0)
						{

							// add slot update requests to our message
							foreach (FItem item in modifiedItems)
							{
								// just in case..
								if (item == null)
								{
									continue;
								}

								// update or add the item to the database and initialize
								FCharacterInventoryService.SetSlot(dbContext, character.ID.Value, item);

								// create the new item broadcast
								modifiedItemBroadcasts.Add(new InventorySetItemBroadcast()
								{
									instanceID = item.ID,
									templateID = item.Template.ID,
									slot = item.Slot,
									stackSize = item.IsStackable ? item.Stackable.Amount : 0,
								});
							}
						}

						// tell the client they have new items
						if (modifiedItemBroadcasts.Count > 0)
						{
							conn.Broadcast(new InventorySetMultipleItemsBroadcast()
							{
								items = modifiedItemBroadcasts,
							}, true, Channel.Reliable);
						}
					}

		}
		private void OnServerMerchantPurchaseBroadcastReceived(NetworkConnection conn, FMerchantPurchaseBroadcast msg, Channel channel)
		{
			if (conn == null)
			{
				return;
			}

			// validate connection character
			if (conn.FirstObject == null)
			{
				return;
			}
			Character character = conn.FirstObject.GetComponent<Character>();
			if (character == null &&
				character.InventoryController != null)
			{
				return;
			}

			// validate request
			FMerchantTemplate merchantTemplate = FMerchantTemplate.Get<FMerchantTemplate>(msg.id);
			if (merchantTemplate == null)
			{
				return;
			}

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			if (dbContext == null)
			{
				return;
			}

			switch (msg.type)
			{
				case MerchantTabType.Item:
					FBaseItemTemplate itemTemplate = merchantTemplate.Items[msg.index];
					if (itemTemplate == null)
					{
						return;
					}

					// do we have enough currency to purchase this?
					if (character.InventoryController.Currency < itemTemplate.Price)
					{
						return;
					}

					if (merchantTemplate.Items != null &&
						merchantTemplate.Items.Count >= msg.index)
					{
						FItem newItem = new FItem(itemTemplate, 1);
						if (newItem == null)
						{
							return;
						}
						
						List<InventorySetItemBroadcast> modifiedItemBroadcasts = new List<InventorySetItemBroadcast>();

						// see if we have successfully added the item
						if (character.InventoryController.TryAddItem(newItem, out List<FItem> modifiedItems) &&
							modifiedItems != null &&
							modifiedItems.Count > 0)
						{
							// remove the price from the characters currency
							character.InventoryController.Currency -= itemTemplate.Price;

							// add slot update requests to our message
							foreach (FItem item in modifiedItems)
							{
								// just in case..
								if (item == null)
								{
									continue;
								}

								// update or add the item to the database and initialize
								FCharacterInventoryService.SetSlot(dbContext, character.ID.Value, item);

								// create the new item broadcast
								modifiedItemBroadcasts.Add(new InventorySetItemBroadcast()
								{
									instanceID = item.ID,
									templateID = item.Template.ID,
									slot = item.Slot,
									stackSize = item.IsStackable ? item.Stackable.Amount : 0,
								});
							}
						}

						// tell the client they have new items
						if (modifiedItemBroadcasts.Count > 0)
						{
							conn.Broadcast(new InventorySetMultipleItemsBroadcast()
							{
								items = modifiedItemBroadcasts,
							}, true, Channel.Reliable);
						}
					}
					break;
				case MerchantTabType.Ability:
					if (merchantTemplate.Abilities != null &&
						merchantTemplate.Abilities.Count >= msg.index)
					{
						LearnAbilityTemplate(dbContext, conn, character, merchantTemplate.Abilities[msg.index]);
					}
					break;
				case MerchantTabType.AbilityEvent:
					if (merchantTemplate.AbilityEvents != null &&
						merchantTemplate.AbilityEvents.Count >= msg.index)
					{
						LearnAbilityTemplate(dbContext, conn, character, merchantTemplate.AbilityEvents[msg.index]);
					}
					break;
				default: return;
			}
		}

		public void LearnAbilityTemplate<T>(NpgsqlDbContext dbContext, NetworkConnection conn, Character character, T template) where T : FBaseAbilityTemplate
		{
			// do we already know this ability?
			if (template == null ||
				character == null ||
				character.AbilityController == null ||
				character.AbilityController.KnowsAbility(template.ID) ||
				character.InventoryController.Currency < template.Price)
			{
				return;
			}

			// learn the ability
			character.AbilityController.LearnBaseAbilities(new List<FBaseAbilityTemplate> { template });

			// remove the price from the characters currency
			character.InventoryController.Currency -= template.Price;

			// add the known ability to the database
			FCharacterKnownAbilityService.Add(dbContext, character.ID.Value, template.ID);

			// tell the client about the new ability event
			conn.Broadcast(new KnownAbilityAddBroadcast()
			{
				templateID = template.ID,
			}, true, Channel.Reliable);
		}

		public void OnServerAbilityCraftBroadcastReceived(NetworkConnection conn, FAbilityCraftBroadcast msg, Channel channel)
		{
			if (conn == null)
			{
				return;
			}

			// validate connection character
			if (conn.FirstObject == null)
			{
				return;
			}
			Character character = conn.FirstObject.GetComponent<Character>();
			if (character == null &&
				character.AbilityController != null)
			{
				return;
			}

			// validate main ability exists
			FAbilityTemplate mainAbility = FAbilityTemplate.Get<FAbilityTemplate>(msg.templateID);
			if (mainAbility == null)
			{
				return;
			}

			// validate that the character knows the main ability
			if (!character.AbilityController.KnowsAbility(mainAbility.ID))
			{
				return;
			}

			long price = mainAbility.Price;

			// validate eventIds if there are any...
			if (msg.events != null)
			{
				for (int i = 0; i < msg.events.Count; ++i)
				{
					FAbilityEvent eventTemplate = FAbilityEvent.Get<FAbilityEvent>(msg.events[i]);
					if (eventTemplate == null)
					{
						// couldn't validate this event...
						return;
					}
					// validate that the character knows the ability event
					if (!character.AbilityController.KnowsAbility(eventTemplate.ID))
					{
						return;
					}
					price += eventTemplate.Price;
				}
			}

			if (character.Currency.Value < price)
			{
				return;
			}

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			if (dbContext == null)
			{
				return;
			}
			if (FCharacterAbilityService.GetCount(dbContext, character.ID.Value) >= MaxAbilityCount)
			{
				// too many abilities! tell the player to forget a few of them first...
				return;
			}
			FAbility newAbility = new FAbility(mainAbility, msg.events);
			if (newAbility == null)
			{
				return;
			}

			
				FCharacterAbilityService.UpdateOrAdd(dbContext, character.ID.Value, newAbility);
				character.AbilityController.LearnAbility(newAbility);

				character.Currency.Value -= price;

				AbilityAddBroadcast abilityAddBroadcast = new AbilityAddBroadcast()
				{
					id = newAbility.ID,
					templateID = newAbility.Template.ID,
					events = msg.events,
				};

				conn.Broadcast(abilityAddBroadcast, true, Channel.Reliable);
			
		}
	}
}