using System;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Achievement Database", menuName = "FellOnlineCharacter/Achievement/Database", order = 0)]
	public class FAchievementTemplateDatabase : ScriptableObject
	{
		[Serializable]
		public class FAchievementDictionary : SerializableDictionary<string, FAchievementTemplate> { }

		[SerializeField]
		private FAchievementDictionary achievements = new FAchievementDictionary();
		public FAchievementDictionary Achievements { get { return this.achievements; } }

		public FAchievementTemplate GetAchievement(string name)
		{
			this.achievements.TryGetValue(name, out FAchievementTemplate achievement);
			return achievement;
		}
	}
}