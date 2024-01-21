namespace FellOnline.Shared
{
	public class FRegionApplyCharacterAttributeAction : FRegionAction
	{
		public FCharacterAttributeTemplate attribute;
		public int value;

		public override void Invoke(Character character, FRegion region)
		{
			if (attribute == null || character == null)
			{
				return;
			}
			if (character.AttributeController.TryGetResourceAttribute(attribute, out FCharacterResourceAttribute r))
			{
				r.AddToCurrentValue(value);
			}
			else if (character.AttributeController.TryGetAttribute(attribute, out FCharacterAttribute c))
			{
				c.AddModifier(value);
			}
		}
	}
}