namespace FellOnline.Shared
{
	public class MobResourceAttribute : MobAttribute
	{
		private int currentValue;

		public int CurrentValue { get { return currentValue; } }

		public override string ToString()
		{
			return Template.Name + ": " + currentValue + "/" + FinalValue;
		}

		public MobResourceAttribute(int templateID, int initialValue, int currentValue, int modifier) : base(templateID, initialValue, modifier)
		{
			this.currentValue = currentValue;
		}

		public void AddToCurrentValue(int value)
		{
			int tmp = currentValue;
			currentValue += value;
			if (currentValue == tmp)
			{
				return;
			}
			if (currentValue > this.FinalValue)
			{
				currentValue = this.FinalValue;
			}
			Internal_OnAttributeChanged(this);
		}

		public void SetCurrentValue(int value)
		{
			currentValue = value;
			Internal_OnAttributeChanged(this);
		}

		public void Consume(int amount)
		{
			currentValue -= amount;
			if (currentValue < 0)
			{
				currentValue = 0;
			}
			Internal_OnAttributeChanged(this);
		}

		public void Gain(int amount)
		{
			currentValue += amount;
			if (currentValue >= FinalValue)
			{
				currentValue = FinalValue;
			}
			Internal_OnAttributeChanged(this);
		}

		protected override void Internal_OnAttributeChanged(MobAttribute attribute)
		{
			base.Internal_OnAttributeChanged(attribute);
		}
	}
}