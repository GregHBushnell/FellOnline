﻿using FishNet.Broadcast;
using System.Collections.Generic;

namespace FellOnline.Shared
{
	public struct FriendAddNewBroadcast : IBroadcast
	{
		public string characterName;
	}

	public struct FriendAddBroadcast : IBroadcast
	{
		public long characterID;
		public bool online;
	}

	public struct FriendAddMultipleBroadcast : IBroadcast
	{
		public List<FriendAddBroadcast> friends;
	}

	public struct FriendRemoveBroadcast : IBroadcast
	{
		public long characterID;
	}
}