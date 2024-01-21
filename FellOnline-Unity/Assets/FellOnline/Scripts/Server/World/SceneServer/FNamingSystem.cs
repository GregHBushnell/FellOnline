using FishNet.Connection;
using FishNet.Transporting;
using FellOnline.Server.DatabaseServices;
using FellOnline.Shared;
//using UnityEngine;

public enum FNamingSystemType : byte
{
	CharacterName,
	GuildName,
}

namespace FellOnline.Server
{
	/// <summary>
	/// This is a simple naming service that provides clients with names of objects based on their ID.
	/// </summary>
	public class FNamingSystem : FServerBehaviour
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

		private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs args)
		{
			if (args.ConnectionState == LocalConnectionState.Started)
			{
				ServerManager.RegisterBroadcast<FNamingBroadcast>(OnServerNamingBroadcastReceived, true);
			}
			else if (args.ConnectionState == LocalConnectionState.Stopped)
			{
				ServerManager.UnregisterBroadcast<FNamingBroadcast>(OnServerNamingBroadcastReceived);
			}
		}

		/// <summary>
		/// Naming request broadcast received from a character.
		/// </summary>
		private void OnServerNamingBroadcastReceived(NetworkConnection conn, FNamingBroadcast msg, Channel channel)
		{
			switch (msg.type)
			{
				case FNamingSystemType.CharacterName:
					//Debug.Log("NamingSystem: Searching by Character ID: " + msg.id);
					// check our local scene server first
					if (Server.CharacterSystem != null &&
						Server.CharacterSystem.CharactersByID.TryGetValue(msg.id, out Character character))
					{
						//Debug.Log("NamingSystem: Character Local Result " + character.CharacterName);
						SendNamingBroadcast(conn, FNamingSystemType.CharacterName, msg.id, character.CharacterName);
					}
					// then check the database
					else if (Server.NpgsqlDbContextFactory != null)
					{
						using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
						string name = FCharacterService.GetNameByID(dbContext, msg.id);
						if (!string.IsNullOrWhiteSpace(name))
						{
							//Debug.Log("NamingSystem: Character Database Result " + name);
							SendNamingBroadcast(conn, FNamingSystemType.CharacterName, msg.id, name);
						}
					}
					break;
				case FNamingSystemType.GuildName:
					//Debug.Log("NamingSystem: Searching by Guild ID: " + msg.id);
					// get the name from the database
					if (Server.NpgsqlDbContextFactory != null)
					{
						using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
						string name = FGuildService.GetNameByID(dbContext, msg.id);
						if (!string.IsNullOrWhiteSpace(name))
						{
							//Debug.Log("NamingSystem: Guild Database Result " + name);
							SendNamingBroadcast(conn, FNamingSystemType.GuildName, msg.id, name);
						}
					}
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Allows the server to send naming requests to the connection
		/// </summary>
		public void SendNamingBroadcast(NetworkConnection conn, FNamingSystemType type, long id, string name)
		{
			if (conn == null)
				return;

			FNamingBroadcast msg = new FNamingBroadcast()
			{
				type = type,
				id = id,
				name = name,
			};

			conn.Broadcast(msg, true, Channel.Reliable);
		}
	}
}