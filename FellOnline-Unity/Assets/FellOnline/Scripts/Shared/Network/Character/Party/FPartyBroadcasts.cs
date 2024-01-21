﻿using FishNet.Broadcast;
using System.Collections.Generic;

namespace FellOnline.Shared
{
	public struct PartyCreateBroadcast : IBroadcast
	{
		public long partyID;
		public string location;
	}

	public struct PartyInviteBroadcast : IBroadcast
	{
		public long inviterCharacterID;
		public long targetCharacterID;
	}

	public struct PartyAcceptInviteBroadcast : IBroadcast
	{
	}
	public struct PartyDeclineInviteBroadcast : IBroadcast
	{
	}

	public struct PartyAddBroadcast : IBroadcast
	{
		public long partyID;
		public long characterID;
		public PartyRank rank;
		public float healthPCT;
	}

	public struct PartyLeaveBroadcast : IBroadcast
	{
	}

	public struct PartyAddMultipleBroadcast : IBroadcast
	{
		public List<PartyAddBroadcast> members;

	}
	public struct PartyRemoveBroadcast : IBroadcast
	{
		public List<long> members;
	}
}