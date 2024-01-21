using System;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Item Database", menuName = "FellOnline/Item/Database", order = 0)]
	public class FItemTemplateDatabase : ScriptableObject
	{
		[Serializable]
		public class ItemDictionary : SerializableDictionary<string, FBaseItemTemplate> { }

		[SerializeField]
		private ItemDictionary items = new ItemDictionary();
		public ItemDictionary Items { get { return items; } }

		public FBaseItemTemplate GetItem(string name)
		{
			items.TryGetValue(name, out FBaseItemTemplate item);
			return item;
		}
	}
}