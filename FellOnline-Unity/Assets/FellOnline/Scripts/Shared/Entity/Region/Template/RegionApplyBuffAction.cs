namespace FellOnline.Shared
{
	public class RegionApplyBuffAction : RegionAction
	{
		public BuffTemplate Buff;

		public override void Invoke(Character character, Region region)
		{
			if (Buff == null ||
				character == null ||
				!character.TryGet(out BuffController buffController))
			{
				return;
			}
			buffController.Apply(Buff);
		}
	}
}