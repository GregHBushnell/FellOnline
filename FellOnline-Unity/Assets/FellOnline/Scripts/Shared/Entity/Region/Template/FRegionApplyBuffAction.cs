namespace FellOnline.Shared
{
	public class FRegionApplyBuffAction : FRegionAction
	{
		public FBuffTemplate Buff;

		public override void Invoke(Character character, FRegion region)
		{
			if (Buff == null || character == null)
			{
				return;
			}
			character.BuffController.Apply(Buff);
		}
	}
}