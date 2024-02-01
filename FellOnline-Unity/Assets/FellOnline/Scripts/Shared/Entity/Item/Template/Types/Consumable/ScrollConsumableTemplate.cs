﻿using System.Collections.Generic;

namespace FellOnline.Shared
{
	public abstract class ScrollConsumableTemplate : ConsumableTemplate
	{
		public List<BaseAbilityTemplate> AbilityTemplates;

		public override bool Invoke(Character character, Item item)
		{
			if (base.Invoke(character, item))
			{
				if (character.TryGet(out AbilityController abilityController))
				{
					abilityController.LearnBaseAbilities(AbilityTemplates);
				}
				return true;
			}
			return false;
		}
	}
}