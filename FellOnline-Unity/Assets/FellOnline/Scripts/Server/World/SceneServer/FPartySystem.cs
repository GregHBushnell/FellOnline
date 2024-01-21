﻿﻿using FishNet.Connection;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FellOnline.Server.DatabaseServices;
using FellOnline.Shared;
using FellOnline.Database.Npgsql.Entities;


namespace FellOnline.Server
{
	/// <summary>
	/// Server party system.
	/// </summary>
	public class FPartySystem : FServerBehaviour
	{
		public FCharacterAttributeTemplate HealthTemplate;
		public int MaxPartySize = 6;
		[Tooltip("The server party update pump rate limit in seconds.")]
		public float UpdatePumpRate = 1.0f;
		public int UpdateFetchCount = 100;

		private LocalConnectionState serverState;
		private DateTime lastFetchTime = DateTime.UtcNow;
		private long lastPosition = 0;
		private float nextPump = 0.0f;
		// clientID / partyID
		private readonly Dictionary<long, long> pendingInvitations = new Dictionary<long, long>();

		private Dictionary<string, ChatCommand> partyChatCommands;
		public bool OnPartyInvite(Character sender, ChatBroadcast msg)
		{
			string targetName = msg.text.Trim().ToLower();
			if (Server.CharacterSystem.CharactersByLowerCaseName.TryGetValue(targetName, out Character character))
			{
				OnServerPartyInviteBroadcastReceived(sender.Owner, new PartyInviteBroadcast()
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
				partyChatCommands = new Dictionary<string, ChatCommand>()
				{
					{ "/pi", OnPartyInvite },
					{ "/invite", OnPartyInvite },
				};
				FChatHelper.AddDirectCommands(partyChatCommands);

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
			if (args.ConnectionState == LocalConnectionState.Started)
			{
				ServerManager.RegisterBroadcast<PartyCreateBroadcast>(OnServerPartyCreateBroadcastReceived, true);
				ServerManager.RegisterBroadcast<PartyInviteBroadcast>(OnServerPartyInviteBroadcastReceived, true);
				ServerManager.RegisterBroadcast<PartyAcceptInviteBroadcast>(OnServerPartyAcceptInviteBroadcastReceived, true);
				ServerManager.RegisterBroadcast<PartyDeclineInviteBroadcast>(OnServerPartyDeclineInviteBroadcastReceived, true);
				ServerManager.RegisterBroadcast<PartyLeaveBroadcast>(OnServerPartyLeaveBroadcastReceived, true);
				ServerManager.RegisterBroadcast<PartyRemoveBroadcast>(OnServerPartyRemoveBroadcastReceived, true);

				// remove the characters pending guild invite request on disconnect
				if (Server.CharacterSystem != null)
				{
					Server.CharacterSystem.OnDisconnect += RemovePending;
				}
			}
			else if (args.ConnectionState == LocalConnectionState.Stopped)
			{
				ServerManager.UnregisterBroadcast<PartyCreateBroadcast>(OnServerPartyCreateBroadcastReceived);
				ServerManager.UnregisterBroadcast<PartyInviteBroadcast>(OnServerPartyInviteBroadcastReceived);
				ServerManager.UnregisterBroadcast<PartyAcceptInviteBroadcast>(OnServerPartyAcceptInviteBroadcastReceived);
				ServerManager.UnregisterBroadcast<PartyDeclineInviteBroadcast>(OnServerPartyDeclineInviteBroadcastReceived);
				ServerManager.UnregisterBroadcast<PartyLeaveBroadcast>(OnServerPartyLeaveBroadcastReceived);
				ServerManager.UnregisterBroadcast<PartyRemoveBroadcast>(OnServerPartyRemoveBroadcastReceived);

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

					List<PartyUpdateEntity> updates = FetchPartyUpdates();
					ProcessPartyUpdates(updates);

				}
				nextPump -= Time.deltaTime;
			}
		}

		private List<PartyUpdateEntity> FetchPartyUpdates()
		{
			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();

			// fetch party updates from the database
			List<PartyUpdateEntity> updates = PartyUpdateService.Fetch(dbContext, lastFetchTime, lastPosition, UpdateFetchCount);
			if (updates != null && updates.Count > 0)
			{
				PartyUpdateEntity latest = updates[updates.Count - 1];
				if (latest != null)
				{
					lastFetchTime = latest.TimeCreated;
					lastPosition = latest.ID;
				}
			}
			return updates;
		}

		// process updates from the database
		private void ProcessPartyUpdates(List<PartyUpdateEntity> updates)
		{
			if (Server == null || Server.NpgsqlDbContextFactory == null || updates == null || updates.Count < 1)
			{
				return;
			}

			// parties that have previously been updated, we do this so we aren't updating partys multiple times
			HashSet<long> updatedParties = new HashSet<long>();

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			foreach (PartyUpdateEntity update in updates)
			{
				// check if we have already updated this party
				if (updatedParties.Contains(update.PartyID))
				{
					continue;
				}
				// otherwise add the party to our list and continue with the update
				updatedParties.Add(update.PartyID);

				// get the current party members from the database
				List<CharacterPartyEntity> dbMembers = FCharacterPartyService.Members(dbContext, update.PartyID);

				var addBroadcasts = dbMembers.Select(x => new PartyAddBroadcast()
				{
					partyID = x.PartyID,
					characterID = x.CharacterID,
					rank = (PartyRank)x.Rank,
					healthPCT = x.HealthPCT,
				}).ToList();

				PartyAddMultipleBroadcast partyAddBroadcast = new PartyAddMultipleBroadcast()
				{
					members = addBroadcasts,
				};

				// tell all of the local party members to update their party member lists
				foreach (CharacterPartyEntity entity in dbMembers)
				{
					if (Server.CharacterSystem.CharactersByID.TryGetValue(entity.CharacterID, out Character character))
					{
						if (character.PartyController.ID < 1)
						{
							continue;
						}
						character.PartyController.Rank = (PartyRank)entity.Rank;
						character.Owner.Broadcast(partyAddBroadcast, true, Channel.Reliable);
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

		public void OnServerPartyCreateBroadcastReceived(NetworkConnection conn, PartyCreateBroadcast msg, Channel channel)
		{
			if (conn.FirstObject == null)
			{
				return;
			}
			if (Server.NpgsqlDbContextFactory == null)
			{
				return;
			}

			FPartyController partyController = conn.FirstObject.GetComponent<FPartyController>();
			if (partyController == null || partyController.ID > 0)
			{
				// already in a party
				return;
			}

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			if (FPartyService.TryCreate(dbContext, out PartyEntity newParty))
			{
				partyController.ID = newParty.ID;
				partyController.Rank = PartyRank.Leader;
				FCharacterPartyService.Save(dbContext,
										   partyController.Character.ID.Value,
										   partyController.ID,
										   partyController.Rank,
										   partyController.Character.AttributeController.GetResourceAttributeCurrentPercentage(HealthTemplate));

				// tell the character we made their party successfully
				conn.Broadcast(new PartyCreateBroadcast()
				{
					partyID = newParty.ID,
					location = partyController.gameObject.scene.name,
				}, true, Channel.Reliable);
			}
		}

		public void OnServerPartyInviteBroadcastReceived(NetworkConnection conn, PartyInviteBroadcast msg, Channel channel)
		{
			if (Server.NpgsqlDbContextFactory == null)
			{
				return;
			}
			if (conn.FirstObject == null)
			{
				return;
			}
			FPartyController inviter = conn.FirstObject.GetComponent<FPartyController>();
			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();

			// validate party leader is inviting
			if (inviter == null ||
				inviter.ID < 1 ||
				inviter.Rank != PartyRank.Leader ||
				!FCharacterPartyService.ExistsNotFull(dbContext, inviter.ID, MaxPartySize))
			{
				return;
			}

			// if the target doesn't already have a pending invite
			if (!pendingInvitations.ContainsKey(msg.targetCharacterID) &&
				Server.CharacterSystem.CharactersByID.TryGetValue(msg.targetCharacterID, out Character targetCharacter))
			{
				FPartyController targetPartyController = targetCharacter.GetComponent<FPartyController>();

				// validate target
				if (targetPartyController == null || targetPartyController.ID > 0)
				{
					// we should tell the inviter the target is already in a party
					return;
				}

				// add to our list of pending invitations... used for validation when accepting/declining a party invite
				pendingInvitations.Add(targetCharacter.ID.Value, inviter.ID);
				targetCharacter.Owner.Broadcast(new PartyInviteBroadcast()
				{
					inviterCharacterID = inviter.ID,
					targetCharacterID = targetCharacter.ID.Value
				}, true, Channel.Reliable);
			}
		}

		public void OnServerPartyAcceptInviteBroadcastReceived(NetworkConnection conn, PartyAcceptInviteBroadcast msg, Channel channel)
		{
			if (conn.FirstObject == null)
			{
				return;
			}
			FPartyController partyController = conn.FirstObject.GetComponent<FPartyController>();

			// validate character
			if (partyController == null || partyController.ID > 0)
			{
				return;
			}

			// validate party invite
			if (pendingInvitations.TryGetValue(partyController.Character.ID.Value, out long pendingPartyID))
			{
				pendingInvitations.Remove(partyController.Character.ID.Value);

				if (Server == null || Server.NpgsqlDbContextFactory == null)
				{
					return;
				}
				using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
				List<CharacterPartyEntity> members = FCharacterPartyService.Members(dbContext, pendingPartyID);
				if (members != null &&
					members.Count < MaxPartySize)
				{
					partyController.ID = pendingPartyID;
					partyController.Rank = PartyRank.Member;

					FCharacterPartyService.Save(dbContext,
											   partyController.Character.ID.Value,
											   partyController.ID,
											   partyController.Rank,
											   partyController.Character.AttributeController.GetResourceAttributeCurrentPercentage(HealthTemplate));

					// tell the other servers to update their party lists
					PartyUpdateService.Save(dbContext, pendingPartyID);

					// tell the new member they joined immediately, other clients will catch up with the PartyUpdate pass
					conn.Broadcast(new PartyAddBroadcast()
					{
						partyID = pendingPartyID,
						characterID = partyController.Character.ID.Value,
						rank = PartyRank.Member,
						healthPCT = partyController.Character.AttributeController.GetResourceAttributeCurrentPercentage(HealthTemplate),
					}, true, Channel.Reliable);
				}
			}
		}

		public void OnServerPartyDeclineInviteBroadcastReceived(NetworkConnection conn, PartyDeclineInviteBroadcast msg, Channel channel)
		{
			Character character = conn.FirstObject.GetComponent<Character>();
			if (character != null)
			{
				pendingInvitations.Remove(character.ID.Value);
			}
		}

		public void OnServerPartyLeaveBroadcastReceived(NetworkConnection conn, PartyLeaveBroadcast msg, Channel channel)
		{
			if (Server.NpgsqlDbContextFactory == null)
			{
				return;
			}
			if (conn.FirstObject == null)
			{
				return;
			}
			FPartyController partyController = conn.FirstObject.GetComponent<FPartyController>();

			// validate character
			if (partyController == null || partyController.ID < 1)
			{
				// not in a party..
				return;
			}

			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();

			// validate party
			List<CharacterPartyEntity> members = FCharacterPartyService.Members(dbContext, partyController.ID);
			if (members != null &&
				members.Count > 0)
			{
				int remainingCount = members.Count - 1;

				List<CharacterPartyEntity> remainingMembers = new List<CharacterPartyEntity>();

				// are there any other members in the party? if so we transfer leadership
				if (partyController.Rank == PartyRank.Leader && remainingCount > 0)
				{
					foreach (CharacterPartyEntity member in members)
					{
						if (member.CharacterID == partyController.Character.ID.Value)
						{
							continue;
						}
						remainingMembers.Add(member);
					}

					CharacterPartyEntity newLeader = null;
					if (remainingMembers.Count > 0)
					{
						// pick a random member
						newLeader = remainingMembers[UnityEngine.Random.Range(0, remainingMembers.Count)];
					}

					// update the party leader status in the database
					if (newLeader != null)
					{
						FCharacterPartyService.Save(dbContext, newLeader.CharacterID, newLeader.PartyID, PartyRank.Leader, newLeader.HealthPCT);
					}
				}

				// remove the party member
				FCharacterPartyService.Delete(dbContext, partyController.Character.ID.Value);

				if (remainingCount < 1)
				{
					// delete the party
					FPartyService.Delete(dbContext, partyController.ID);
					PartyUpdateService.Delete(dbContext, partyController.ID);
				}
				else
				{
					// tell the other servers to update their party lists
					PartyUpdateService.Save(dbContext, partyController.ID);
				}

				partyController.ID = 0;
				partyController.Rank = PartyRank.None;

				// tell character that they left the party immediately, other clients will catch up with the PartyUpdate pass
				conn.Broadcast(new PartyLeaveBroadcast(), true, Channel.Reliable);
			}
		}

		public void OnServerPartyRemoveBroadcastReceived(NetworkConnection conn, PartyRemoveBroadcast msg, Channel channel)
		{
			if (Server.NpgsqlDbContextFactory == null)
			{
				return;
			}
			if (conn.FirstObject == null)
			{
				return;
			}
			FPartyController partyController = conn.FirstObject.GetComponent<FPartyController>();

			// validate character
			if (partyController == null ||
				partyController.ID < 1 ||
				partyController.Rank != PartyRank.Leader)
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
			if (memberID == partyController.Character.ID.Value)
			{
				return;
			}

			// remove the character from the party in the database
			using var dbContext = Server.NpgsqlDbContextFactory.CreateDbContext();
			bool result = FCharacterPartyService.Delete(dbContext, partyController.Rank, partyController.ID, memberID);
			if (result)
			{
				// tell the other servers to update their party lists
				PartyUpdateService.Save(dbContext, partyController.ID);
			}
		}
	}
}