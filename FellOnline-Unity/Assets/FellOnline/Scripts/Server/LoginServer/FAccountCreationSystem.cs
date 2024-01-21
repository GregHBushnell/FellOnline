using FishNet.Connection;
using FishNet.Transporting;
using FellOnline.Server.DatabaseServices;
using FellOnline.Shared;

namespace FellOnline.Server
{
	/// <summary>
	/// Account Creation system.
	/// </summary>
	public class FAccountCreationSystem : FServerBehaviour
	{
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
				ServerManager.RegisterBroadcast<FCreateAccountBroadcast>(OnServerCreateAccountBroadcastReceived, false);
			}
			else if (obj.ConnectionState == LocalConnectionState.Stopped)
			{
				ServerManager.UnregisterBroadcast<FCreateAccountBroadcast>(OnServerCreateAccountBroadcastReceived);
			}
		}

		private void OnServerCreateAccountBroadcastReceived(NetworkConnection conn, FCreateAccountBroadcast msg, Channel channel)
		{
			FClientAuthenticationResult result = FClientAuthenticationResult.InvalidUsernameOrPassword;
			if (Server.NpgsqlDbContextFactory != null)
			{
				using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
				if (dbContext != null)
				{
					result = FAccountService.TryCreate(dbContext, msg.username, msg.salt, msg.verifier);
				}
			}
			conn.Broadcast(new ClientAuthResultBroadcast() { result = result }, false, Channel.Reliable);
		}
	}
}