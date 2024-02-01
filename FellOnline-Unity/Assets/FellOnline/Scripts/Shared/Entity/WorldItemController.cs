
using UnityEngine;
using FishNet.Transporting;
using FishNet.Object;
using System.Collections.Generic;
using System;
#if !UNITY_SERVER
using static FellOnline.Client.Client;
#endif
namespace FellOnline.Shared
{
	public class WorldItemController : CharacterBehaviour {
	
		private int CurrentTemplateID = 0;
		private int CurrentInteractableID = 0;
		public override void OnStartClient()
		{
			base.OnStartClient();

			if (!IsOwner)
			{
				enabled = false;
			}
			else
			{
				ClientManager.RegisterBroadcast<WorldItemBroadcast>(OnClientWorldItemBroadcastReceived);
			}
		}

		public override void OnStopClient()
		{
			base.OnStopClient();

			if (IsOwner)
			{
				ClientManager.UnregisterBroadcast<WorldItemBroadcast>(OnClientWorldItemBroadcastReceived);
			}
		}

		private void OnClientWorldItemBroadcastReceived(WorldItemBroadcast msg, Channel channel)
		{
#if !UNITY_SERVER
			CurrentInteractableID = msg.interactableID;
			CurrentTemplateID = msg.templateID;


			var message = new WorldItemPickupBroadcast()
			{
				interactableID = CurrentInteractableID,
				templateID = CurrentTemplateID
			};
			Broadcast(message, Channel.Reliable);

			AddWeaponAbilities(CurrentTemplateID);

#endif

		}

		public void AddWeaponAbilities(int templateID)
		{
#if !UNITY_SERVER
			if (templateID == 0)
			{
				return;
			}
			WorldItemTemplate template = WorldItemTemplate.Get<WorldItemTemplate>(templateID);
			
			WeaponTemplate WeaponItemTemplate = WeaponTemplate.Get<WeaponTemplate>(template.Item.ID);

			AbilityTemplate catagoryTemplate = WeaponItemTemplate.AbilityCategoryTemplate;
		
			AbilityEvent mainAttackEvent = WeaponItemTemplate.MainAttackEvent;
			List<AbilityEvent> events = WeaponItemTemplate.ExtraAbilityEvents;
			if (catagoryTemplate == null)
			{
				return;
			}

			List<int> eventIds = new List<int>();

			if (catagoryTemplate.EventSlots != 0)
			{
				eventIds.Add(mainAttackEvent.ID);
				for (int i = 0; i < events.Count; ++i)
				{
					
					if (events[i] != null)
					{
						eventIds.Add(events[i].ID);
					}
				}
			}

			AbilityLearnItemPickupBroadcast abilityLearnItemPickupAddBroadcast = new AbilityLearnItemPickupBroadcast()
			{
				templateID = catagoryTemplate.ID,
				events = eventIds,
			};

			Broadcast(abilityLearnItemPickupAddBroadcast, Channel.Reliable);
#endif
		}
	}
}