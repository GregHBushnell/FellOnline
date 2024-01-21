using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Item Attribute", menuName = "FellOnline/Item/Item Attribute/Attribute", order = 1)]
	public class FItemAttributeTemplate : FCachedScriptableObject<FItemAttributeTemplate>, FICachedObject
	{
		public int MinValue;
		public int MaxValue;
		public FCharacterAttributeTemplate CharacterAttribute;

		public string Name { get { return this.name; } }
	}
}