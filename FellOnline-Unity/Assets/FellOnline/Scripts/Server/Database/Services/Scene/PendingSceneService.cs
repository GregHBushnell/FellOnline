﻿using System.Linq;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;

namespace FellOnline.Server.DatabaseServices
{
	public class PendingSceneService
	{
		public static bool Exists(NpgsqlDbContext dbContext, long worldServerID, string sceneName)
		{
			if (worldServerID == 0)
			{
				return false;
			}
			return dbContext.PendingScenes.FirstOrDefault(c => c.WorldServerID == worldServerID && c.SceneName == sceneName) != null;
		}

		public static void Enqueue(NpgsqlDbContext dbContext, long worldServerID, string sceneName)
		{
			if (worldServerID == 0)
			{
				return;
			}
			var entity = dbContext.PendingScenes.FirstOrDefault(c => c.WorldServerID == worldServerID && c.SceneName == sceneName);
			if (entity == null)
			{
				entity = new PendingSceneEntity()
				{
					WorldServerID = worldServerID,
					SceneName = sceneName,
				};
				dbContext.PendingScenes.Add(entity);
				dbContext.SaveChanges();
			}
		}

		public static void Delete(NpgsqlDbContext dbContext, long worldServerID)
		{
			if (worldServerID == 0)
			{
				return;
			}
			var pending = dbContext.PendingScenes.Where(c => c.WorldServerID == worldServerID);
			if (pending != null)
			{
				dbContext.PendingScenes.RemoveRange(pending);
				dbContext.SaveChanges();
			}
		}

		public static PendingSceneEntity Dequeue(NpgsqlDbContext dbContext)
		{
			PendingSceneEntity entity = dbContext.PendingScenes.FirstOrDefault();
			if (entity != null)
			{
				dbContext.PendingScenes.Remove(entity);
				dbContext.SaveChanges();
			}
			return entity;
		}
	}
}