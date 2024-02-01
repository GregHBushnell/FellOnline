using UnityEngine;

namespace FellOnline.Shared
{
	public abstract class CharacterAttributeFormulaTemplate : ScriptableObject
	{
		public abstract int CalculateBonus(CharacterAttribute self, CharacterAttribute bonusAttribute);
	}
}