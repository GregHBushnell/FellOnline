﻿using System;
using System.Collections.Generic;
using System.Linq;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;

namespace FellOnline.Server.DatabaseServices
{
	public class GuildUpdateService
	{
		public static void Save(NpgsqlDbContext dbContext, long guildID)
		{
			dbContext.GuildUpdates.Add(new GuildUpdateEntity()
			{
				GuildID = guildID,
				TimeCreated = DateTime.UtcNow,
			});
			dbContext.SaveChanges();
		}

		public static void Delete(NpgsqlDbContext dbContext, long guildID)
		{
			var guildEntity = dbContext.GuildUpdates.Where(a => a.GuildID == guildID);
			if (guildEntity != null)
			{
				dbContext.GuildUpdates.RemoveRange(guildEntity);
				dbContext.SaveChanges();
			}
		}

		public static List<GuildUpdateEntity> Fetch(NpgsqlDbContext dbContext, DateTime lastFetch, long lastPosition, int amount)
		{
			var nextPage = dbContext.GuildUpdates
				.OrderBy(b => b.TimeCreated)
				.ThenBy(b => b.ID)
				.Where(b => b.TimeCreated >= lastFetch &&
							b.ID > lastPosition)
				.Take(amount)
				.ToList();
			return nextPage;
		}
	}
}