using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Achievement", menuName = "FellOnlineItem/Achievement/Achievement", order = 1)]
	public sealed class AchievementTemplate : CachedScriptableObject<AchievementTemplate>, ICachedObject
	{
		public List<AchievementTier> Tiers;

		public string Name { get { return this.name; } }
	}
}