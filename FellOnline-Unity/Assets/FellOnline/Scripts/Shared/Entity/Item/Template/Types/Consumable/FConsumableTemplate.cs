namespace FellOnline.Shared
{
	public abstract class FConsumableTemplate : FBaseItemTemplate
	{
		public ConsumableType ConsumableType;
		public uint ChargeCost = 1;
		public float Cooldown;

		public bool CanConsume(Character character, FItem item)
		{
			return character != null &&
				   item != null &&
				   item.IsStackable &&
				   item.Stackable.Amount > 1 &&
				   !character.CooldownController.IsOnCooldown(ConsumableType.ToString());
		}

		public virtual bool Invoke(Character character, FItem item)
		{
			if (CanConsume(character, item))
			{
				if (Cooldown > 0.0f)
				{
					character.CooldownController.AddCooldown(ConsumableType.ToString(), new FCooldownInstance(Cooldown));
				}
				if (item.IsStackable && item.Stackable.Amount > ChargeCost)
				{
					//consume charges
					item.Stackable.Remove(ChargeCost);

					if (item.Stackable.Amount < 1)
					{
						item.Destroy();
					}
				}
				else
				{
					item.Destroy();
				}
				return true;
			}
			return false;
		}
	}
}