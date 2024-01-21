﻿using FishNet.Connection;
using FishNet.Transporting;
using FellOnline.Server.DatabaseServices;
using FellOnline.Shared;
using FellOnline.Database.Npgsql.Entities;


namespace FellOnline.Server
{
	/// <summary>
	/// Server friend system.
	/// </summary>
	public class FFriendSystem : FServerBehaviour
	{
		public int MaxFriends = 100;

		public override void InitializeOnce()
		{
			if (ServerManager != null &&
				Server.CharacterSystem != null)
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
				ServerManager.RegisterBroadcast<FriendAddNewBroadcast>(OnServerFriendAddNewBroadcastReceived, true);
				ServerManager.RegisterBroadcast<FriendRemoveBroadcast>(OnServerFriendRemoveBroadcastReceived, true);
			}
			else if (args.ConnectionState == LocalConnectionState.Stopped)
			{
				ServerManager.UnregisterBroadcast<FriendAddNewBroadcast>(OnServerFriendAddNewBroadcastReceived);
				ServerManager.UnregisterBroadcast<FriendRemoveBroadcast>(OnServerFriendRemoveBroadcastReceived);
			}
		}

		public void OnServerFriendAddNewBroadcastReceived(NetworkConnection conn, FriendAddNewBroadcast msg, Channel channel)
		{
			if (conn.FirstObject == null)
			{
				return;
			}
			FFriendController friendController = conn.FirstObject.GetComponent<FFriendController>();

			// validate character
			if (friendController == null ||
				friendController.Friends.Count > MaxFriends)
			{
				return;
			}

			// validate friend invite
			if (Server == null || Server.NpgsqlDbContextFactory == null)
			{
				return;
			}
			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			CharacterEntity friendEntity = FCharacterService.GetByName(dbContext, msg.characterName);
			if (friendEntity != null)
			{
				// add the friend to the database
				FCharacterFriendService.Save(dbContext, friendController.Character.ID.Value, friendEntity.ID);

				// tell the character they added a new friend!
				conn.Broadcast(new FriendAddBroadcast()
				{
					characterID = friendEntity.ID,
					online = friendEntity.Online,
				}, true, Channel.Reliable);
			}
		}

		public void OnServerFriendRemoveBroadcastReceived(NetworkConnection conn, FriendRemoveBroadcast msg, Channel channel)
		{
			if (Server.NpgsqlDbContextFactory == null)
			{
				return;
			}
			if (conn.FirstObject == null)
			{
				return;
			}
			FFriendController friendController = conn.FirstObject.GetComponent<FFriendController>();

			// validate character
			if (friendController == null)
			{
				return;
			}

			// remove the friend if it exists
			if (friendController.Friends.Contains(msg.characterID))
			{
				// remove the character from the friend in the database
				using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
				if (FCharacterFriendService.Delete(dbContext, friendController.Character.ID.Value, msg.characterID))
				{
					// tell the character they removed a friend
					conn.Broadcast(new FriendRemoveBroadcast()
					{
						characterID = msg.characterID,
					}, true, Channel.Reliable);
				}
			}
		}
	}
}