namespace FellOnline.Shared
{
	public class FAchievement
	{
		public byte CurrentTier;
		public uint CurrentValue;

		public FAchievementTemplate Template { get; private set; }

		public FAchievement(int templateID)
		{
			Template = FAchievementTemplate.Get<FAchievementTemplate>(templateID);
			CurrentTier = 0;
			CurrentValue = 0;
		}

		public FAchievement(int templateID, byte tier, uint value)
		{
			Template = FAchievementTemplate.Get<FAchievementTemplate>(templateID);
			CurrentTier = tier;
			CurrentValue = value;
		}
	}
}