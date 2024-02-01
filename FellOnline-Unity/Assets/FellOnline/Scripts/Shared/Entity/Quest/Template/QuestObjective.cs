using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	public abstract class QuestObjective : ScriptableObject
	{
		public long RequiredValue;
		public List<BaseItemTemplate> Rewards;
	}
}