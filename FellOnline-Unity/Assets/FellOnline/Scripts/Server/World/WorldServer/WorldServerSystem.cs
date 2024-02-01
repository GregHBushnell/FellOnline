﻿using FishNet.Managing.Server;
using FishNet.Transporting;
using FellOnline.Server.DatabaseServices;
using UnityEngine;
using FellOnline.Shared;

namespace FellOnline.Server
{
	// World Manager handles the database heartbeat for Login Service
	public class WorldServerSystem : ServerBehaviour
	{
		private LocalConnectionState serverState;

		private long id;
		private bool locked = false;
		private float pulseRate = 5.0f;
		private float nextPulse = 0.0f;

		public long ID { get { return id; } }

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
			serverState = args.ConnectionState;
			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();

			if (args.ConnectionState == LocalConnectionState.Started)
			{
				if (Server.TryGetServerIPAddress(out ServerAddress server) &&
					ServerBehaviour.TryGet(out WorldSceneSystem worldSceneSystem))
				{
					int characterCount = worldSceneSystem.ConnectionCount;

					if (Server.Configuration.TryGetString("ServerName", out string name))
					{
						Debug.Log("World Server System: Adding World Server to Database: " + name + ":" + server.address + ":" + server.port);
						WorldServerService.Add(dbContext, name, server.address, server.port, characterCount, locked, out id);
					}
				}
			}
			else if (args.ConnectionState == LocalConnectionState.Stopped)
			{
				if (Server.Configuration.TryGetString("ServerName", out string name))
				{
					Debug.Log("World Server System: Removing World Server from Database: " + name);
					WorldServerService.Delete(dbContext, id);
				}
			}
		}

		void LateUpdate()
		{
			if (serverState == LocalConnectionState.Started &&
				ServerBehaviour.TryGet(out WorldSceneSystem worldSceneSystem))
			{
				if (nextPulse < 0)
				{
					nextPulse = pulseRate;

					// TODO: maybe this one should exist....how expensive will this be to run on update?
					using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
					//Debug.Log("World Server System: Pulse");
					int characterCount = worldSceneSystem.ConnectionCount;
					WorldServerService.Pulse(dbContext, id, characterCount);
				}
				nextPulse -= Time.deltaTime;
			}
		}

		private void OnApplicationQuit()
		{
			if (Server != null && Server.NpgsqlDbContextFactory != null &&
				serverState != LocalConnectionState.Stopped &&
				Server.Configuration.TryGetString("ServerName", out string name))
			{
				using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
				Debug.Log("World Server System: Removing World Server: " + name);
				WorldServerService.Delete(dbContext, id);
			}
		}
	}
}
