using System.Collections.Generic;

namespace FellOnline.Shared
{
	public abstract class FScrollConsumableTemplate : FConsumableTemplate
	{
		public List<FBaseAbilityTemplate> AbilityTemplates;

		public override bool Invoke(Character character, FItem item)
		{
			if (base.Invoke(character, item))
			{
				if (character.AbilityController != null)
				{
					character.AbilityController.LearnBaseAbilities(AbilityTemplates);
				}
				return true;
			}
			return false;
		}
	}
}