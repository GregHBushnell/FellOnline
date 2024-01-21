﻿using System.Collections.Generic;
using System.Linq;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;
using FellOnline.Shared;

namespace FellOnline.Server.DatabaseServices
{
	public class FCharacterPartyService
	{
		public static bool ExistsNotFull(NpgsqlDbContext dbContext, long partyID, int max)
		{
			if (partyID == 0)
			{
				return false;
			}
			var partyCharacters = dbContext.CharacterParties.Where(a => a.PartyID == partyID);
			if (partyCharacters != null && partyCharacters.Count() <= max)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Saves a CharacterPartyEntity to the database.
		/// </summary>
		public static void Save(NpgsqlDbContext dbContext, long characterID, long partyID, PartyRank rank, float healthPCT)
		{
			if (partyID == 0)
			{
				return;
			}
			var characterPartyEntity = dbContext.CharacterParties.FirstOrDefault(a => a.CharacterID == characterID);
			if (characterPartyEntity == null)
			{
				characterPartyEntity = new CharacterPartyEntity()
				{
					CharacterID = characterID,
					PartyID = partyID,
					Rank = (byte)rank,
					HealthPCT = healthPCT,
				};
				dbContext.CharacterParties.Add(characterPartyEntity);
			}
			else
			{
				characterPartyEntity.PartyID = partyID;
				characterPartyEntity.Rank = (byte)rank;
				characterPartyEntity.HealthPCT = healthPCT;
			}
			dbContext.SaveChanges();
		}

		/// <summary>
		/// Removes a character from their party.
		/// </summary>
		public static bool Delete(NpgsqlDbContext dbContext, PartyRank kickerRank, long partyID, long memberID)
		{
			if (partyID == 0)
			{
				return false;
			}
			var characterPartyEntity = dbContext.CharacterParties.FirstOrDefault(a => a.PartyID == partyID && a.CharacterID == memberID && a.Rank < (byte)kickerRank);
			if (characterPartyEntity != null)
			{
				dbContext.CharacterParties.Remove(characterPartyEntity);
				dbContext.SaveChanges();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Removes a character from their party.
		/// </summary>
		public static void Delete(NpgsqlDbContext dbContext, long memberID)
		{
			if (memberID == 0)
			{
				return;
			}
			var characterPartyEntity = dbContext.CharacterParties.FirstOrDefault(a => a.CharacterID == memberID);
			if (characterPartyEntity != null)
			{
				dbContext.CharacterParties.Remove(characterPartyEntity);
				dbContext.SaveChanges();
			}
		}

		/// <summary>
		/// Load a CharacterPartyEntity from the database.
		/// </summary>
		public static void Load(NpgsqlDbContext dbContext, Character character)
		{
			if (character != null &&
				character.PartyController != null)
			{
				var characterPartyEntity = dbContext.CharacterParties.FirstOrDefault(a => a.CharacterID == character.ID.Value);
				if (characterPartyEntity != null)
				{
					character.PartyController.ID = characterPartyEntity.PartyID;
					character.PartyController.Rank = (PartyRank)characterPartyEntity.Rank;
				}
			}
		}

		public static List<CharacterPartyEntity> Members(NpgsqlDbContext dbContext, long partyID)
		{
			if (partyID == 0)
			{
				return null;
			}
			return dbContext.CharacterParties.Where(a => a.PartyID == partyID).ToList();
		}
	}
}