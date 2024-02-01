using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Armor", menuName = "FellOnline/Item/Armor", order = 0)]
	public class ArmorTemplate : BaseEquipmentTemplate
	{
		public ItemAttributeTemplate ArmorBonus;
	}
}