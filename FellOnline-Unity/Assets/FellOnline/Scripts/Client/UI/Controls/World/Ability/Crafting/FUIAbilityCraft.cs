using FishNet.Transporting;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class FUIAbilityCraft : FUICharacterControl
	{
		private const int MAX_CRAFT_EVENT_SLOTS = 10;

		public FUITooltipButton MainEntry;
		public TMP_Text AbilityDescription;
		public TMP_Text CraftCost;
		public RectTransform AbilityEventParent;
		public FUITooltipButton AbilityEventPrefab;

		private List<FUITooltipButton> EventSlots;

		public override void OnStarting()
		{
			Client.NetworkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
			if (MainEntry != null)
			{
				MainEntry.OnLeftClick += MainEntry_OnLeftClick;
				MainEntry.OnRightClick += MainEntry_OnRightClick;
			}
		}

		public override void OnDestroying()
		{
			Client.NetworkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;

			if (MainEntry != null)
			{
				MainEntry.OnLeftClick -= MainEntry_OnLeftClick;
				MainEntry.OnRightClick -= MainEntry_OnRightClick;
			}

			ClearSlots();
		}

		public void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
		{
			if (args.ConnectionState == LocalConnectionState.Started)
			{
				Client.NetworkManager.ClientManager.RegisterBroadcast<FAbilityCrafterBroadcast>(OnClientAbilityCrafterBroadcastReceived);
			}
			else if (args.ConnectionState == LocalConnectionState.Stopped)
			{
				Client.NetworkManager.ClientManager.UnregisterBroadcast<FAbilityCrafterBroadcast>(OnClientAbilityCrafterBroadcastReceived);
			}
		}

		private void OnClientAbilityCrafterBroadcastReceived(FAbilityCrafterBroadcast msg, Channel channel)
		{
			Show();
		}

		private void MainEntry_OnLeftClick(int index, object[] optionalParams)
		{
			if (Character != null &&
				Character.AbilityController != null &&
				FUIManager.TryGet("UISelector", out FUISelector uiSelector))
			{
				List<FICachedObject> templates = FAbilityTemplate.Get<FAbilityTemplate>(Character.AbilityController.KnownBaseAbilities);
				uiSelector.Open(templates, (i) =>
				{
					FAbilityTemplate template = FAbilityTemplate.Get<FAbilityTemplate>(i);
					if (template != null)
					{
						MainEntry.Initialize(Character, template);
						SetEventSlots(template.EventSlots);
						
						// update the main description text
						UpdateMainDescription();
					}
				});
			}
		}

		private void MainEntry_OnRightClick(int index, object[] optionalParams)
		{
			MainEntry.Clear();
			ClearSlots();
			// update the main description text
			UpdateMainDescription();
		}

		private void EventEntry_OnLeftClick(int index, object[] optionalParams)
		{
			if (index > -1 && index < EventSlots.Count &&
				Character != null &&
				Character.AbilityController != null &&
				FUIManager.TryGet("UISelector", out FUISelector uiSelector))
			{
				List<FICachedObject> templates = FAbilityEvent.Get<FAbilityEvent>(Character.AbilityController.KnownEvents);
				uiSelector.Open(templates, (i) =>
				{
					FAbilityEvent template = FAbilityEvent.Get<FAbilityEvent>(i);
					if (template != null)
					{
						EventSlots[index].Initialize(Character, template);
					}
					
					// update the main description text
					UpdateMainDescription();
				});
			}
		}

		private void EventEntry_OnRightClick(int index, object[] optionalParams)
		{
			if (index > -1 && index < EventSlots.Count)
			{
				EventSlots[index].Clear();
				// update the main description text
				UpdateMainDescription();
			}
		}
		private void UpdateMainDescription()
		{
			if (AbilityDescription == null)
			{
				return;
			}
			if (MainEntry == null ||
				MainEntry.Tooltip == null)
			{
				AbilityDescription.text = "";
				if (CraftCost != null)
				{
					CraftCost.text = "Cost: ";
				}
				return;
			}

			long price = 0;
			FAbilityTemplate abilityTemplate = MainEntry.Tooltip as FAbilityTemplate;
			if (abilityTemplate != null)
			{
				price = abilityTemplate.Price;
			}

			if (EventSlots != null &&
				EventSlots.Count > 0)
			{
				List<FITooltip> tooltips = new List<FITooltip>();
				foreach (FUITooltipButton button in EventSlots)
				{
					if (button.Tooltip == null)
					{
						continue;
					}
					tooltips.Add(button.Tooltip);

					
					FAbilityEvent eventTemplate = button.Tooltip as FAbilityEvent;
					if (eventTemplate != null)
					{
						price += eventTemplate.Price;
					}
				}
				AbilityDescription.text = MainEntry.Tooltip.Tooltip(tooltips);
			}
			else
			{
				AbilityDescription.text = MainEntry.Tooltip.Tooltip();
			}
			if (CraftCost != null)
			{
				CraftCost.text = "Cost: " + price.ToString();
			}
		}

		private void ClearSlots()
		{
			if (EventSlots != null)
			{
				for (int i = 0; i < EventSlots.Count; ++i)
				{
					if (EventSlots[i] == null)
					{
						continue;
					}
					if (EventSlots[i].gameObject != null)
					{
						Destroy(EventSlots[i].gameObject);
					}
				}
				EventSlots.Clear();
			}
		}

		private void SetEventSlots(int count)
		{
			ClearSlots();

			EventSlots = new List<FUITooltipButton>();

			for (int i = 0; i < count && i < MAX_CRAFT_EVENT_SLOTS; ++i)
			{
				FUITooltipButton eventButton = Instantiate(AbilityEventPrefab, AbilityEventParent);
				eventButton.Initialize(i, EventEntry_OnLeftClick, EventEntry_OnRightClick);
				EventSlots.Add(eventButton);
			}
		}

		public void OnCraft()
		{
			if (MainEntry == null)
			{
				return;
			}

			FAbilityTemplate main = MainEntry.Tooltip as FAbilityTemplate;
			if (main == null)
			{
				return;
			}

			long price = main.Price;

			List<int> eventIds = new List<int>();

			if (EventSlots != null)
			{
				for (int i = 0; i < EventSlots.Count; ++i)
				{
					FAbilityEvent template = EventSlots[i].Tooltip as FAbilityEvent;
					if (template != null)
					{
						eventIds.Add(template.ID);
						price += template.Price;
					}
				}
			}

			if (Character.Currency.Value < price)
			{
				return;
			}

			FAbilityCraftBroadcast abilityAddBroadcast = new FAbilityCraftBroadcast()
			{
				templateID = main.ID,
				events = eventIds,
			};

			Client.NetworkManager.ClientManager?.Broadcast(abilityAddBroadcast, Channel.Reliable);

			MainEntry.Clear();
			ClearSlots();

			// update the main description text
			UpdateMainDescription();
		}
	}
}