namespace FellOnline.Shared
{
	public class FSetOnce<T>
	{
		private readonly object lockObj = new object();
		private bool isSet = false;
		private T value;

		public T Value
		{
			get
			{
				lock (this.lockObj)
				{
					return this.value;
				}
			}
			set
			{
				lock (this.lockObj)
				{
					if (this.isSet)
					{
						return;
					}
					this.isSet = true;
					this.value = value;
				}
			}
		}

		public static implicit operator T(FSetOnce<T> convert)
		{
			return convert.Value;
		}
	}
}