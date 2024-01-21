using System.Collections.Generic;

namespace FellOnline.Shared
{
	public class FQuestInstance
	{
		public FQuestTemplate template;

		public List<FQuestObjective> Objectives;

		private QuestStatus status = QuestStatus.Inactive;

		public QuestStatus Status
		{
			get
			{
				return this.status;
			}
		}
	}
}