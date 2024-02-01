using System;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Item Attribute Database", menuName = "FellOnline/Item/Item Attribute/Database", order = 0)]
	public class ItemAttributeTemplateDatabase : ScriptableObject
	{
		[Serializable]
		public class ItemAttributeDictionary : SerializableDictionary<string, ItemAttributeTemplate> { }

		[SerializeField]
		private ItemAttributeDictionary attributes = new ItemAttributeDictionary();
		public ItemAttributeDictionary Attributes { get { return this.attributes; } }

		public ItemAttributeTemplate GetItemAttribute(string name)
		{
			this.attributes.TryGetValue(name, out ItemAttributeTemplate attribute);
			return attribute;
		}
	}
}