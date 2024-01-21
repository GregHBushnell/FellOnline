﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;
using FellOnline.Shared;

namespace FellOnline.Server.DatabaseServices
{
    public class FWorldServerService
    {
        /// <summary>
        /// Adds a new server to the server list. The Login server will fetch this list for new clients.
        /// </summary>
        public static WorldServerEntity Add(
            NpgsqlDbContext dbContext,
            string name,
            string address,
            ushort port,
            int characterCount,
            bool locked,
            out long id
        )
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address))
                throw new Exception("Name or address is invalid");
            
            if (dbContext.WorldServers
                .Any(server => EF.Functions.Like(server.Name, name.ToLower()))) 
                throw new Exception($"World with name \"{name}\" already exists");

            var server = new WorldServerEntity()
            {
                Name = name,
                LastPulse = DateTime.UtcNow,
                Address = address,
                Port = port,
                CharacterCount = characterCount,
                Locked = locked
            };
            dbContext.WorldServers.Add(server);
			dbContext.SaveChanges();

            id = server.ID;
			return server;
        }

        public static void Pulse(NpgsqlDbContext dbContext, long id, int characterCount)
        {
           if (id == 0)
			{
				return;
			}
			var worldServer = dbContext.WorldServers.FirstOrDefault(c => c.ID == id);
			if (worldServer == null) throw new Exception($"Couldn't find World Server with ID: {id}");
            
            worldServer.LastPulse = DateTime.UtcNow;
            worldServer.CharacterCount = characterCount;
             dbContext.SaveChanges();
		}
        
        public static void Delete(NpgsqlDbContext dbContext, long id) 
        {
            if (id == 0)
            {
                return;
            }
            var worldServer = dbContext.WorldServers.FirstOrDefault(c => c.ID == id);
			if (worldServer == null) throw new Exception($"Couldn't find World Server with ID: {id}");

            dbContext.WorldServers.Remove(worldServer);
            dbContext.SaveChanges();
        }

		public static WorldServerEntity GetServer(NpgsqlDbContext dbContext, long worldServerID)
		{
            if (worldServerID == 0)
			{
				return null;
			}
			var worldServer = dbContext.WorldServers.FirstOrDefault(c => c.ID == worldServerID);
			if (worldServer == null) throw new Exception($"Couldn't find World Server with ID: {worldServerID}");

            return worldServer;
		}

		public static List<WorldServerDetails> GetServerList(NpgsqlDbContext dbContext, float idleTimeout = 60.0f /*60 second timeout*/)
        {
            return dbContext.WorldServers.Where((s) => s.LastPulse.AddSeconds(idleTimeout) >= DateTime.UtcNow)
                .Select(server => new WorldServerDetails()
                {
                    Name = server.Name,
                    LastPulse = server.LastPulse,
                    Address = server.Address,
                    Port = server.Port,
                    CharacterCount = server.CharacterCount,
                    Locked = server.Locked,
                })
                .ToList();
        }
    }
}