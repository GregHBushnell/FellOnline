using System.Collections.Generic;
using System.Linq;
using Cysharp.Text;
using System;

namespace FellOnline.Shared
{
	public class FItemGenerator : FBaseRNGenerator
	{
		private Dictionary<string, FItemAttribute> attributes = new Dictionary<string, FItemAttribute>();

		private FItem item;
		public event Action<FItemAttribute, int, int> OnSetAttribute;

		public void Initialize(FItem item, int seed)
		{
			this.item = item;
			Seed = seed;

			if (item.Equippable != null)
			{
				item.Equippable.OnEquip += ItemEquippable_OnEquip;
				item.Equippable.OnUnequip += ItemEquippable_OnUnequip;
			}
		}

		public void Destroy()
		{
			if (item.Equippable != null)
			{
				item.Equippable.OnEquip -= ItemEquippable_OnEquip;
				item.Equippable.OnUnequip -= ItemEquippable_OnUnequip;
			}
			item = null;
		}

		public void Tooltip(ref Utf16ValueStringBuilder sb)
		{
			sb.Append("<color=#a66ef5>Seed: ");
			sb.Append(Seed);
			sb.Append("</color>");
			if (attributes.Count > 0)
			{
				sb.AppendLine();
				sb.Append("<size=125%><color=#a66ef5>Attributes:</color></size>");
				sb.AppendLine();
				foreach (FItemAttribute attribute in attributes.Values)
				{
					sb.Append("<size=110%>");
					sb.Append(attribute.Template.Name);
					sb.Append(": <color=#32a879>");
					sb.Append(attribute.value);
					sb.Append("</color></size>");
					sb.AppendLine();
				}
			}
		}

		public override void Generate(int seed)
		{
			this.seed = seed;

			System.Random random = new System.Random(seed);
			if (random != null)
			{
				if (attributes != null)
				{
					attributes.Clear();
					attributes = new Dictionary<string, FItemAttribute>();

					FEquippableItemTemplate Equippable = item.Template as FEquippableItemTemplate;
					if (Equippable == null)
						return;

					FWeaponTemplate weapon = item.Template as FWeaponTemplate;
					if (weapon != null)
					{
						attributes.Add(weapon.AttackPower.Name, new FItemAttribute(weapon.AttackPower.ID, random.Next(weapon.AttackPower.MinValue, weapon.AttackPower.MaxValue)));
						attributes.Add(weapon.AttackSpeed.Name, new FItemAttribute(weapon.AttackSpeed.ID, random.Next(weapon.AttackSpeed.MinValue, weapon.AttackSpeed.MaxValue)));
					}
					else
					{
						FArmorTemplate armor = item.Template as FArmorTemplate;
						if (armor != null)
						{
							attributes.Add(armor.ArmorBonus.Name, new FItemAttribute(armor.ArmorBonus.ID, random.Next(armor.ArmorBonus.MinValue, armor.ArmorBonus.MaxValue)));
						}
					}

					if (Equippable.AttributeDatabases != null && Equippable.AttributeDatabases.Length > 0)
					{
						int attributeCount = random.Next(0, Equippable.MaxItemAttributes);
						for (int i = 0, rng; i < attributeCount; ++i)
						{
							rng = random.Next(0, Equippable.AttributeDatabases.Length);
							FItemAttributeTemplateDatabase db = Equippable.AttributeDatabases[rng];
							rng = random.Next(0, db.Attributes.Count);
							FItemAttributeTemplate attributeTemplate = Enumerable.ToList(db.Attributes.Values)[rng];
							attributes.Add(attributeTemplate.Name, new FItemAttribute(attributeTemplate.ID, random.Next(attributeTemplate.MinValue, attributeTemplate.MaxValue)));
						}
					}
				}
			}
		}

		public FItemAttribute GetAttribute(string name)
		{
			attributes.TryGetValue(name, out FItemAttribute attribute);
			return attribute;
		}

		public void SetAttribute(string name, int newValue)
		{
			if (attributes.TryGetValue(name, out FItemAttribute attribute))
			{
				if (attribute.value == newValue) return;

				int oldValue = attribute.value;
				attribute.value = newValue;

				OnSetAttribute?.Invoke(attribute, oldValue, newValue);
			}
		}

		public void ItemEquippable_OnEquip(Character character)
		{
			foreach (KeyValuePair<string, FItemAttribute> pair in attributes)
			{
				if (character.AttributeController.TryGetAttribute(pair.Value.Template.CharacterAttribute.ID, out FCharacterAttribute characterAttribute))
				{
					characterAttribute.AddValue(pair.Value.value);
				}
			}
		}

		public void ItemEquippable_OnUnequip(Character character)
		{
			foreach (KeyValuePair<string, FItemAttribute> pair in attributes)
			{
				if (character.AttributeController.TryGetAttribute(pair.Value.Template.CharacterAttribute.ID, out FCharacterAttribute characterAttribute))
				{
					characterAttribute.AddValue(-pair.Value.value);
				}
			}
		}
	}
}