﻿using System.Collections.Generic;
using System.Linq;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;
using FellOnline.Shared;

namespace FellOnline.Server.DatabaseServices
{
	public class FCharacterFriendService
	{
		/// <summary>
		/// Checks if the characters friends list is full.
		/// </summary>
		public static bool Full(NpgsqlDbContext dbContext, long characterID, int max)
		{
			if (characterID == 0)
			{
				return false;
			}
			var characterFriends = dbContext.CharacterFriends.Where(a => a.CharacterID == characterID);
			if (characterFriends != null && characterFriends.Count() <= max)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Save a characters friends to the database.
		/// </summary>
		public static void Save(NpgsqlDbContext dbContext, Character character)
		{
			if (character == null)
			{
				return;
			}

			var friends = dbContext.CharacterFriends.Where(c => c.CharacterID == character.ID.Value)
													.ToDictionary(k => k.FriendCharacterID);

			foreach (long friendID in character.FriendController.Friends)
			{
				if (!friends.ContainsKey(friendID))
				{
					dbContext.CharacterFriends.Add(new CharacterFriendEntity()
					{
						CharacterID = character.ID.Value,
						FriendCharacterID = friendID,
					});
				}
			}
			dbContext.SaveChanges();
		}

		/// <summary>
		/// Saves a CharacterFriendEntity to the database.
		/// </summary>
		public static void Save(NpgsqlDbContext dbContext, long characterID, long friendID)
		{
			if (friendID == 0)
			{
				return;
			}
			var characterFriendEntity = dbContext.CharacterFriends.FirstOrDefault(a => a.CharacterID == characterID && a.FriendCharacterID == friendID);
			if (characterFriendEntity == null)
			{
				characterFriendEntity = new CharacterFriendEntity()
				{
					CharacterID = characterID,
					FriendCharacterID = friendID,
				};
				dbContext.CharacterFriends.Add(characterFriendEntity);
				dbContext.SaveChanges();
			}
		}

		/// <summary>
		/// Removes a character from a friend list.
		/// </summary>
		public static bool Delete(NpgsqlDbContext dbContext, long characterID, long friendID)
		{
			
			if (friendID == 0)
			{
				return false;
			}
			var characterFriendEntity = dbContext.CharacterFriends.FirstOrDefault(a => a.CharacterID == characterID && a.FriendCharacterID == friendID);
			if (characterFriendEntity != null)
			{
				dbContext.CharacterFriends.Remove(characterFriendEntity);
				dbContext.SaveChanges();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Removes all characters from a friend list.
		/// </summary>
		public static void Delete(NpgsqlDbContext dbContext, long characterID, bool keepData = false)
		{
			if (characterID == 0)
			{
				return;
			}
			if (!keepData)
			{
				var characterFriends = dbContext.CharacterFriends.Where(a => a.CharacterID == characterID);
				if (characterFriends != null)
				{
					dbContext.CharacterFriends.RemoveRange(characterFriends);
					dbContext.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Load characters friends from the database.
		/// </summary>
		public static void Load(NpgsqlDbContext dbContext, Character character)
		{
			var friends = dbContext.CharacterFriends.Where(c => c.CharacterID == character.ID.Value);
			foreach (CharacterFriendEntity friend in friends)
			{
				character.FriendController.AddFriend(friend.FriendCharacterID);
			};
		}

		/// <summary>
		/// Load all CharacterFriendEntity from the database for a specific character.
		/// </summary>
		public static List<CharacterFriendEntity> Friends(NpgsqlDbContext dbContext, long characterID)
		{
			if (characterID == 0)
			{
				return null;
			}
			return dbContext.CharacterFriends.Where(a => a.CharacterID == characterID).ToList();
		}
	}
}