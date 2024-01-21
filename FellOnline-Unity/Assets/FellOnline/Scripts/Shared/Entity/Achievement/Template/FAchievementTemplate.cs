using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Achievement", menuName = "FellOnlineItem/Achievement/Achievement", order = 1)]
	public sealed class FAchievementTemplate : FCachedScriptableObject<FAchievementTemplate>, FICachedObject
	{
		public List<FAchievementTier> Tiers;

		public string Name { get { return this.name; } }
	}
}