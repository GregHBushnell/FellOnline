﻿using UnityEngine;
#if UNITY_SERVER
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using FellOnline.Server;
using FellOnline.Server.DatabaseServices;
using FellOnline.Database.Npgsql.Entities;
#endif

namespace FellOnline.Shared
{
	public class FTeleporter : FInteractable
	{
		public Transform Target;

#if UNITY_SERVER
		private FSceneServerSystem sceneServerSystem;

		public override void OnStarting()
		{
			if (sceneServerSystem == null)
			{
				sceneServerSystem = FServerBehaviour.Get<FSceneServerSystem>();
			}
		}
#endif

		public override bool OnInteract(Character character)
		{
			if (!base.OnInteract(character))
			{
				return false;
			}

#if UNITY_SERVER
			if (character.IsTeleporting)
			{
				return false;
			}

			if (Target != null)
			{

				// move the character
				character.Motor.SetPositionAndRotation(Target.position, Target.rotation);
				return true;
			}

			if (sceneServerSystem == null)
			{
				Debug.Log("SceneServerSystem not found!");
				return false;
			}

			if (sceneServerSystem.WorldSceneDetailsCache == null)
			{
				Debug.Log("SceneServerSystem: World Scene Details Cache not found!");
				return false;
			}

			// cache the current scene name
			string playerScene = character.SceneName.Value;

			if (!sceneServerSystem.WorldSceneDetailsCache.Scenes.TryGetValue(playerScene, out FWorldSceneDetails details))
			{
				Debug.Log(playerScene + " not found!");
				return false;
			}

			// check if we are a scene teleporter
			if (!details.Teleporters.TryGetValue(gameObject.name, out FSceneTeleporterDetails teleporter))
			{
				Debug.Log("Teleporter: " + gameObject.name + " not found!");
				return false;
			}

			character.IsTeleporting = true;

			// should we prevent players from moving to a different scene if they are in combat?
			/*if (character.DamageController.Attackers.Count > 0)
			{
				return;
			}*/

			// make the character immortal for teleport
			if (character.DamageController != null)
			{
				character.DamageController.Immortal = true;
			}

			// update scene instance details
			if (sceneServerSystem.TryGetSceneInstanceDetails(character.WorldServerID,
															 playerScene,
															 character.SceneHandle,
															 out FSceneInstanceDetails instance))
			{
				--instance.CharacterCount;
			}

			character.SceneName.Value = teleporter.ToScene;
			character.Motor.SetPositionAndRotation(teleporter.ToPosition, teleporter.ToRotation);

			// save the character with new scene and position
			using var dbContext = sceneServerSystem.Server.NpgsqlDbContextFactory.CreateDbContext();
			FCharacterService.Save(dbContext, character, false);


			NetworkConnection conn = character.Owner;
			long worldServerId = character.WorldServerID;

			sceneServerSystem.ServerManager.Despawn(character.NetworkObject, DespawnType.Pool);

			WorldServerEntity worldServer = FWorldServerService.GetServer(dbContext, worldServerId);
			if (worldServer != null)
			{
				// tell the client to reconnect to the world server for automatic re-entry
				conn.Broadcast(new SceneWorldReconnectBroadcast()
				{
					address = worldServer.Address,
					port = worldServer.Port,
					sceneName = playerScene,
					teleporterName = gameObject.name,
				}, true, Channel.Reliable);
			}
			else
			{
				// world not found?
				conn.Kick(FishNet.Managing.Server.KickReason.UnexpectedProblem);
			}
#endif
			return true;
		}
	}
}