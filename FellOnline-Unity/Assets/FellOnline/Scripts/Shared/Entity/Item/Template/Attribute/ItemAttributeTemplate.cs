using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Item Attribute", menuName = "FellOnline/Item/Item Attribute/Attribute", order = 1)]
	public class ItemAttributeTemplate : CachedScriptableObject<ItemAttributeTemplate>, ICachedObject
	{
		public int MinValue;
		public int MaxValue;
		public CharacterAttributeTemplate CharacterAttribute;

		public string Name { get { return this.name; } }
	}
}