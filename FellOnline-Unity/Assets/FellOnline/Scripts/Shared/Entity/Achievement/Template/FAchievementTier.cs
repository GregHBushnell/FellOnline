using System;

namespace FellOnline.Shared
{
	[Serializable]
	public class FAchievementTier
	{
		public string Description;
		public uint MaxValue;
		public string TierCompleteMessage;
		//public AudioEvent CompleteSound;
		public FBaseItemTemplate[] ItemRewards;
		public FBuffTemplate[] BuffRewards;
		//public string[] TitleRewards;
		//public AbilityTemplate[] AbilityRewards;
	}
}