using FishNet.Transporting;
using System.Collections.Generic;
using UnityEngine;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class FUIFriendList : FUIControl
	{
		public RectTransform FriendParent;
		public FUIFriend FriendPrefab;
		public Dictionary<long, FUIFriend> Friends = new Dictionary<long, FUIFriend>();

		public override void OnStarting()
		{
		}

		public override void OnDestroying()
		{
			foreach (FUIFriend friend in new List<FUIFriend>(Friends.Values))
			{
				friend.FriendID = 0;
				friend.OnRemoveFriend = null;
				Destroy(friend.gameObject);
			}
			Friends.Clear();
		}

		public void OnAddFriend(long friendID, bool online)
		{
			if (FriendPrefab != null && FriendParent != null)
			{
				if (!Friends.TryGetValue(friendID, out FUIFriend uiFriend))
				{
					uiFriend = Instantiate(FriendPrefab, FriendParent);
					uiFriend.FriendID = friendID;
					uiFriend.OnRemoveFriend += OnButtonRemoveFriend;
					Friends.Add(friendID, uiFriend);
				}
				if (uiFriend != null)
				{
					if (uiFriend.Name != null)
					{
						FClientNamingSystem.SetName(FNamingSystemType.CharacterName, friendID, (n) =>
						{
							uiFriend.Name.text = n;
						});
					}
					if (uiFriend.Status != null)
					{
						uiFriend.Status.text = online ? "Online" : "Offline";
					}
				}
			}
		}

		public void OnRemoveFriend(long friendID)
		{
			if (Friends.TryGetValue(friendID, out FUIFriend friend))
			{
				Friends.Remove(friendID);
				friend.FriendID = 0;
				friend.OnRemoveFriend = null;
				Destroy(friend.gameObject);
			}
		}

		private void OnButtonRemoveFriend(long friendID)
		{
			if (FUIManager.TryGet("UIConfirmationTooltip", out FUIConfirmationTooltip tooltip))
			{
				tooltip.Open("Are you sure you want to remove your friend?", () =>
				{
					Client.NetworkManager.ClientManager.Broadcast(new FriendRemoveBroadcast()
					{
						characterID = friendID,
					}, Channel.Reliable);
				}, null);
			}
		}

		public void OnButtonAddFriend()
		{
			if (FriendPrefab != null && FriendParent != null)
			{
				if (FUIManager.TryGet("UIInputConfirmationTooltip", out FUIInputConfirmationTooltip tooltip))
				{
					tooltip.Open("Who would you like to add as a friend?", (s) =>
					{
						Client.NetworkManager.ClientManager.Broadcast(new FriendAddNewBroadcast()
						{
							characterName = s,
						}, Channel.Reliable);
					}, null);
				}
			}
		}
	}
}