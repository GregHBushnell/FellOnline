﻿using FishNet.Transporting;
using FishNet.Managing.Scened;
using UnityEngine;
using System.Collections.Generic;
using FishNet.Connection;
using FellOnline.Server.DatabaseServices;
using FellOnline.Shared;
using FellOnline.Database.Npgsql.Entities;

namespace FellOnline.Server
{
	// Scene Manager handles the node services and heartbeat to World Server
	public class FSceneServerSystem : FServerBehaviour
	{
		public SceneManager SceneManager;

		private LocalConnectionState serverState;

		public FWorldSceneDetailsCache WorldSceneDetailsCache;

		private long id;
		private bool locked = false;
		private float pulseRate = 5.0f;
		private float nextPulse = 0.0f;

		public long ID { get { return id; } }

		// <worldID, <sceneName, <sceneHandle, details>>>
		public Dictionary<long, Dictionary<string, Dictionary<int, FSceneInstanceDetails>>> worldScenes = new Dictionary<long, Dictionary<string, Dictionary<int, FSceneInstanceDetails>>>();

		public override void InitializeOnce()
		{
			if (ServerManager != null &&
				SceneManager != null)
			{
				SceneManager.OnLoadEnd += SceneManager_OnLoadEnd;
				ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
			}
			else
			{
				enabled = false;
			}
		}

		private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs args)
		{
			serverState = args.ConnectionState;
			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();

			if (args.ConnectionState == LocalConnectionState.Started)
			{
				if (Server.TryGetServerIPAddress(out ServerAddress server))
				{
					int characterCount = Server.CharacterSystem.ConnectionCharacters.Count;

					if (Server.Configuration.TryGetString("ServerName", out string name))
					{
						FSceneServerService.Add(dbContext, server.address, server.port, characterCount, locked, out id);
						Debug.Log("Scene Server System: Added Scene Server to Database: [" + id + "] " + name + ":" + server.address + ":" + server.port);
					}
				}
			}
			else if (args.ConnectionState == LocalConnectionState.Stopped)
			{
				if (Server.Configuration.TryGetString("ServerName", out string name))
				{
					Debug.Log("Scene Server System: Removing Scene Server: " + id);
					FSceneServerService.Delete(dbContext, id);
					FLoadedSceneService.Delete(dbContext, id);
				}
			}
		}

		void LateUpdate()
		{
			if (serverState == LocalConnectionState.Started)
			{
				if (nextPulse < 0)
				{
					nextPulse = pulseRate;

					// TODO: maybe this one should exist....how expensive will this be to run on update?
					using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
					//Debug.Log("Scene Server System: Pulse");
					int characterCount = Server.CharacterSystem.ConnectionCharacters.Count;
					FSceneServerService.Pulse(dbContext, id, characterCount);

					// process loaded scene pulse update
					if (worldScenes != null)
					{
						foreach (Dictionary<string, Dictionary<int, FSceneInstanceDetails>> sceneGroup in worldScenes.Values)
						{
							foreach (Dictionary<int, FSceneInstanceDetails> scene in sceneGroup.Values)
							{
								foreach (KeyValuePair<int, FSceneInstanceDetails> sceneDetails in scene)
								{
									//Debug.Log("Scene Server System: " + sceneDetails.Value.Name + ":" + sceneDetails.Value.WorldServerID + ":" + sceneDetails.Value.Handle + " Pulse");
									FLoadedSceneService.Pulse(dbContext, sceneDetails.Key, sceneDetails.Value.CharacterCount);
								}
							}
						}
					}

					// process pending scenes
					PendingSceneEntity pending = FPendingSceneService.Dequeue(dbContext);
					if (pending != null)
					{
						Debug.Log("Scene Server System: Dequeued Pending Scene Load request World:" + pending.WorldServerID + " Scene:" + pending.SceneName);
						ProcessSceneLoadRequest(pending.WorldServerID, pending.SceneName);
					}
				}
				nextPulse -= Time.deltaTime;
			}
		}

		private void OnApplicationQuit()
		{
			if (Server != null && Server.NpgsqlDbContextFactory != null &&
				serverState != LocalConnectionState.Stopped)
			{
				using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
				Debug.Log("Scene Server System: Removing Scene Server: " + id);
				FSceneServerService.Delete(dbContext, id);
				FLoadedSceneService.Delete(dbContext, id);
			}
		}

		/// <summary>
		/// Process a single scene load request from the database.
		/// </summary>
		private void ProcessSceneLoadRequest(long worldServerID, string sceneName)
		{
			if (WorldSceneDetailsCache == null ||
				!WorldSceneDetailsCache.Scenes.Contains(sceneName))
			{
				Debug.Log("Scene Server System: Scene is missing from the cache. Unable to load the scene.");
				// TODO kick players waiting for this scene otherwise they get stuck
				return;
			}

			// pre cache the scene on the server
			SceneLookupData lookupData = new SceneLookupData(sceneName);
			SceneLoadData sld = new SceneLoadData(lookupData)
			{
				ReplaceScenes = ReplaceOption.None,
				Options = new LoadOptions
				{
					AllowStacking = true,
					AutomaticallyUnload = false,
					LocalPhysics = UnityEngine.SceneManagement.LocalPhysicsMode.Physics3D,
				},
				Params = new LoadParams()
				{
					ServerParams = new object[]
					{
						worldServerID
					}
				},
			};
			SceneManager.LoadConnectionScenes(sld);
		}

		// we only track scene handles here for scene stacking, the SceneManager has the real Scene reference
		private void SceneManager_OnLoadEnd(SceneLoadEndEventArgs args)
		{
			// we only operate on newly loaded scenes here
			if (args.LoadedScenes == null ||
				args.LoadedScenes.Length < 1)
			{
				return;
			}

			UnityEngine.SceneManagement.Scene scene = args.LoadedScenes[0];

			// note there should only ever be one world id. we load one at a time
			long worldServerID = -1;
			if (args.QueueData.SceneLoadData.Params.ServerParams != null &&
				args.QueueData.SceneLoadData.Params.ServerParams.Length > 0)
			{
				worldServerID = (long)args.QueueData.SceneLoadData.Params.ServerParams[0];
			}
			// if the world scene is < 0 it is a local scene
			if (worldServerID < 0)
			{
				return;
			}

			// configure the mapping for this specific world scene
			if (!worldScenes.TryGetValue(worldServerID, out Dictionary<string, Dictionary<int, FSceneInstanceDetails>> scenes))
			{
				worldScenes.Add(worldServerID, scenes = new Dictionary<string, Dictionary<int, FSceneInstanceDetails>>());
			}
			if (!scenes.TryGetValue(scene.name, out Dictionary<int, FSceneInstanceDetails> handles))
			{
				scenes.Add(scene.name, handles = new Dictionary<int, FSceneInstanceDetails>());
			}
			if (!handles.ContainsKey(scene.handle))
			{
				// configure the scene physics ticker
				GameObject gob = new GameObject("PhysicsTicker");
				FPhysicsTicker physicsTicker = gob.AddComponent<FPhysicsTicker>();
				physicsTicker.InitializeOnce(scene.GetPhysicsScene(), Server.NetworkManager.TimeManager);
				UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gob, scene);

				// cache the newly loaded scene
				handles.Add(scene.handle, new FSceneInstanceDetails()
				{
					WorldServerID = worldServerID,
					Name = scene.name,
					Handle = scene.handle,
					CharacterCount = 0,
				});

				// save the loaded scene information to the database
				using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
				Debug.Log("Scene Server System: Loaded Scene " + scene.name + ":" + scene.handle);
				FLoadedSceneService.Add(dbContext, id, worldServerID, scene.name, scene.handle);
			}
			else
			{
				throw new UnityException("Scene Server System: Duplicate scene handles!!");
			}
		}

		public bool TryGetSceneInstanceDetails(long worldServerID, string sceneName, int sceneHandle, out FSceneInstanceDetails instanceDetails)
		{
			instanceDetails = default;

			if (worldScenes != null &&
				worldScenes.TryGetValue(worldServerID, out Dictionary<string, Dictionary<int, FSceneInstanceDetails>> scenes))
			{
				if (scenes != null &&
					scenes.TryGetValue(sceneName, out Dictionary<int, FSceneInstanceDetails> instances))
				{
					if (instances != null &&
						instances.TryGetValue(sceneHandle, out instanceDetails))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool TryLoadSceneForConnection(NetworkConnection conn, FSceneInstanceDetails instance)
		{
			UnityEngine.SceneManagement.Scene scene = SceneManager.GetScene(instance.Handle);
			if (scene != null && scene.IsValid() && scene.isLoaded)
			{
				SceneLookupData lookupData = new SceneLookupData(instance.Handle);
				SceneLoadData sld = new SceneLoadData(lookupData)
				{
					ReplaceScenes = ReplaceOption.None,
					Options = new LoadOptions
					{
						AllowStacking = false,
						AutomaticallyUnload = false,
					},
					PreferredActiveScene = new PreferredScene(lookupData),
				};
				SceneManager.LoadConnectionScenes(conn, sld);
				return true;
			}
			return false;
		}

		public void AssignPhysicsScene(Character character)
		{
			UnityEngine.SceneManagement.Scene scene = SceneManager.GetScene(character.SceneHandle);
			if (scene != null && scene.IsValid() && scene.isLoaded)
			{
				UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(character.gameObject, scene);
				character.Motor.SetPhysicsScene(scene.GetPhysicsScene());
			}
		}
	}
}