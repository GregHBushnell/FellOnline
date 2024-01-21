using System;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Buff Database", menuName = "FellOnline/Character/Buff/Database", order = 1)]
	public class FBuffTemplateDatabase : ScriptableObject
	{
		[Serializable]
		public class BuffDictionary : SerializableDictionary<string, FBuffTemplate> { }

		[SerializeField]
		private BuffDictionary buffs = new BuffDictionary();
		public BuffDictionary Buffs { get { return this.buffs; } }

		public FBuffTemplate GetBuff(string name)
		{
			this.buffs.TryGetValue(name, out FBuffTemplate buff);
			return buff;
		}
	}
}