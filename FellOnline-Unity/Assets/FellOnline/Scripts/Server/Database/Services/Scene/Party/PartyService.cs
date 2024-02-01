﻿using System.Collections.Generic;
using System.Linq;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;

namespace FellOnline.Server.DatabaseServices
{
	public class PartyService
	{
		public static bool Exists(NpgsqlDbContext dbContext, long partyID)
		{
			if (partyID == 0)
			{
				return false;
			}
			return dbContext.Parties.FirstOrDefault(a => a.ID == partyID) != null;
		}

		/// <summary>
		/// Saves a new Party to the database.
		/// </summary>
		public static bool TryCreate(NpgsqlDbContext dbContext, out PartyEntity party)
		{
			party = new PartyEntity()
			{
				Characters = new List<CharacterPartyEntity>(),
			};
			dbContext.Parties.Add(party);
			dbContext.SaveChanges();
			return true;
		}

		public static void Delete(NpgsqlDbContext dbContext, long partyID)
		{
			if (partyID == 0)
			{
				return;
			}
			var partyEntity = dbContext.Parties.FirstOrDefault(a => a.ID == partyID);
			if (partyEntity != null)
			{
				dbContext.Parties.Remove(partyEntity);
				dbContext.SaveChanges();
			}
		}

		/// <summary>
		/// Load a Party from the database.
		/// </summary>
		public static PartyEntity Load(NpgsqlDbContext dbContext, long partyID)
		{
			if (partyID == 0)
			{
				return null;
			}
			return dbContext.Parties.FirstOrDefault(a => a.ID == partyID);
		}
	}
}