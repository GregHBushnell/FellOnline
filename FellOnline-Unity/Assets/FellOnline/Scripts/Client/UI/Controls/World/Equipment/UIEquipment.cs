using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class UIEquipment :UICharacterControl
	{
		public RectTransform content;
		public TMP_Text UILabel;
		public UIAttribute AttributeLabelPrefab;

		public List<UIEquipmentButton> buttons = new List<UIEquipmentButton>();

		public List<TMP_Text> attributeCategoryLabels = new List<TMP_Text>();
		public Dictionary<string, UIAttribute> attributeLabels = new Dictionary<string, UIAttribute>();
		public override void OnStarting()
		{
			UIEquipmentButton[] equipmentButtons = gameObject.GetComponentsInChildren<UIEquipmentButton>();
			if (equipmentButtons != null)
			{
				buttons = new List<UIEquipmentButton>();
				for (int i = 0; i < equipmentButtons.Length; ++i)
				{
					UIEquipmentButton button = equipmentButtons[i];
					button.Type = ReferenceButtonType.Equipment;
					button.ReferenceID = (int)button.ItemSlotType;
					buttons.Add(button);
				}
			}
		}

		public override void OnDestroying()
		{
			DestroyAttributes();
		}

		private void DestroyAttributes()
		{
			if (attributeLabels != null)
			{
				foreach (UIAttribute obj in attributeLabels.Values)
				{
					Destroy(obj.gameObject);
				}
				attributeLabels.Clear();
			}
			if (attributeCategoryLabels != null)
			{
				foreach (TMP_Text text in attributeCategoryLabels)
				{
					Destroy(text.gameObject);
				}
				attributeCategoryLabels.Clear();
			}
		}

		public override void OnPreSetCharacter()
		{
			if (Character != null)
			{
				if (Character.TryGet(out CharacterAttributeController attributeController))
				{
					foreach (CharacterAttribute attribute in attributeController.Attributes.Values)
					{
						attribute.OnAttributeUpdated -= OnAttributeUpdated;
					}
				}
				if (Character.TryGet(out EquipmentController equipmentController))
				{
					equipmentController.OnSlotUpdated -= OnEquipmentSlotUpdated;
				}
			}
		}

		public override void SetCharacter(Character character)
		{
			base.SetCharacter(character);
			DestroyAttributes();

			if (buttons != null &&
				Character != null &&
				Character.TryGet(out EquipmentController equipmentController))
			{
				equipmentController.OnSlotUpdated -= OnEquipmentSlotUpdated;
				foreach (UIEquipmentButton button in buttons)
				{
					button.Character = Character;
					if (Character != null)
					{
						SetButtonSlot(equipmentController, button);
					}
				}
				equipmentController.OnSlotUpdated += OnEquipmentSlotUpdated;
			}

			if (Character != null &&
				Character.TryGet(out CharacterAttributeController attributeController))
			{
				
				List<CharacterAttribute> resourceAttributes = new List<CharacterAttribute>();
				List<CharacterAttribute> damageAttributes = new List<CharacterAttribute>();
				List<CharacterAttribute> resistanceAttributes = new List<CharacterAttribute>();
				List<CharacterAttribute> coreAttributes = new List<CharacterAttribute>();
				foreach (CharacterAttribute attribute in attributeController.Attributes.Values)
				{
					if (attribute is CharacterResourceAttribute || attribute.Template.Name.Contains("Regeneration"))
					{
						resourceAttributes.Add(attribute);
					}
					else if (attribute.Template is DamageAttributeTemplate)
					{
						damageAttributes.Add(attribute);
					}
					else if (attribute.Template is ResistanceAttributeTemplate)
					{
						resistanceAttributes.Add(attribute);
					}
					else
					{
						coreAttributes.Add(attribute);
					}
				}
				TMP_Text label = Instantiate(UILabel, content);
				label.text = "Resource";
				label.fontSize = 16.0f;
				label.alignment = TextAlignmentOptions.Center;
				attributeCategoryLabels.Add(label);

				foreach (CharacterAttribute core in resourceAttributes)
				{
					AddCharacterAttributeLabel(core);
				}

				label = Instantiate(UILabel, content);
				label.text = "Damage";
				label.fontSize = 16.0f;
				label.alignment = TextAlignmentOptions.Center;
				attributeCategoryLabels.Add(label);

				foreach (CharacterAttribute damage in damageAttributes)
				{
					AddCharacterAttributeLabel(damage);
				}

				label = Instantiate(UILabel, content);
				label.text = "Resistance";
				label.fontSize = 16.0f;
				label.alignment = TextAlignmentOptions.Center;
				attributeCategoryLabels.Add(label);

				foreach (CharacterAttribute resistance in resistanceAttributes)
				{
					AddCharacterAttributeLabel(resistance);
				}

				label = Instantiate(UILabel, content);
				label.text = "Core";
				label.fontSize = 16.0f;
				label.alignment = TextAlignmentOptions.Center;
				attributeCategoryLabels.Add(label);

				foreach (CharacterAttribute core in coreAttributes)
				{
					AddCharacterAttributeLabel(core);
				}

				resourceAttributes.Clear();
				damageAttributes.Clear();
				resistanceAttributes.Clear();
				coreAttributes.Clear();
			}
		}
		private void AddCharacterAttributeLabel(CharacterAttribute attribute)
		{
			attribute.OnAttributeUpdated -= OnAttributeUpdated; // just in case..
			UIAttribute label = Instantiate(AttributeLabelPrefab, content);
			label.Name.text = attribute.Template.Name;
			label.Value.text = attribute.FinalValue.ToString();
			if (attribute.Template.IsPercentage)
			{
				label.Value.text += "%";
			}
			else if (attribute.Template.IsResourceAttribute)
			{
				CharacterResourceAttribute resource = attribute as CharacterResourceAttribute;
				if (resource != null)
				{
					label.Value.text += " / " + resource.FinalValue.ToString();
				}
			}
			attributeLabels.Add(attribute.Template.Name, label);
			attribute.OnAttributeUpdated += OnAttributeUpdated;
		}


		private void SetButtonSlot(ItemContainer container, UIEquipmentButton button)
		{
			if (container == null || button == null)
			{
				return;
			}

			if (container.TryGetItem((int)button.ItemSlotType, out Item item))
			{
				// update our button display
			if (button.Icon != null)
				{
					button.Icon.sprite = item.Template.Icon;
				}
				//inventorySlots[i].cooldownText = character.CooldownController.IsOnCooldown();
				if (button.AmountText != null)
				{
					button.AmountText.text = item.IsStackable ? item.Stackable.Amount.ToString() : "";
				}
				}
			else
			{
				// clear the slot
				button.Clear();
			}
		}

		public void OnEquipmentSlotUpdated(ItemContainer container, Item item, int equipmentSlot)
		{
			if (container == null || buttons == null)
			{
				return;
			}

			if (!container.IsSlotEmpty(equipmentSlot))
			{
				// update our button display
				UIEquipmentButton button = buttons[equipmentSlot];
				if (button.Icon != null)
				{
					button.Icon.sprite = item.Template.Icon;
				}
				//inventorySlots[i].cooldownText = character.CooldownController.IsOnCooldown();
				if (button.AmountText != null)
				{
					button.AmountText.text = item.IsStackable ? item.Stackable.Amount.ToString() : "";
				}
			}
			else
			{
				// clear the slot
				buttons[equipmentSlot].Clear();
			}
		}

		public void OnAttributeUpdated(CharacterAttribute attribute)
		{
			if (attributeLabels.TryGetValue(attribute.Template.Name, out UIAttribute label))
			{
				label.Name.text = attribute.Template.Name;
				label.Value.text = attribute.FinalValue.ToString();
				if (attribute.Template.IsPercentage)
				{
					label.Value.text += "%";
				}
				else if (attribute.Template.IsResourceAttribute)
				{
					CharacterResourceAttribute resource = attribute as CharacterResourceAttribute;
					if (resource != null)
					{
						label.Value.text += " / " + resource.FinalValue.ToString();
					}
				}
			}
		}
	}
}