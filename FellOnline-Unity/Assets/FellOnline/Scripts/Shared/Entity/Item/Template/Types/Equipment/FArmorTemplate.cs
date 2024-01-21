using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Armor", menuName = "FellOnline/Item/Armor", order = 0)]
	public class FArmorTemplate : FEquippableItemTemplate
	{
		public FItemAttributeTemplate ArmorBonus;
	}
}