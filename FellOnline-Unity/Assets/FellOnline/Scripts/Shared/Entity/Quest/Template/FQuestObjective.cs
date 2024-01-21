using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	public abstract class FQuestObjective : ScriptableObject
	{
		public long RequiredValue;
		public List<FBaseItemTemplate> Rewards;
	}
}