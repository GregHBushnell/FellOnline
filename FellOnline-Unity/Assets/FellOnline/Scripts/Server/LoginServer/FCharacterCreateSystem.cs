using FishNet.Connection;
using FishNet.Transporting;
using System;
using FellOnline.Database.Npgsql.Entities;
using FellOnline.Server.DatabaseServices;
using FellOnline.Shared;
using FishNet.Object;
using System.Diagnostics;

namespace FellOnline.Server
{
	/// <summary>
	/// Server Character Creation system.
	/// </summary>
	public class FCharacterCreateSystem : FServerBehaviour
	{
		public int MaxCharacters = 8;

		public FWorldSceneDetailsCache WorldSceneDetailsCache;

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
				ServerManager.RegisterBroadcast<CharacterCreateBroadcast>(OnServerCharacterCreateBroadcastReceived, true);
				
			}
			else if (obj.ConnectionState == LocalConnectionState.Stopped)
			{
				ServerManager.UnregisterBroadcast<CharacterCreateBroadcast>(OnServerCharacterCreateBroadcastReceived);
			}
		}

		private void OnServerCharacterCreateBroadcastReceived(NetworkConnection conn, CharacterCreateBroadcast msg, Channel channel)
		{
			if (conn.IsActive)
			{
				// validate character creation data
				if (!FAuthenticationHelper.IsAllowedCharacterName(msg.characterName))
				{
					// invalid character name
					conn.Broadcast(new CharacterCreateResultBroadcast()
					{
						result = CharacterCreateResult.InvalidCharacterName,
					}, true, Channel.Reliable);
					return;
				}

				using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
				if (!FAccountManager.GetAccountNameByConnection(conn, out string accountName))
				{
					// account not found??
					conn.Kick(FishNet.Managing.Server.KickReason.UnusualActivity);
					return;
				}
				int characterCount = FCharacterService.GetCount(dbContext, accountName);
				if (characterCount >= MaxCharacters)
				{
					// too many characters
					conn.Broadcast(new CharacterCreateResultBroadcast()
					{
						result = CharacterCreateResult.TooMany,
					}, true, Channel.Reliable);
					return;
				}
				var character = FCharacterService.GetByName(dbContext, msg.characterName);
				if (character != null)
				{
					// character name already taken
					conn.Broadcast(new CharacterCreateResultBroadcast()
					{
						result = CharacterCreateResult.CharacterNameTaken,
					}, true, Channel.Reliable);
					return;
				}

				if (WorldSceneDetailsCache == null ||
					WorldSceneDetailsCache.Scenes == null ||
					WorldSceneDetailsCache.Scenes.Count < 1)
				{
					// failed to find spawn positions to validate with
					conn.Broadcast(new CharacterCreateResultBroadcast()
					{
						result = CharacterCreateResult.InvalidSpawn,
					}, true, Channel.Reliable);
					return;
				}
				// validate spawn details
				if (WorldSceneDetailsCache.Scenes.TryGetValue(msg.initialSpawnPosition.SceneName, out FWorldSceneDetails details))
				{
					int raceIndex = -1;
					// validate spawner
					if (details.InitialSpawnPositions.TryGetValue(msg.initialSpawnPosition.SpawnerName, out FCharacterInitialSpawnPosition initialSpawnPosition))
					{
						if (msg.raceIndex > -1 &&
							msg.raceIndex < Server.NetworkManager.SpawnablePrefabs.GetObjectCount())
						{
							NetworkObject prefab = Server.NetworkManager.SpawnablePrefabs.GetObject(true, msg.raceIndex);
							if (prefab != null)
							{
								Character prefabCharacter = prefab.gameObject.GetComponent<Character>();
								if (prefabCharacter != null)
								{
									raceIndex = msg.raceIndex;
								}
							}
						}

						// invalid race id! fall back to first race....
						if (raceIndex < 0)
						{
							for (int i = 0; i < Server.NetworkManager.SpawnablePrefabs.GetObjectCount(); ++i)
							{
								NetworkObject prefab = Server.NetworkManager.SpawnablePrefabs.GetObject(true, i);
								if (prefab != null)
								{
									// ensure it's a Character type
									Character prefabCharacter = prefab.gameObject.GetComponent<Character>();
									if (prefabCharacter != null)
									{
										raceIndex = i;
										break;
									}
								}
							}
						}
						
		
						var newCharacter = new CharacterEntity()
						{
							Account = accountName,
							Name = msg.characterName,
							NameLowercase = msg.characterName?.ToLower(),
							RaceID = raceIndex,
							SceneName = initialSpawnPosition.SceneName,
							X = initialSpawnPosition.Position.x,
							Y = initialSpawnPosition.Position.y,
							Z = initialSpawnPosition.Position.z,
							RotX = initialSpawnPosition.Rotation.x,
							RotY = initialSpawnPosition.Rotation.y,
							RotZ = initialSpawnPosition.Rotation.z,
							RotW = initialSpawnPosition.Rotation.w,
							TimeCreated = DateTime.UtcNow,
						};
						
						var newAppearance = new CharacterAppearanceEntity()
						{
							Character = newCharacter,
							SkinColor = msg.skinColor,
							HairID = msg.hairIndex,
							HairColor = msg.hairColor,
						};
						newCharacter.Appearance = newAppearance;
						//.CharacterAppearance.Add(newAppearance);
						dbContext.Characters.Add(newCharacter);
						dbContext.SaveChanges();

						// send success to the client
						conn.Broadcast(new CharacterCreateResultBroadcast()
						{
							result = CharacterCreateResult.Success,
						}, true, Channel.Reliable);

						// send the create broadcast back to the client
						conn.Broadcast(msg, true, Channel.Reliable);
					}
				}
			}
		}
	}
}