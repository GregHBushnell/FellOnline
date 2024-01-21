using UnityEngine;

namespace FellOnline.Shared
{
	public abstract class FCharacterAttributeFormulaTemplate : ScriptableObject
	{
		public abstract int CalculateBonus(FCharacterAttribute self, FCharacterAttribute bonusAttribute);
	}
}