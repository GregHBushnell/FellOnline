using FishNet.Connection;
using System.Collections.Generic;
using FishNet.Transporting;
using FellOnline.Server.DatabaseServices;
using FellOnline.Shared;

namespace FellOnline.Server
{
	/// <summary>
	/// Server Select system.
	/// </summary>
	public class FServerSelectSystem : FServerBehaviour
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
				List<WorldServerDetails> worldServerList = FWorldServerService.GetServerList(dbContext, IdleTimeout);

				ServerListBroadcast serverListMsg = new ServerListBroadcast()
				{
					servers = worldServerList
				};

				conn.Broadcast(serverListMsg, true, Channel.Reliable);
			}
		}
	}
}