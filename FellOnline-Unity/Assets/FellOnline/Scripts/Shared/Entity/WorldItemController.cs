
using UnityEngine;
using FishNet.Transporting;
using FellOnline.Client;
using FishNet.Object;
namespace FellOnline.Shared
{
public class WorldItemController : NetworkBehaviour {
    
		public Character Character;
        private int CurrentTemplateID = 0;
		private int CurrentInteractableID = 0;
        public override void OnStartClient()
		{
			base.OnStartClient();

			if (!base.IsOwner)
			{
				this.enabled = false;
			}
			else
			{
				ClientManager.RegisterBroadcast<WorldItemBroadcast>(OnClientWorldItemBroadcastReceived);
			}
		}

		public override void OnStopClient()
		{
			base.OnStopClient();

			if (base.IsOwner)
			{
				ClientManager.UnregisterBroadcast<WorldItemBroadcast>(OnClientWorldItemBroadcastReceived);
			}
		}

        private void OnClientWorldItemBroadcastReceived(WorldItemBroadcast msg, Channel channel)
		{
			CurrentInteractableID = msg.interactableID;
			CurrentTemplateID = msg.templateID;
			//WorldItemTemplate template = WorldItemTemplate.Get<WorldItemTemplate>(CurrentTemplateID);
            //if (template != null)
           // {

                WorldItemPickupBroadcast message = new WorldItemPickupBroadcast()
                {
                    interactableID = CurrentInteractableID,
					templateID = CurrentTemplateID
                };
                ClientManager.Broadcast(message, Channel.Reliable);
           // }else{
                // Debug.LogError("Template with ID " + CurrentTemplateID + " not found");
           // }
		}
    }
}