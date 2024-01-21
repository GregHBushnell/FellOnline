using System;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Hair Database", menuName = "FellOnline/Character/Customization/HairDatabase", order = 1)]
	public class CharacterHairDataBase : ScriptableObject
	{
		[Serializable]
		public class HairDictionary : SerializableDictionary<int, CharacterHairTemplate> { }

		[SerializeField]
		private HairDictionary hairs = new HairDictionary();
		public HairDictionary Hairs { get { return this.hairs; } }

		public CharacterHairTemplate GetHair(int id)
		{
			this.hairs.TryGetValue(id, out CharacterHairTemplate hair);
			return hair;
		}
	}
}