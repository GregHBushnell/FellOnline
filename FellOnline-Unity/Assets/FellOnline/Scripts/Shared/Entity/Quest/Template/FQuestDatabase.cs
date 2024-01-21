using System;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "FNew Quest Database", menuName = "FellOnline/Character/Quest/Database", order = 0)]
	public class QuestDatabase : ScriptableObject
	{
		[Serializable]
		public class QuestDictionary : SerializableDictionary<string, FQuestTemplate> { }

		[SerializeField]
		private QuestDictionary quests = new QuestDictionary();
		public QuestDictionary Quests { get { return this.quests; } }

		public FQuestTemplate GetQuest(string name)
		{
			this.quests.TryGetValue(name, out FQuestTemplate quest);
			return quest;
		}
	}
}