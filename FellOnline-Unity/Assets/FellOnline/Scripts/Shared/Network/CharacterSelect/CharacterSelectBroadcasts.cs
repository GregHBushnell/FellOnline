using System.Collections.Generic;
using FishNet.Broadcast;

namespace FellOnline.Shared
{
	public struct CharacterRequestListBroadcast : IBroadcast
	{
	}

	public struct CharacterListBroadcast : IBroadcast
	{
		public List<CharacterDetails> characters;
	}

	public struct CharacterDeleteBroadcast : IBroadcast
	{
		public string characterName;
	}

	public struct CharacterSelectBroadcast : IBroadcast
	{
		public string characterName;
	}
}