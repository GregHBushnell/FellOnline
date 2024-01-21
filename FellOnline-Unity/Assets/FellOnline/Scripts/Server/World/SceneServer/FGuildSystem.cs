﻿﻿using FishNet.Connection;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using FellOnline.Server.DatabaseServices;
using FellOnline.Shared;
using FellOnline.Database.Npgsql.Entities;

namespace FellOnline.Server
{
	/// <summary>
	/// Server guild system.
	/// </summary>
	public class FGuildSystem : FServerBehaviour
	{
		private LocalConnectionState serverState;
		private DateTime lastFetchTime = DateTime.UtcNow;
		private long lastPosition = 0;
		private float nextPump = 0.0f;

		public int MaxGuildSize = 100;
		public int MaxGuildNameLength = 64;
		[Tooltip("The server guild update pump rate limit in seconds.")]
		public float UpdatePumpRate = 1.0f;
		public int UpdateFetchCount = 100;

		// clientID / guildID
		private readonly Dictionary<long, long> pendingInvitations = new Dictionary<long, long>();

		private Dictionary<string, ChatCommand> guildChatCommands;
		public bool OnGuildInvite(Character sender, ChatBroadcast msg)
		{
			string characterName = msg.text.Trim().ToLower();
			if (Server.CharacterSystem.CharactersByLowerCaseName.TryGetValue(characterName, out Character character))
			{
				OnServerGuildInviteBroadcastReceived(sender.Owner, new GuildInviteBroadcast()
				{
					inviterCharacterID = sender.ID.Value,
					targetCharacterID = character.ID.Value,
				}, Channel.Reliable);
				return true;
			}
			return false;
		}

		public override void InitializeOnce()
		{
			if (ServerManager != null &&
				Server.CharacterSystem != null)
			{
				guildChatCommands = new Dictionary<string, ChatCommand>()
				{
					{ "/gi", OnGuildInvite },
					{ "/ginvite", OnGuildInvite },
				};
				FChatHelper.AddDirectCommands(guildChatCommands);

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
			if (serverState == LocalConnectionState.Started)
			{
				ServerManager.RegisterBroadcast<GuildCreateBroadcast>(OnServerGuildCreateBroadcastReceived, true);
				ServerManager.RegisterBroadcast<GuildInviteBroadcast>(OnServerGuildInviteBroadcastReceived, true);
				ServerManager.RegisterBroadcast<GuildAcceptInviteBroadcast>(OnServerGuildAcceptInviteBroadcastReceived, true);
				ServerManager.RegisterBroadcast<GuildDeclineInviteBroadcast>(OnServerGuildDeclineInviteBroadcastReceived, true);
				ServerManager.RegisterBroadcast<GuildLeaveBroadcast>(OnServerGuildLeaveBroadcastReceived, true);
				ServerManager.RegisterBroadcast<GuildRemoveBroadcast>(OnServerGuildRemoveBroadcastReceived, true);

				// remove the characters pending guild invite request on disconnect
				if (Server.CharacterSystem != null)
				{
					Server.CharacterSystem.OnDisconnect += RemovePending;
				}
			}
			else if (serverState == LocalConnectionState.Stopped)
			{
				ServerManager.UnregisterBroadcast<GuildCreateBroadcast>(OnServerGuildCreateBroadcastReceived);
				ServerManager.UnregisterBroadcast<GuildInviteBroadcast>(OnServerGuildInviteBroadcastReceived);
				ServerManager.UnregisterBroadcast<GuildAcceptInviteBroadcast>(OnServerGuildAcceptInviteBroadcastReceived);
				ServerManager.UnregisterBroadcast<GuildDeclineInviteBroadcast>(OnServerGuildDeclineInviteBroadcastReceived);
				ServerManager.UnregisterBroadcast<GuildLeaveBroadcast>(OnServerGuildLeaveBroadcastReceived);
				ServerManager.UnregisterBroadcast<GuildRemoveBroadcast>(OnServerGuildRemoveBroadcastReceived);

				// remove the characters pending guild invite request on disconnect
				if (Server.CharacterSystem != null)
				{
					Server.CharacterSystem.OnDisconnect -= RemovePending;
				}
			}
		}

		void LateUpdate()
		{
			if (serverState == LocalConnectionState.Started)
			{
				if (nextPump < 0)
				{
					nextPump = UpdatePumpRate;

					List<GuildUpdateEntity> updates = FetchGuildUpdates();
					ProcessGuildUpdates(updates);

				}
				nextPump -= Time.deltaTime;
			}
		}

		private List<GuildUpdateEntity> FetchGuildUpdates()
		{
			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();

			// fetch guild updates from the database
			List<GuildUpdateEntity> updates = FGuildUpdateService.Fetch(dbContext, lastFetchTime, lastPosition, UpdateFetchCount);
			if (updates != null && updates.Count > 0)
			{
				GuildUpdateEntity latest = updates[updates.Count - 1];
				if (latest != null)
				{
					lastFetchTime = latest.TimeCreated;
					lastPosition = latest.ID;
				}
			}
			return updates;
		}

		// process updates from the database
		private void ProcessGuildUpdates(List<GuildUpdateEntity> updates)
		{
			if (Server == null || Server.NpgsqlDbContextFactory == null || updates == null || updates.Count < 1)
			{
				return;
			}

			// guilds that have previously been updated, we do this so we aren't updating guilds multiple times
			HashSet<long> updatedGuilds = new HashSet<long>();

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			foreach (GuildUpdateEntity update in updates)
			{
				// check if we have already updated this guild
				if (updatedGuilds.Contains(update.GuildID))
				{
					continue;
				}
				// otherwise add the guild to our list and continue with the update
				updatedGuilds.Add(update.GuildID);

				// get the current guild members from the database
				List<CharacterGuildEntity> dbMembers = FCharacterGuildService.Members(dbContext, update.GuildID);

				var addBroadcasts = dbMembers.Select(x => new GuildAddBroadcast()
				{
					guildID = x.GuildID,
					characterID = x.CharacterID,
					rank = (GuildRank)x.Rank,
					location = x.Location,
				}).ToList();

				GuildAddMultipleBroadcast guildAddBroadcast = new GuildAddMultipleBroadcast()
				{
					members = addBroadcasts,
				};

				// tell all of the local guild members to update their guild member lists
				foreach (CharacterGuildEntity entity in dbMembers)
				{
					if (Server.CharacterSystem.CharactersByID.TryGetValue(entity.CharacterID, out Character character))
					{
						if (character.GuildController.ID.Value < 1)
						{
							continue;
						}
						// update server rank in the case of a membership rank change
						character.GuildController.Rank = (GuildRank)entity.Rank;
						character.Owner.Broadcast(guildAddBroadcast, true, Channel.Reliable);
					}
				}
			}
		}

		public void RemovePending(NetworkConnection conn, Character character)
		{
			if (character != null)
			{
				pendingInvitations.Remove(character.ID.Value);
			}
		}

		public void OnServerGuildCreateBroadcastReceived(NetworkConnection conn, GuildCreateBroadcast msg, Channel channel)
		{
			if (conn.FirstObject == null)
			{
				return;
			}
			if (Server.NpgsqlDbContextFactory == null)
			{
				return;
			}

			FGuildController guildController = conn.FirstObject.GetComponent<FGuildController>();
			if (guildController == null || guildController.ID.Value > 0)
			{
				// already in a guild
				return;
			}

			// remove white space
			msg.guildName = msg.guildName.Trim();

			if (!Constants.Authentication.IsAllowedGuildName(msg.guildName))
			{
				// we should tell the player the guild name is not valid TODO
				return;
			}

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			if (FGuildService.Exists(dbContext, msg.guildName))
			{
				// guild name is taken
				return;
			}
			if (FGuildService.TryCreate(dbContext, msg.guildName, out GuildEntity newGuild))
			{
				guildController.ID.Value = newGuild.ID;
				guildController.Rank = GuildRank.Leader;
				FCharacterGuildService.Save(dbContext, guildController.Character);

				// tell the character we made their guild successfully
				conn.Broadcast(new GuildAddBroadcast()
				{
					guildID = guildController.ID.Value,
					characterID = guildController.Character.ID.Value,
					rank = guildController.Rank,
					location = guildController.gameObject.scene.name,
				}, true, Channel.Reliable);
			}
		}

		public void OnServerGuildInviteBroadcastReceived(NetworkConnection conn, GuildInviteBroadcast msg, Channel channel)
		{
			if (Server.NpgsqlDbContextFactory == null)
			{
				return;
			}
			if (conn.FirstObject == null)
			{
				return;
			}
			FGuildController inviter = conn.FirstObject.GetComponent<FGuildController>();
			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();

			// validate guild leader or officer is inviting
			if (inviter == null ||
				inviter.ID.Value < 1 ||
				inviter.Character.ID.Value == msg.targetCharacterID || 
				!(inviter.Rank == GuildRank.Leader | inviter.Rank == GuildRank.Officer) ||
				!FCharacterGuildService.ExistsNotFull(dbContext, inviter.ID.Value, MaxGuildSize))
			{
				return;
			}

			// if the target doesn't already have a pending invite
			if (!pendingInvitations.ContainsKey(msg.targetCharacterID) &&
				Server.CharacterSystem.CharactersByID.TryGetValue(msg.targetCharacterID, out Character targetCharacter))
			{
				FGuildController targetGuildController = targetCharacter.GetComponent<FGuildController>();

				// validate target
				if (targetGuildController == null || targetGuildController.ID.Value > 0)
				{
					// we should tell the inviter the target is already in a guild
					return;
				}

				// add to our list of pending invitations... used for validation when accepting/declining a guild invite
				pendingInvitations.Add(targetCharacter.ID.Value, inviter.ID.Value);
				targetCharacter.Owner.Broadcast(new GuildInviteBroadcast()
				{
					inviterCharacterID = inviter.Character.ID.Value,
					targetCharacterID = targetCharacter.ID.Value
				}, true, Channel.Reliable);
			}
		}

		public void OnServerGuildAcceptInviteBroadcastReceived(NetworkConnection conn, GuildAcceptInviteBroadcast msg, Channel channel)
		{
			if (conn.FirstObject == null)
			{
				return;
			}
			FGuildController guildController = conn.FirstObject.GetComponent<FGuildController>();

			// validate character
			if (guildController == null || guildController.ID.Value > 0)
			{
				return;
			}

			// validate guild invite
			if (pendingInvitations.TryGetValue(guildController.Character.ID.Value, out long pendingGuildID))
			{
				pendingInvitations.Remove(guildController.Character.ID.Value);

				if (Server == null || Server.NpgsqlDbContextFactory == null)
				{
					return;
				}
				using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
				List<CharacterGuildEntity> members = FCharacterGuildService.Members(dbContext, pendingGuildID);
				if (members != null &&
					members.Count < MaxGuildSize)
				{
					guildController.ID.Value = pendingGuildID;
					guildController.Rank = GuildRank.Member;
					
					FCharacterGuildService.Save(dbContext, guildController.Character);
					// tell the other servers to update their guild lists
					FGuildUpdateService.Save(dbContext, pendingGuildID);

					// tell the new member they joined immediately, other clients will catch up with the GuildUpdate pass
					conn.Broadcast(new GuildAddBroadcast()
					{
						guildID = guildController.ID.Value,
						characterID = guildController.Character.ID.Value,
						rank = GuildRank.Member,
						location = guildController.gameObject.scene.name,
					}, true, Channel.Reliable);
				}
			}
		}

		public void OnServerGuildDeclineInviteBroadcastReceived(NetworkConnection conn, GuildDeclineInviteBroadcast msg, Channel channel)
		{
			Character character = conn.FirstObject.GetComponent<Character>();
			if (character != null)
			{
				pendingInvitations.Remove(character.ID.Value);
			}
		}

		public void OnServerGuildLeaveBroadcastReceived(NetworkConnection conn, GuildLeaveBroadcast msg, Channel channel)
		{
			if (Server.NpgsqlDbContextFactory == null)
			{
				return;
			}
			if (conn.FirstObject == null)
			{
				return;
			}
			FGuildController guildController = conn.FirstObject.GetComponent<FGuildController>();

			// validate character
			if (guildController == null || guildController.ID.Value < 1)
			{
				// not in a guild..
				return;
			}

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();

			// validate guild
			List<CharacterGuildEntity> members = FCharacterGuildService.Members(dbContext, guildController.ID.Value);
			if (members != null &&
				members.Count > 0)
			{
				int remainingCount = members.Count - 1;

				List<CharacterGuildEntity> remainingMembers = new List<CharacterGuildEntity>();

				// are there any other members in the guild? if so we transfer leadership to officers first and then members
				if (guildController.Rank == GuildRank.Leader && remainingCount > 0)
				{
					List<CharacterGuildEntity> officers = new List<CharacterGuildEntity>();

					foreach (CharacterGuildEntity member in members)
					{
						if (member.CharacterID == guildController.Character.ID.Value)
						{
							continue;
						}

						if (member.Rank == (byte)GuildRank.Officer)
						{
							officers.Add(member);
						}
						remainingMembers.Add(member);
					}

					CharacterGuildEntity newLeader = null;
					if (officers.Count > 0)
					{
						// pick a random officer
						newLeader = officers[UnityEngine.Random.Range(0, officers.Count)];
					}
					else if (remainingMembers.Count > 0)
					{
						// pick a random member
						newLeader = remainingMembers[UnityEngine.Random.Range(0, remainingMembers.Count)];
					}

					// update the guild leader status in the database
					if (newLeader != null)
					{
						FCharacterGuildService.Save(dbContext, newLeader.CharacterID, newLeader.GuildID, GuildRank.Leader, newLeader.Location);
					}
				}

				long guildID = guildController.ID.Value;

				guildController.ID.Value = 0;
				guildController.Rank = GuildRank.None;

				// tell character that they left the guild immediately, other clients will catch up with the GuildUpdate pass
				conn.Broadcast(new GuildLeaveBroadcast(), true, Channel.Reliable);

				// remove the guild member
				FCharacterGuildService.Delete(dbContext, guildController.Character.ID.Value);

				if (remainingCount < 1)
				{
					// delete the guild
					FGuildService.Delete(dbContext, guildID);
					FGuildUpdateService.Delete(dbContext, guildID);
				}
				else
				{
					// tell the other servers to update their guild lists
					FGuildUpdateService.Save(dbContext, guildID);
				}
			}
		}

		public void OnServerGuildRemoveBroadcastReceived(NetworkConnection conn, GuildRemoveBroadcast msg, Channel channel)
		{
			if (Server.NpgsqlDbContextFactory == null)
			{
				return;
			}
			if (conn.FirstObject == null)
			{
				return;
			}
			FGuildController guildController = conn.FirstObject.GetComponent<FGuildController>();

			// validate character
			if (guildController == null ||
				guildController.ID.Value < 1 ||
				guildController.Rank != GuildRank.Leader ||
				guildController.Rank != GuildRank.Officer) 
			{
				return;
			}

			if (msg.members == null || msg.members.Count < 1)
			{
				return;
			}

			// first index only
			long memberID = msg.members[0];

			// we can't kick ourself
			if (memberID == guildController.Character.ID.Value)
			{
				return;
			}

			// remove the character from the guild in the database
			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			bool result = FCharacterGuildService.Delete(dbContext, guildController.Rank, guildController.ID.Value, memberID);
			if (result)
			{
				// tell the other servers to update their guild lists
				FGuildUpdateService.Save(dbContext, guildController.ID.Value);
			}
		}
	}
}