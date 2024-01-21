﻿using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Managing;
using FishNet.Object;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;
using FellOnline.Shared;
using UnityEngine;

namespace FellOnline.Server.DatabaseServices
{
	/// <summary>
	/// Handles all Database<->Server Character interactions.
	/// </summary>
	public class FCharacterService
	{
		public static int GetCount(NpgsqlDbContext dbContext, string account)
		{
			return dbContext.Characters.Where((c) => c.Account == account && !c.Deleted).Count();
		}

		public static bool ExistsAndOnline(NpgsqlDbContext dbContext, long id)
		{
			if (id == 0)
			{
				return false;
			}
			return dbContext.Characters.FirstOrDefault((c) => c.ID == id &&
															  c.Online) != null;
		}

		public static bool ExistsAndOnline(NpgsqlDbContext dbContext, string characterName)
		{
			return dbContext.Characters.FirstOrDefault((c) => c.NameLowercase == characterName.ToLower() &&
															  c.Online) != null;
		}

		public static bool Exists(NpgsqlDbContext dbContext, string account, string characterName)
		{
			return dbContext.Characters.FirstOrDefault((c) => c.Account == account &&
															  c.NameLowercase == characterName.ToLower()) != null;
		}

		public static CharacterEntity GetByID(NpgsqlDbContext dbContext, long id)
		{
			if (id == 0)
			{
				return null;
			}
			var character = dbContext.Characters.FirstOrDefault(c => c.ID == id);
			if (character == null)
			{
				//throw new Exception($"Couldn't find character with id {id}");
			}
			return character;
		}

		public static long GetIdByName(NpgsqlDbContext dbContext, string name)
		{
			var character = dbContext.Characters.FirstOrDefault(c => c.NameLowercase == name.ToLower());
			if (character == null)
			{
				return 0;
			}
			return character.ID;
		}

		public static string GetNameByID(NpgsqlDbContext dbContext, long id)
		{
			if (id == 0)
			{
				return "";
			}
			var character = dbContext.Characters.FirstOrDefault(c => c.ID == id);
			if (character == null)
			{
				return "";
			}
			return character.Name;
		}

		public static CharacterEntity GetByName(NpgsqlDbContext dbContext, string name)
		{
			var character = dbContext.Characters.FirstOrDefault(c => c.NameLowercase == name.ToLower());
			if (character == null)
			{
				// Log: $"Couldn't find character with name {name}"
			}
			return character;
		}

		public static List<FCharacterDetails> GetDetails(NpgsqlDbContext dbContext, string account)
		{
			return dbContext.Characters.Where(c => c.Account == account && !c.Deleted)
										.Select(c => new FCharacterDetails()
										{
											CharacterName = c.Name
										})
										.ToList();
		}

		/// <summary>
		/// Selects a character in the database. This is used for validation purposes.
		/// </summary>
		public static bool TrySetSelected(NpgsqlDbContext dbContext, string account, string characterName)
		{
			// get all characters for account
			var characters = dbContext.Characters.Where((c) => c.Account == account && !c.Deleted);

			// deselect all characters
			foreach (var characterEntity in characters)
			{
				characterEntity.Selected = false;
			}
			dbContext.SaveChanges();
			var selectedCharacter = characters.FirstOrDefault((c) => c.Account == account && !c.Deleted && c.NameLowercase == characterName.ToLower());
			if (selectedCharacter != null)
			{
				selectedCharacter.Selected = true;
				dbContext.SaveChanges();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Selects a character in the database. This is used for validation purposes.
		/// </summary>
		public static bool GetSelected(NpgsqlDbContext dbContext, string account)
		{
			return dbContext.Characters.Where((c) => c.Account == account && c.Selected && !c.Deleted) != null;
		}

		/// <summary>
		/// Returns true if we successfully get our selected characters scene for the connections account, otherwise returns false.
		/// </summary>
		public static bool TryGetSelectedSceneName(NpgsqlDbContext dbContext, string account, out string sceneName)
		{
			var character = dbContext.Characters.FirstOrDefault((c) => c.Account == account &&
																	   c.Selected &&
																	   !c.Deleted);

			if (character != null)
			{
				sceneName = character.SceneName;
				return true;
			}
			sceneName = "";
			return false;
		}

		/// <summary>
		/// Returns true if we successfully get our selected character for the connections account, otherwise returns false.
		/// </summary>
		public static bool TryGetSelectedDetails(NpgsqlDbContext dbContext, string account, out long characterID)
		{
			var character = dbContext.Characters.FirstOrDefault((c) => c.Account == account && c.Selected && !c.Deleted);
			if (character != null)
			{
				characterID = character.ID;
				return true;
			}
			characterID = 0;
			return false;
		}
		
		/// <summary>
		/// Returns true if we successfully set our selected character for the connections account, otherwise returns false.
		/// </summary>
		public static void SetOnline(NpgsqlDbContext dbContext, string account, string characterName)
		{
			var selectedCharacter = dbContext.Characters.FirstOrDefault((c) => c.Account == account &&
																			   c.NameLowercase == characterName.ToLower());
			if (selectedCharacter != null)
			{
				selectedCharacter.Online = true;
				dbContext.SaveChanges();
			}
		}

		/// <summary>
		/// Returns true if any of the accounts characters are currently online.
		/// </summary>
		public static bool TryGetOnline(NpgsqlDbContext dbContext, string account)
		{
			var characters = dbContext.Characters.Where((c) => c.Account == account &&
															   c.Online == true &&
															   !c.Deleted).ToList();
			return characters != null && characters.Count > 0;
		}

		/// <summary>
		/// Set the selected characters world server id for the connections account.
		/// </summary>
		public static void SetWorld(NpgsqlDbContext dbContext, string account, long worldServerID)
		{
			if (worldServerID == 0)
			{
				return;
			}
			// get all characters for account
			var character = dbContext.Characters.FirstOrDefault((c) => c.Account == account && c.Selected && !c.Deleted);
			if (character != null)
			{
				character.WorldServerID = worldServerID;
				dbContext.SaveChanges();
			}
		}

		/// <summary>
		/// Set the selected characters scene handle for the connections account.
		/// </summary>
		public static void SetSceneHandle(NpgsqlDbContext dbContext, string account, int sceneHandle)
		{
			// get all characters for account
			var character = dbContext.Characters.FirstOrDefault((c) => c.Account == account && c.Selected && !c.Deleted);
			if (character != null)
			{
				character.SceneHandle = sceneHandle;
				dbContext.SaveChanges();
			}
		}

		public static void Save(NpgsqlDbContext dbContext, List<Character> characters, bool online = true)
		{
			using var dbTransaction = dbContext.Database.BeginTransaction();

			if (characters == null ||
				characters.Count < 1)
			{
				return;
			}
			foreach (Character character in characters)
			{
				Save(dbContext, character, online);
			}

			dbTransaction.Commit();
		}

		/// <summary>
		/// Save a character to the database. Only Scene Servers should be saving characters. A character can only be in one scene at a time.
		/// </summary>
		public static void Save(NpgsqlDbContext dbContext, Character character, bool online = true, CharacterEntity existingCharacter = null)
		{
			if (character == null)
			{
				return;
			}
			if (existingCharacter == null)
			{
				existingCharacter = dbContext.Characters.FirstOrDefault((c) => c.NameLowercase == character.CharacterName.ToLower());

				// if it's still null, throw exception
				if (existingCharacter == null)
				{
					//throw new Exception($"Unable to fetch character with name {character.CharacterName}");
					return;
				}
			}

			// store these into vars so we don't have to access them a bunch of times
			var charPosition = character.Transform.position;
			var rotation = character.Transform.rotation;

			// copy over the new values into the existing entity
			existingCharacter.Name = character.CharacterName;
			existingCharacter.NameLowercase = character.CharacterName.ToLower();
			existingCharacter.Account = character.Account;
			existingCharacter.WorldServerID = character.WorldServerID;
			existingCharacter.AccessLevel = (byte)character.AccessLevel;
			existingCharacter.RaceID = character.RaceID.Value;
			existingCharacter.Currency = character.Currency.Value;
			existingCharacter.SceneHandle = character.SceneHandle;
			existingCharacter.SceneName = character.SceneName.Value;
			existingCharacter.X = charPosition.x;
			existingCharacter.Y = charPosition.y;
			existingCharacter.Z = charPosition.z;
			existingCharacter.RotX = rotation.x;
			existingCharacter.RotY = rotation.y;
			existingCharacter.RotZ = rotation.z;
			existingCharacter.RotW = rotation.w;
			existingCharacter.Online = online;
			existingCharacter.LastSaved = DateTime.UtcNow;

			FCharacterAttributeService.Save(dbContext, character);
			FCharacterAchievementService.Save(dbContext, character);
			FCharacterBuffService.Save(dbContext, character);

			CharacterAppearanceService.Save(dbContext, character);

			dbContext.SaveChanges();

			/*Debug.Log(character.CharacterName + " has been saved at Pos: " +
					  character.Transform.position.ToString() +
					  " Rot: " + rotation);*/
		}

		/// <summary>
		/// KeepData is automatically true... This means we don't actually delete anything. Deleted is simply set to true just incase we need to reinstate a character..
		/// </summary>
		public static void Delete(NpgsqlDbContext dbContext, string account, string characterName, bool keepData = true)
		{
			using var dbTransaction = dbContext.Database.BeginTransaction();

			if (dbTransaction == null)
			{
				return;
			}

			var character = dbContext.Characters.FirstOrDefault(c => c.Account == account &&
																	 c.NameLowercase == characterName.ToLower());

			if (character == null)
			{
				return;
			}
			// possible preserved data
			FCharacterAttributeService.Delete(dbContext, character.ID, keepData);
			FCharacterAchievementService.Delete(dbContext, character.ID, keepData);
			FCharacterBuffService.Delete(dbContext, character.ID, keepData);
			FCharacterFriendService.Delete(dbContext, character.ID, keepData);
			FCharacterInventoryService.Delete(dbContext, character.ID, keepData);
			FCharacterEquipmentService.Delete(dbContext, character.ID, keepData);
			CharacterBankService.Delete(dbContext, character.ID, keepData);
			FCharacterAbilityService.Delete(dbContext, character.ID, keepData);
			FCharacterKnownAbilityService.Delete(dbContext, character.ID, keepData);
			CharacterAppearanceService.Delete(dbContext, character.ID, keepData);

			// complete deletions
			FCharacterGuildService.Delete(dbContext, character.ID);
			FCharacterPartyService.Delete(dbContext, character.ID);


			if (keepData)
			{
				character.TimeDeleted = DateTime.UtcNow;
				character.Deleted = true;
				dbContext.SaveChanges();
			}
			else
			{

				dbContext.Characters.Remove(character);
				dbContext.SaveChanges();

			}
			dbTransaction.Commit();
		}

		/// <summary>
		/// Attempts to load a character from the database. The character is loaded to the last known position/rotation and set inactive.
		/// </summary>
		public static bool TryGet(NpgsqlDbContext dbContext, long characterID, NetworkManager networkManager, out Character character)
		{
			using var dbTransaction = dbContext.Database.BeginTransaction();

			var dbCharacter = dbContext.Characters.FirstOrDefault((c) => c.ID == characterID &&
																		 !c.Deleted);
			if (dbCharacter != null &&
				dbTransaction != null)
			{
				// find prefab
				NetworkObject prefab = networkManager.SpawnablePrefabs.GetObject(true, dbCharacter.RaceID);
				if (prefab != null)
				{
					// instantiate the character object
					NetworkObject nob = networkManager.GetPooledInstantiated(prefab, true);

					character = nob.GetComponent<Character>();
					if (character != null)
					{
						character.Motor.SetPositionAndRotationAndVelocity(new Vector3(dbCharacter.X, dbCharacter.Y, dbCharacter.Z),
																		  new Quaternion(dbCharacter.RotX, dbCharacter.RotY, dbCharacter.RotZ, dbCharacter.RotW),
																		  Vector3.zero);
						character.ID.Value = dbCharacter.ID;
						character.CharacterName = dbCharacter.Name;
						character.CharacterNameLower = dbCharacter.NameLowercase;
						character.Account = dbCharacter.Account;
						character.WorldServerID = dbCharacter.WorldServerID;
						character.AccessLevel = (AccessLevel)dbCharacter.AccessLevel;
						character.RaceID.Value = dbCharacter.RaceID;
						//character.HairID.Value = dbCharacter.HairID;
						character.Currency.Value = dbCharacter.Currency;
						character.RaceName.Value = prefab.name;
						character.SceneHandle = dbCharacter.SceneHandle;
						character.SceneName.Value = dbCharacter.SceneName;
						character.IsTeleporting = false;

						FCharacterAttributeService.Load(dbContext, character);
						FCharacterAchievementService.Load(dbContext, character);
						FCharacterBuffService.Load(dbContext, character);
						FCharacterGuildService.Load(dbContext, character);
						FCharacterPartyService.Load(dbContext, character);
						FCharacterFriendService.Load(dbContext, character);
						FCharacterInventoryService.Load(dbContext, character);
						FCharacterEquipmentService.Load(dbContext, character);
						CharacterBankService.Load(dbContext, character);
						FCharacterAbilityService.Load(dbContext, character);
						FCharacterKnownAbilityService.Load(dbContext, character);
						CharacterAppearanceService.Load(dbContext, character);


						/*Debug.Log(dbCharacter.Name + " has been loaded at Pos:" +
							  nob.transform.position.ToString() +
							  " Rot:" + nob.transform.rotation.ToString());*/

						dbTransaction.Commit();

						return true;
					}
				}
			}
			character = null;
			return false;
		}
	}
}