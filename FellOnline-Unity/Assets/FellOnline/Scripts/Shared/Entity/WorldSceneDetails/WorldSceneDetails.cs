using System;

namespace FellOnline.Shared
{
	[Serializable]
	public class WorldSceneDetails
	{
		public int MaxClients = 100;
		public CharacterInitialSpawnPositionDictionary InitialSpawnPositions = new CharacterInitialSpawnPositionDictionary();
		public RespawnPositionDictionary RespawnPositions = new RespawnPositionDictionary();
		public SceneTeleporterDictionary Teleporters = new SceneTeleporterDictionary();
		public SceneBoundaryDictionary Boundaries = new SceneBoundaryDictionary();
	}
}