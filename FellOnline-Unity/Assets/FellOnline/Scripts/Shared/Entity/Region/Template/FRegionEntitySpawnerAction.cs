namespace FellOnline.Shared
{
	public class FRegionEntitySpawnerAction : FRegionAction
	{
		//public Entity[] spawnables;
		public int minSpawns = 0;
		public int maxSpawns = 1;

		public override void Invoke(Character character, FRegion region)
		{
			// spawn things
		}
	}
}