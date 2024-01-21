using System.Collections.Generic;
using UnityEngine;
using FishNet.Transporting;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class UIBank : FUICharacterControl
	{
		public RectTransform content;
		public UIBankButton buttonPrefab;
		public List<UIBankButton> bankSlots = null;

        public override void OnStarting()
		{
			Client.NetworkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
		}

		public override void OnDestroying()
		{
            Client.NetworkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
			DestroySlots();
		}

		private void DestroySlots()
		{
			if (bankSlots != null)
			{
				for (int i = 0; i < bankSlots.Count; ++i)
				{
					Destroy(bankSlots[i].gameObject);
				}
				bankSlots.Clear();
			}
		}

		public void ClientManager_OnClientConnectionState(ClientConnectionStateArgs args)
		{
			if (args.ConnectionState == LocalConnectionState.Started)
			{
				Client.NetworkManager.ClientManager.RegisterBroadcast<BankerBroadcast>(OnClientBankerBroadcastReceived);
			}
			else if (args.ConnectionState == LocalConnectionState.Stopped)
			{
				Client.NetworkManager.ClientManager.UnregisterBroadcast<BankerBroadcast>(OnClientBankerBroadcastReceived);
			}
		}

		private void OnClientBankerBroadcastReceived(BankerBroadcast msg, Channel channel)
		{
			if (FUIManager.TryGet("UIBank", out UIBank bank))
			{
				bank.Show();
			}
		}

		public override void OnPreSetCharacter()
		{
			if (Character != null)
			{
				Character.BankController.OnSlotUpdated -= OnBankSlotUpdated;
			}
		}

		public override void SetCharacter(Character character)
		{
			base.SetCharacter(character);

			if (Character == null ||
				content == null ||
				buttonPrefab == null ||
				Character.BankController == null)
			{
				return;
			}

			// destroy the old slots
			Character.BankController.OnSlotUpdated -= OnBankSlotUpdated;
			DestroySlots();

			// generate new slots
			bankSlots = new List<UIBankButton>();
			for (int i = 0; i < Character.BankController.Items.Count; ++i)
			{
				UIBankButton button = Instantiate(buttonPrefab, content);
				button.Character = Character;
				button.ReferenceID = i;
				button.Type = FReferenceButtonType.Bank;
				if (Character.BankController.TryGetItem(i, out FItem item))
				{
					if (button.Icon != null)
					{
						button.Icon.sprite = item.Template.Icon;
					}
                    if (button.AmountText != null)
					{
						button.AmountText.text = item.IsStackable ? item.Stackable.Amount.ToString() : "";
					}
				}
				bankSlots.Add(button);
			}
			// update our buttons when the bank slots change
			Character.BankController.OnSlotUpdated += OnBankSlotUpdated;
		}

		public void OnBankSlotUpdated(FItemContainer container, FItem item, int bankIndex)
		{
			if (bankSlots == null)
			{
				return;
			}

			if (!container.IsSlotEmpty(bankIndex))
			{
				// update our button display
				UIBankButton button = bankSlots[bankIndex];
				button.Type = FReferenceButtonType.Bank;
				if (button.Icon != null)
				{
					button.Icon.sprite = item.Template.Icon;
				}
				//bankSlots[i].cooldownText = character.CooldownController.IsOnCooldown();
				if (button.AmountText != null)
				{
					button.AmountText.text = item.IsStackable ? item.Stackable.Amount.ToString() : "";
				}
			}
			else
			{
				// the item no longer exists
				bankSlots[bankIndex].Clear();
			}
		}
	}
}