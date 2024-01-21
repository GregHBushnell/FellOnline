namespace FellOnline.Shared
{
	public class FQuestAttributeRequirement
	{
		public FCharacterAttributeTemplate template;
		public long minRequiredValue;

		public bool MeetsRequirements(FCharacterAttributeController characterAttributes)
		{
			FCharacterAttribute attribute;
			if (!characterAttributes.TryGetAttribute(template.ID, out attribute) || attribute.FinalValue < minRequiredValue)
			{
				return false;
			}
			return true;
		}
	}
}