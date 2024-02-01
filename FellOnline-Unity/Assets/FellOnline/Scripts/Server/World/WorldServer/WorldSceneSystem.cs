using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Transporting;
using System.Collections.Generic;
using FellOnline.Server.DatabaseServices;
using FellOnline.Shared;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;
using UnityEngine;

namespace FellOnline.Server
{
	// World scene system handles the node services
	public class WorldSceneSystem : ServerBehaviour
	{
		private const int MAX_CLIENTS_PER_INSTANCE = 100;

		private WorldServerAuthenticator loginAuthenticator;

		public WorldSceneDetailsCache WorldSceneDetailsCache;

		// sceneName, waitingConnections
		/// <summary>
		/// Connections waiting for a scene to finish loading.
		/// </summary>
		public Dictionary<string, HashSet<NetworkConnection>> WaitingConnections = new Dictionary<string, HashSet<NetworkConnection>>();
		public Dictionary<NetworkConnection, string> WaitingConnectionsScenes = new Dictionary<NetworkConnection, string>();

		public int ConnectionCount { get; private set; }

		private float waitQueueRate = 2.0f;
		private float nextWaitQueueUpdate = 0.0f;

		public override void InitializeOnce()
		{
			loginAuthenticator = FindObjectOfType<WorldServerAuthenticator>();

			if (ServerManager != null)
			{
				ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
				ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
			}
			else
			{
				enabled = false;
			}
		}

		private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs args)
		{
			if (loginAuthenticator == null)
				return;

			if (args.ConnectionState == LocalConnectionState.Started)
			{
				loginAuthenticator.OnClientAuthenticationResult += Authenticator_OnClientAuthenticationResult;
			}
			else if (args.ConnectionState == LocalConnectionState.Stopped)
			{
				loginAuthenticator.OnClientAuthenticationResult -= Authenticator_OnClientAuthenticationResult;
			}
		}

		private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
		{
			if (args.ConnectionState == RemoteConnectionState.Stopped)
			{
				if (WaitingConnectionsScenes.TryGetValue(conn, out string sceneName))
				{
					if (WaitingConnections.TryGetValue(sceneName, out HashSet<NetworkConnection> connections))
					{
						connections.Remove(conn);
						if (connections.Count < 1)
						{
							WaitingConnections.Remove(sceneName);
						}
					}
					WaitingConnectionsScenes.Remove(conn);
				}
			}
		}

		private void OnApplicationQuit()
		{
			if (Server != null &&
				Server.NpgsqlDbContextFactory != null &&
				ServerBehaviour.TryGet(out WorldServerSystem worldServerSystem))
			{
				using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
				PendingSceneService.Delete(dbContext, worldServerSystem.ID);
			}
		}

		private void LateUpdate()
		{
			if (nextWaitQueueUpdate < 0)
			{
				nextWaitQueueUpdate = waitQueueRate;

				if (Server != null && Server.NpgsqlDbContextFactory != null)
				{
					using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
					foreach (string sceneName in new List<string>(WaitingConnections.Keys))
					{
						TryClearWaitQueues(dbContext, sceneName);
					}

					UpdateConnectionCount(dbContext);
				}
			}
			nextWaitQueueUpdate -= Time.deltaTime;
		}

		private void TryClearWaitQueues(NpgsqlDbContext dbContext, string sceneName)
		{
			if (WaitingConnections.TryGetValue(sceneName, out HashSet<NetworkConnection> connections))
			{
				if (connections == null ||
					connections.Count < 1 ||
					!ServerBehaviour.TryGet(out WorldServerSystem worldServerSystem))
				{
					WaitingConnections.Remove(sceneName);
					return;
				}

				List<LoadedSceneEntity> loadedScenes = LoadedSceneService.GetServerList(dbContext, worldServerSystem.ID, sceneName, MAX_CLIENTS_PER_INSTANCE);
				if (loadedScenes == null || loadedScenes.Count < 1)
				{
					return;
				}

				foreach (LoadedSceneEntity loadedScene in loadedScenes)
				{
					if (connections.Count < 1)
					{
						break;
					}

					SceneServerEntity sceneServer = SceneServerService.GetServer(dbContext, loadedScene.SceneServerID);
					if (sceneServer == null)
					{
						continue;
					}

					foreach (NetworkConnection connection in new List<NetworkConnection>(connections))
					{
						int maxClientsPerInstance = MAX_CLIENTS_PER_INSTANCE;

						// see if we can get a per scene max client count
						if (WorldSceneDetailsCache != null &&
							WorldSceneDetailsCache.Scenes != null &&
							WorldSceneDetailsCache.Scenes.TryGetValue(sceneName, out WorldSceneDetails details))
						{
							maxClientsPerInstance = details.MaxClients;
						}

						// clamp at 1 to 100
						maxClientsPerInstance = maxClientsPerInstance.Clamp(1, MAX_CLIENTS_PER_INSTANCE);

						// if we are at maximum capacity on this server move to the next one
						if (loadedScene.CharacterCount >= maxClientsPerInstance)
						{
							continue;
						}

						// clear the connection from our wait queues
						connections.Remove(connection);
						WaitingConnectionsScenes.Remove(connection);

						// if the connection is no longer active
						if (connection == null || !connection.IsActive || !AccountManager.GetAccountNameByConnection(connection, out string accountName))
						{
							continue;
						}

						// successfully found a scene to connect to
						CharacterService.SetSceneHandle(dbContext, accountName, loadedScene.SceneHandle);

						// tell the client to connect to the scene
						Server.Broadcast(connection, new WorldSceneConnectBroadcast()
						{
							address = sceneServer.Address,
							port = sceneServer.Port,
						});
					}
				}

				// check if we still have some players that are waiting for a scene
				if (connections.Count < 1)
				{
					WaitingConnections.Remove(sceneName);
				}
				// enqueue a new pending scene load request to the database, we need a new scene
				else if (!PendingSceneService.Exists(dbContext, worldServerSystem.ID, sceneName))
				{
					Debug.Log("World Scene System: Enqueing new PendingSceneLoadRequest: " + worldServerSystem.ID + ":" + sceneName);
					PendingSceneService.Enqueue(dbContext, worldServerSystem.ID, sceneName);
				}
			}
		}

		private void UpdateConnectionCount(NpgsqlDbContext dbContext)
		{
			if (dbContext == null ||
				ServerBehaviour.TryGet(out WorldServerSystem worldServerSystem))
			{
				return;
			}

			// get the scene data for each of our worlds scenes
			List<LoadedSceneEntity> sceneServerCount = LoadedSceneService.GetServerList(dbContext, worldServerSystem.ID);
			if (sceneServerCount != null)
			{
				// count the total
				ConnectionCount = WaitingConnections != null ? WaitingConnections.Count : 0;
				foreach (LoadedSceneEntity scene in sceneServerCount)
				{
					ConnectionCount += scene.CharacterCount;
				}
			}
		}

		private void Authenticator_OnClientAuthenticationResult(NetworkConnection conn, bool authenticated)
		{
			if (!authenticated)
				return;

			// if we are authenticated try and connect the client to a scene server
			TryConnectToSceneServer(conn);
		}

		/// <summary>
		/// Try to connect the client to a Scene Server.
		/// </summary>
		private void TryConnectToSceneServer(NetworkConnection conn)
		{
			if (!ServerBehaviour.TryGet(out WorldServerSystem worldServerSystem))
			{
				Debug.Log("World Scene System: World Server System could not be found.");
				conn.Kick(KickReason.UnexpectedProblem);
				return;
			}
			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			// get the scene for the selected character
			if (!AccountManager.GetAccountNameByConnection(conn, out string accountName) ||
				!CharacterService.TryGetSelectedSceneName(dbContext, accountName, out string sceneName))
			{
				Debug.Log("World Scene System: " + conn.ClientId + " failed to get selected scene or account name.");
				conn.Kick(KickReason.UnexpectedProblem);
				return;
			}

			List<LoadedSceneEntity> loadedScenes = LoadedSceneService.GetServerList(dbContext, worldServerSystem.ID, sceneName, MAX_CLIENTS_PER_INSTANCE);

			LoadedSceneEntity selectedScene = null;
			if (loadedScenes != null && loadedScenes.Count > 0)
			{
				foreach (LoadedSceneEntity sceneEntity in loadedScenes)
				{
					selectedScene = sceneEntity;
					break;// first scene? we could load balance here and distribute players evently across scenes
				}
			}
			// if we found a valid scene server
			if (selectedScene != null)
			{
				SceneServerEntity sceneServer = SceneServerService.GetServer(dbContext, selectedScene.SceneServerID);

				// successfully found a scene to connect to
				CharacterService.SetSceneHandle(dbContext, accountName, selectedScene.SceneHandle);

				// tell the character to reconnect to the scene server
				Server.Broadcast(conn, new WorldSceneConnectBroadcast()
				{
					address = sceneServer.Address,
					port = sceneServer.Port,
				});
			}
			else
			{
				// add the client to the wait queue, when a scene server loads the scene the client will be connected when it's ready
				WaitingConnectionsScenes[conn] = sceneName;
				if (!WaitingConnections.TryGetValue(sceneName, out HashSet<NetworkConnection> connections))
				{
					WaitingConnections.Add(sceneName, connections = new HashSet<NetworkConnection>());
				}
				if (!connections.Contains(conn))
				{
					connections.Add(conn);
				}

				if (!PendingSceneService.Exists(dbContext, worldServerSystem.ID, sceneName))
				{
					// enqueue the pending scene load to the database
					Debug.Log("World Scene System: Enqueing new PendingSceneLoadRequest: " + worldServerSystem.ID + ":" + sceneName);
					PendingSceneService.Enqueue(dbContext, worldServerSystem.ID, sceneName);
				}
			}
		}
	}
}