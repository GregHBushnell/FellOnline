using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Damage Attribute", menuName = "FellOnline/Character/Attribute/Damage Attribute", order = 1)]
	public class DamageAttributeTemplate : CharacterAttributeTemplate
	{
		public ResistanceAttributeTemplate Resistance;
		public Color DisplayColor;
	}
}