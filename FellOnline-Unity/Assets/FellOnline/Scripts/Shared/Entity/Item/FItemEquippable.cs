﻿using System;

namespace FellOnline.Shared
{
	public class FItemEquippable : FIEquippable<Character>
	{
		private FItem item;

		public event Action<Character> OnEquip;
		public event Action<Character> OnUnequip;

		public Character Owner { get; private set; }

		public void Initialize(FItem item)
		{
			this.item = item;
			if (item.Generator != null)
			{
				item.Generator.OnSetAttribute += ItemGenerator_OnSetAttribute;
			}
		}

		public void Destroy()
		{
			if (item.Generator != null)
			{
				item.Generator.OnSetAttribute -= ItemGenerator_OnSetAttribute;
			}

			Unequip();
		}

		public void Equip(Character owner)
		{
			if (Owner != null)
			{
				Unequip();
			}
			if (owner != null)
			{
				Owner = owner;
				OnEquip?.Invoke(owner);
			}
		}

		public void Unequip()
		{
			if (Owner != null)
			{
				OnUnequip?.Invoke(Owner);
				Owner = null;
			}
		}

		public void ItemGenerator_OnSetAttribute(FItemAttribute attribute, int oldValue, int newValue)
		{
			if (Owner != null)
			{
				if (Owner.AttributeController != null &&
					Owner.AttributeController.TryGetAttribute(attribute.Template.CharacterAttribute.ID, out FCharacterAttribute characterAttribute))
				{
					characterAttribute.AddValue(-oldValue);
					characterAttribute.AddValue(newValue);
				}
			}
		}
	}
}