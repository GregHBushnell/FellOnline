using System;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Item Attribute Database", menuName = "FellOnline/Item/Item Attribute/Database", order = 0)]
	public class FItemAttributeTemplateDatabase : ScriptableObject
	{
		[Serializable]
		public class FItemAttributeDictionary : SerializableDictionary<string, FItemAttributeTemplate> { }

		[SerializeField]
		private FItemAttributeDictionary attributes = new FItemAttributeDictionary();
		public FItemAttributeDictionary Attributes { get { return this.attributes; } }

		public FItemAttributeTemplate GetItemAttribute(string name)
		{
			this.attributes.TryGetValue(name, out FItemAttributeTemplate attribute);
			return attribute;
		}
	}
}