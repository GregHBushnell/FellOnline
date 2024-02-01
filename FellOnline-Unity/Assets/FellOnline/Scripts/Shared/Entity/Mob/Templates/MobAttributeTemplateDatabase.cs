using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Mob Attribute Database", menuName = "FellOnline/Mob/Attribute/Database", order = 0)]
	public class MobAttributeTemplateDatabase : ScriptableObject
	{
		[SerializeField]
		private MobAttributeTemplateDictionary attributes = new MobAttributeTemplateDictionary();
		public MobAttributeTemplateDictionary Attributes { get { return attributes; } }

		public MobAttributeTemplate GetMobAttribute(string name)
		{
			attributes.TryGetValue(name, out MobAttributeTemplate attribute);
			return attribute;
		}
	}
}