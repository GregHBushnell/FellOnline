using UnityEngine;

namespace FellOnline.Shared
{
	public abstract class MobAttributeFormulaTemplate : ScriptableObject
	{
		public abstract int CalculateBonus(MobAttribute self,MobAttribute bonusAttribute);
	}
}