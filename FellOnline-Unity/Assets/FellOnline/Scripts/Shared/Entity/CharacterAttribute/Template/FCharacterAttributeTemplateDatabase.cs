using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Character Attribute Database", menuName = "FellOnline/Character/Attribute/Database", order = 0)]
	public class FCharacterAttributeTemplateDatabase : ScriptableObject
	{
		[SerializeField]
		private FCharacterAttributeTemplateDictionary attributes = new FCharacterAttributeTemplateDictionary();
		public FCharacterAttributeTemplateDictionary Attributes { get { return attributes; } }

		public FCharacterAttributeTemplate GetCharacterAttribute(string name)
		{
			attributes.TryGetValue(name, out FCharacterAttributeTemplate attribute);
			return attribute;
		}
	}
}