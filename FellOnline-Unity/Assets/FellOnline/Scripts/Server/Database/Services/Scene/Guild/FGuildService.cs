﻿using System.Collections.Generic;
using System.Linq;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;

namespace FellOnline.Server.DatabaseServices
{
	public class FGuildService
	{
		public static bool Exists(NpgsqlDbContext dbContext, string name)
		{
			return dbContext.Guilds.FirstOrDefault(a => a.Name.ToUpper() == name.ToUpper()) != null;
		}

		public static string GetNameByID(NpgsqlDbContext dbContext, long guildID)
		{
			if (guildID == 0)
			{
				return "";
			}
			var guild = dbContext.Guilds.FirstOrDefault(a => a.ID == guildID);
			if (guild == null)
			{
				return "";
			}
			return guild.Name;
		}

		/// <summary>
		/// Saves a Guild to the database if it doesn't exist.
		/// </summary>
		public static bool TryCreate(NpgsqlDbContext dbContext, string name, out GuildEntity guild)
		{
			guild = dbContext.Guilds.FirstOrDefault(a => a.Name.ToUpper() == name.ToUpper());
			if (guild == null)
			{
				guild = new GuildEntity()
				{
					Name = name,
					Notice = "",
					Characters = new List<CharacterGuildEntity>(),
				};
				dbContext.Guilds.Add(guild);
				dbContext.SaveChanges();
				return true;
			}
			return false;
		}

		public static void Delete(NpgsqlDbContext dbContext, long guildID)
		{
			if (guildID == 0)
			{
				return;
			}
			var guildEntity = dbContext.Guilds.FirstOrDefault(a => a.ID == guildID);
			if (guildEntity != null)
			{
				dbContext.Guilds.Remove(guildEntity);
				dbContext.SaveChanges();
			}
		}

		/// <summary>
		/// Load a Guild from the database.
		/// </summary>
		public static GuildEntity Load(NpgsqlDbContext dbContext, string name)
		{
			return dbContext.Guilds.FirstOrDefault(a => a.Name.ToUpper() == name.ToUpper());
		}

		/// <summary>
		/// Load a Guild from the database.
		/// </summary>
		public static GuildEntity Load(NpgsqlDbContext dbContext, long id)
		{
			if (id == 0)
			{
				return null;
			}
			return dbContext.Guilds.FirstOrDefault(a => a.ID == id);
		}
	}
}