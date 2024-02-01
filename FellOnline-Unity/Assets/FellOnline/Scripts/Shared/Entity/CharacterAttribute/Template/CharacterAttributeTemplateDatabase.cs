using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Character Attribute Database", menuName = "FellOnline/Character/Attribute/Database", order = 0)]
	public class CharacterAttributeTemplateDatabase : ScriptableObject
	{
		[SerializeField]
		private CharacterAttributeTemplateDictionary attributes = new CharacterAttributeTemplateDictionary();
		public CharacterAttributeTemplateDictionary Attributes { get { return attributes; } }

		public CharacterAttributeTemplate GetCharacterAttribute(string name)
		{
			attributes.TryGetValue(name, out CharacterAttributeTemplate attribute);
			return attribute;
		}
	}
}