using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "Percentage Bonus Formula", menuName = "FellOnline/Mob/Attribute/Formula/Percentage Bonus Formula", order = 1)]
	public class MobPercentageBonusFormulaTemplate : MobAttributeFormulaTemplate
	{
		public float Percentage;

		public override int CalculateBonus(MobAttribute self, MobAttribute bonusAttribute)
		{
			return (int)(bonusAttribute.FinalValue * Percentage);
		}
	}
}