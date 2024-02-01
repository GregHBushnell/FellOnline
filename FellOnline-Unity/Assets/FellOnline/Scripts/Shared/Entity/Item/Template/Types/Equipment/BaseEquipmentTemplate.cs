using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Armor", menuName = "FellOnline/Item/Equipment", order = 0)]
	public class BaseEquipmentTemplate : BaseItemTemplate
	{
         public ItemSlot Slot;
		[Tooltip("The maximum number of attributes the item will have when it's generated.")]
        	public int MaxItemAttributes;
		public ItemAttributeTemplateDatabase[] AttributeDatabases;

		public uint VisualID;

		public WeaponEquipStyle WeaponEquipStyle;
	}
}