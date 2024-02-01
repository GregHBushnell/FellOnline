using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "Percentage Bonus Formula", menuName = "FellOnline/Character/Attribute/Formula/Percentage Bonus Formula", order = 1)]
	public class PercentageBonusFormulaTemplate : CharacterAttributeFormulaTemplate
	{
		public float Percentage;

		public override int CalculateBonus(CharacterAttribute self, CharacterAttribute bonusAttribute)
		{
			return (int)(bonusAttribute.FinalValue * Percentage);
		}
	}
}