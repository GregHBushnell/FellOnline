using Unity.VisualScripting;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "Flat Bonus Formula", menuName = "FellOnline/Mob/Attribute/Formula/Flat  Bonus Formula", order = 1)]
	public class MobFlatBonusFormulaTemplate : MobAttributeFormulaTemplate
	{
		public override int CalculateBonus(MobAttribute self, MobAttribute bonusAttribute)
		{
			return bonusAttribute.FinalValue + bonusAttribute.FinalValue;
		}
	}
}