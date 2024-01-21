using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "Percentage Bonus Formula", menuName = "FellOnline/Character/Attribute/Formula/Percentage Bonus Formula", order = 1)]
	public class FPercentageBonusFormulaTemplate : FCharacterAttributeFormulaTemplate
	{
		public float Percentage;

		public override int CalculateBonus(FCharacterAttribute self, FCharacterAttribute bonusAttribute)
		{
			return (int)(bonusAttribute.FinalValue * Percentage);
		}
	}
}