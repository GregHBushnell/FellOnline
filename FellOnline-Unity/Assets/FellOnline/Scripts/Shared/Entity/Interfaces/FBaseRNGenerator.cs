namespace FellOnline.Shared
{
	public abstract class FBaseRNGenerator
	{
		protected int seed;

		public int Seed
		{
			get
			{
				return seed;
			}
			set
			{
				if (seed != value)
				{
					seed = value;
					Generate();
				}
			}
		}

		public void Generate()
		{
			Generate(seed);
		}

		public abstract void Generate(int seed);
	}
}