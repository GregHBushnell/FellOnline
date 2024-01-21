using System;
using UnityEngine;

namespace FellOnline.Shared
{
	[Serializable]
	public class FBuffAttributeTemplate
	{
		public long MinValue;
		public long MaxValue;
		[Tooltip("Character Attribute the buff will apply its values to.")]
		public FCharacterAttributeTemplate Template;
	}
}