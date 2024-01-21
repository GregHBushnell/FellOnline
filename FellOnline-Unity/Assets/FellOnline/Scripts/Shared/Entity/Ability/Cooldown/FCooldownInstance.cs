namespace FellOnline.Shared
{
	public class FCooldownInstance
	{
		private float remainingTime;

		public bool IsOnCooldown
		{
			get
			{
				return remainingTime > 0.0f;
			}
		}

		public FCooldownInstance(float cooldown)
		{
			remainingTime = cooldown;
		}

		public void SubtractTime(float time)
		{
			remainingTime -= time;
		}

		public void AddTime(float time)
		{
			remainingTime += time;
		}
	}
}