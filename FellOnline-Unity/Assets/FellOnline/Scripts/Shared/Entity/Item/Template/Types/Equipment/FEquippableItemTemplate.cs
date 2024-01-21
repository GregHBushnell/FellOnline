using UnityEngine;

namespace FellOnline.Shared
{
	public abstract class FEquippableItemTemplate : FBaseItemTemplate
	{
		public ItemSlot Slot;
		[Tooltip("The maximum number of attributes the item will have when it's generated.")]
		public int MaxItemAttributes;
		//random item attributes
		public FItemAttributeTemplateDatabase[] AttributeDatabases;
		public uint ModelSeed;
		//different pools for different models
		public int[] ModelPools;
	}
}