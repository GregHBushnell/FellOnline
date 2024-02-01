using System;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Item Database", menuName = "FellOnline/Item/Database", order = 0)]
	public class ItemTemplateDatabase : ScriptableObject
	{
		[Serializable]
		public class ItemDictionary : SerializableDictionary<string, BaseItemTemplate> { }

		[SerializeField]
		private ItemDictionary items = new ItemDictionary();
		public ItemDictionary Items { get { return items; } }

		public BaseItemTemplate GetItem(string name)
		{
			items.TryGetValue(name, out BaseItemTemplate item);
			return item;
		}
	}
}