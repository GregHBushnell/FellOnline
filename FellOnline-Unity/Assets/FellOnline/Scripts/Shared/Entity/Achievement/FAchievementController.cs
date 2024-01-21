#if !UNITY_SERVER
using FellOnline.Client;
using FishNet;
using UnityEngine;
using System;
#endif
using FishNet.Object;
using FishNet.Transporting;
using System.Collections.Generic;

namespace FellOnline.Shared
{
	public class FAchievementController : NetworkBehaviour
	{
		public Character Character;

		private Dictionary<int, FAchievement> achievements = new Dictionary<int, FAchievement>();


		public Dictionary<int, FAchievement> Achievements { get { return achievements; } }

#if !UNITY_SERVER
		public bool ShowAchievementCompletion = true;
		public event Func<string, Vector3, Color, float, float, bool, FCached3DLabel> OnCompleteAchievement;

		public override void OnStartClient()
		{
			base.OnStartClient();

			if (!base.IsOwner)
			{
				enabled = false;
				return;
			}

			if (FLabelMaker.Instance != null)
			{
				OnCompleteAchievement += FLabelMaker.Display;
			}

			ClientManager.RegisterBroadcast<AchievementUpdateBroadcast>(OnClientAchievementUpdateBroadcastReceived);
			ClientManager.RegisterBroadcast<AchievementUpdateMultipleBroadcast>(OnClientAchievementUpdateMultipleBroadcastReceived);
		}

		public override void OnStopClient()
		{
			base.OnStopClient();

			if (base.IsOwner)
			{
				if (FLabelMaker.Instance != null)
				{
					OnCompleteAchievement -= FLabelMaker.Display;
				}

				ClientManager.UnregisterBroadcast<AchievementUpdateBroadcast>(OnClientAchievementUpdateBroadcastReceived);
				ClientManager.UnregisterBroadcast<AchievementUpdateMultipleBroadcast>(OnClientAchievementUpdateMultipleBroadcastReceived);
			}
		}

		/// <summary>
		/// Server sent an achievement update broadcast.
		/// </summary>
		private void OnClientAchievementUpdateBroadcastReceived(AchievementUpdateBroadcast msg, Channel channel)
		{
			FAchievementTemplate template = FAchievementTemplate.Get<FAchievementTemplate>(msg.templateID);
			if (template != null &&
				achievements.TryGetValue(template.ID, out FAchievement achievement))
			{
				achievement.CurrentValue = msg.newValue;
			}
		}

		/// <summary>
		/// Server sent a multiple achievement update broadcast.
		/// </summary>
		private void OnClientAchievementUpdateMultipleBroadcastReceived(AchievementUpdateMultipleBroadcast msg, Channel channel)
		{
			foreach (AchievementUpdateBroadcast subMsg in msg.achievements)
			{
				FAchievementTemplate template = FAchievementTemplate.Get<FAchievementTemplate>(subMsg.templateID);
				if (template != null &&
					achievements.TryGetValue(template.ID, out FAchievement achievement))
				{
					achievement.CurrentValue = subMsg.newValue;
				}
			}
		}
#endif

		public void SetAchievement(int templateID, byte tier, uint value)
		{
			if (achievements == null)
			{
				achievements = new Dictionary<int, FAchievement>();
			}

			if (achievements.TryGetValue(templateID, out FAchievement achievement))
			{
				achievement.CurrentTier = tier;
				achievement.CurrentValue = value;
			}
			else
			{
				achievements.Add(templateID, new FAchievement(templateID, tier, value));
			}
		}

		public bool TryGetAchievement(int templateID, out FAchievement achievement)
		{
			return achievements.TryGetValue(templateID, out achievement);
		}

		public void Increment(FAchievementTemplate template, uint amount)
		{
			if (template == null)
			{
				return;
			}
			if (achievements == null)
			{
				achievements = new Dictionary<int, FAchievement>();
			}

			FAchievement achievement;
			if (!achievements.TryGetValue(template.ID, out achievement))
			{
				achievements.Add(template.ID, achievement = new FAchievement(template.ID));
			}

			// get the old values
			byte currentTier = achievement.CurrentTier;
			uint currentValue = achievement.CurrentValue;

			// update current value
			achievement.CurrentValue += amount;

			List<FAchievementTier> tiers = template.Tiers;
			if (tiers != null)
			{
				for (byte i = currentTier; i < tiers.Count && i < byte.MaxValue; ++i)
				{
					FAchievementTier tier = tiers[i];
					if (achievement.CurrentValue > tier.MaxValue)
					{
#if UNITY_SERVER
						HandleRewards(tier);
#else
						// Display a text message above the characters head showing the achievement.
						OnCompleteAchievement?.Invoke("Achievement: " + achievement.Template.Name + " " + tier.TierCompleteMessage, Character.Transform.position, Color.yellow, 12.0f, 10.0f, false);
#endif
					}
					else
					{
						achievement.CurrentTier = i;
						break;
					}

				}
			}
		}

#if UNITY_SERVER
	private void HandleRewards(FAchievementTier tier)
	{
		if (base.IsServerInitialized   && Character.Owner != null)
		{
			FBaseItemTemplate[] itemRewards = tier.ItemRewards;
			if (itemRewards != null && itemRewards.Length > 0 && Character.InventoryController.FreeSlots() >= itemRewards.Length)
			{
				InventorySetMultipleItemsBroadcast inventorySetMultipleItemsBroadcast = new InventorySetMultipleItemsBroadcast()
				{
					items = new List<InventorySetItemBroadcast>(),
				};

				List<InventorySetItemBroadcast> modifiedItemBroadcasts = new List<InventorySetItemBroadcast>();

				for (int i = 0; i < itemRewards.Length; ++i)
				{
					FItem newItem = new FItem(123, 0, itemRewards[i].ID, 1);

					if (Character.InventoryController.TryAddItem(newItem, out List<FItem> modifiedItems))
					{
						foreach (FItem item in modifiedItems)
						{
							modifiedItemBroadcasts.Add(new InventorySetItemBroadcast()
							{
								instanceID = newItem.ID,
								templateID = newItem.Template.ID,
								slot = newItem.Slot,
								seed = newItem.IsGenerated ? newItem.Generator.Seed : 0,
								stackSize = newItem.IsStackable ? newItem.Stackable.Amount : 0,
							});
						}
					}
				}
				if (modifiedItemBroadcasts.Count > 0)
				{
					Character.Owner.Broadcast(new InventorySetMultipleItemsBroadcast()
					{
						items = modifiedItemBroadcasts,
					}, true, Channel.Reliable);
				}
			}
		}
	}
#endif
	}
}