﻿using System.Collections.Generic;

namespace FellOnline.Shared
{
	public class QuestInstance
	{
		public QuestTemplate template;

		public List<QuestObjective> Objectives;

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