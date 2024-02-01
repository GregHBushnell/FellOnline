using System;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Achievement Database", menuName = "FellOnlineCharacter/Achievement/Database", order = 0)]
	public class AchievementTemplateDatabase : ScriptableObject
	{
		[Serializable]
		public class AchievementDictionary : SerializableDictionary<string, AchievementTemplate> { }

		[SerializeField]
		private AchievementDictionary achievements = new AchievementDictionary();
		public AchievementDictionary Achievements { get { return this.achievements; } }

		public AchievementTemplate GetAchievement(string name)
		{
			this.achievements.TryGetValue(name, out AchievementTemplate achievement);
			return achievement;
		}
	}
}