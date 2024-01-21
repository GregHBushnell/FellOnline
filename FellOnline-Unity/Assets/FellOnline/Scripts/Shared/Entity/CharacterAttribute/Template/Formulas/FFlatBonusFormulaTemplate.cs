using Unity.VisualScripting;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "Flat Bonus Formula", menuName = "FellOnline/Character/Attribute/Formula/Flat  Bonus Formula", order = 1)]
	public class FlatBonusFormulaTemplate : FCharacterAttributeFormulaTemplate
	{
		public override int CalculateBonus(FCharacterAttribute self, FCharacterAttribute bonusAttribute)
		{
			return bonusAttribute.FinalValue + bonusAttribute.FinalValue;
		}
	}
}