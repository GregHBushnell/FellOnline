﻿using FishNet.Connection;
using System.Collections.Generic;
using FishNet.Transporting;
using FellOnline.Server.DatabaseServices;
using FellOnline.Shared;

namespace FellOnline.Server
{
	/// <summary>
	/// Server Select system.
	/// </summary>
	public class ServerSelectSystem : ServerBehaviour
	{
		public float IdleTimeout = 60;

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

		private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
		{
			if (obj.ConnectionState == LocalConnectionState.Started)
			{
				ServerManager.RegisterBroadcast<RequestServerListBroadcast>(OnServerRequestServerListBroadcastReceived, true);
			}
			else if (obj.ConnectionState == LocalConnectionState.Stopped)
			{
				ServerManager.UnregisterBroadcast<RequestServerListBroadcast>(OnServerRequestServerListBroadcastReceived);
			}
		}

		private void OnServerRequestServerListBroadcastReceived(NetworkConnection conn, RequestServerListBroadcast msg, Channel channel)
		{
			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			
			if (conn.IsActive)
			{
				List<WorldServerDetails> worldServerList = WorldServerService.GetServerList(dbContext, IdleTimeout);

				ServerListBroadcast serverListMsg = new ServerListBroadcast()
				{
					servers = worldServerList
				};

			Server.Broadcast(conn, serverListMsg, true, Channel.Reliable);
			}
		}
	}
}