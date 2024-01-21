namespace FellOnline.Shared
{
	public class FRegionCombatStateAction : FRegionAction
	{
		public bool EnableCombat;

		public override void Invoke(Character character, FRegion region)
		{
			/*if (character == null || character.CombatController == null)
			{
				return;
			}
			character.CombatController.SetCombatStatus(enableCombat);*/
		}
	}
}