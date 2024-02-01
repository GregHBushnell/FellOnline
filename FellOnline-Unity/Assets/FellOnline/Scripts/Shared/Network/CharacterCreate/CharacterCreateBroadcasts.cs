using FishNet.Broadcast;

namespace FellOnline.Shared
{
	public struct CharacterCreateBroadcast : IBroadcast
	{
		public string characterName;
		public int raceIndex;
		public int hairIndex;
		public int skinColor;
		public int hairColor;
	
		public CharacterInitialSpawnPosition initialSpawnPosition;
	}

	public struct CharacterCreateResultBroadcast : IBroadcast
	{
		public CharacterCreateResult result;
	}
}