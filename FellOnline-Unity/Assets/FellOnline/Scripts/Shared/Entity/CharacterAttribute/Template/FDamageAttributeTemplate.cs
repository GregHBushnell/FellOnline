using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Damage Attribute", menuName = "FellOnline/Character/Attribute/Damage Attribute", order = 1)]
	public class FDamageAttributeTemplate : FCharacterAttributeTemplate
	{
		public FResistanceAttributeTemplate Resistance;
		public Color DisplayColor;
	}
}