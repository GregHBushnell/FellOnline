using System;

namespace FellOnline.Shared
{
	[Serializable]
	public class FWorldSceneDetails
	{
		public int MaxClients = 100;
		public FCharacterInitialSpawnPositionDictionary InitialSpawnPositions = new FCharacterInitialSpawnPositionDictionary();
		public FRespawnPositionDictionary RespawnPositions = new FRespawnPositionDictionary();
		public FSceneTeleporterDictionary Teleporters = new FSceneTeleporterDictionary();
		public FSceneBoundaryDictionary Boundaries = new FSceneBoundaryDictionary();
	}
}