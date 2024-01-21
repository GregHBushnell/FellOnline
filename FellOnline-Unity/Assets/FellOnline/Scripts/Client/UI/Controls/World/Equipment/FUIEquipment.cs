using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class FUIEquipment :FUICharacterControl
	{
		public RectTransform content;
		public TMP_Text UILabel;
		public FUIAttribute AttributeLabelPrefab;

		public List<FUIEquipmentButton> buttons = new List<FUIEquipmentButton>();

		public List<TMP_Text> attributeCategoryLabels = new List<TMP_Text>();
		public Dictionary<string, FUIAttribute> attributeLabels = new Dictionary<string, FUIAttribute>();
		public override void OnStarting()
		{
			FUIEquipmentButton[] equipmentButtons = gameObject.GetComponentsInChildren<FUIEquipmentButton>();
			if (equipmentButtons != null)
			{
				buttons = new List<FUIEquipmentButton>();
				for (int i = 0; i < equipmentButtons.Length; ++i)
				{
					FUIEquipmentButton button = equipmentButtons[i];
					button.Type = FReferenceButtonType.Equipment;
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
				foreach (FUIAttribute obj in attributeLabels.Values)
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
				if (Character.AttributeController != null)
				{
					foreach (FCharacterAttribute attribute in Character.AttributeController.Attributes.Values)
					{
						attribute.OnAttributeUpdated -= OnAttributeUpdated;
					}
				}
				if (Character.EquipmentController != null)
				{
					Character.EquipmentController.OnSlotUpdated -= OnEquipmentSlotUpdated;
				}
			}
		}

		public override void SetCharacter(Character character)
		{
			base.SetCharacter(character);
			DestroyAttributes();

			if (buttons != null &&
				Character != null &&
				Character.EquipmentController != null)
			{
				Character.EquipmentController.OnSlotUpdated -= OnEquipmentSlotUpdated;
				foreach (FUIEquipmentButton button in buttons)
				{
					button.Character = Character;
					if (Character != null)
					{
						SetButtonSlot(Character.EquipmentController, button);
					}
				}
				Character.EquipmentController.OnSlotUpdated += OnEquipmentSlotUpdated;
			}

			if (Character != null &&
				Character.AttributeController != null)
			{
				
				List<FCharacterAttribute> resourceAttributes = new List<FCharacterAttribute>();
				List<FCharacterAttribute> damageAttributes = new List<FCharacterAttribute>();
				List<FCharacterAttribute> resistanceAttributes = new List<FCharacterAttribute>();
				List<FCharacterAttribute> coreAttributes = new List<FCharacterAttribute>();
				foreach (FCharacterAttribute attribute in Character.AttributeController.Attributes.Values)
				{
					if (attribute is FCharacterResourceAttribute || attribute.Template.Name.Contains("Regeneration"))
					{
						resourceAttributes.Add(attribute);
					}
					else if (attribute.Template is FDamageAttributeTemplate)
					{
						damageAttributes.Add(attribute);
					}
					else if (attribute.Template is FResistanceAttributeTemplate)
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

				foreach (FCharacterAttribute core in resourceAttributes)
				{
					AddCharacterAttributeLabel(core);
				}

				label = Instantiate(UILabel, content);
				label.text = "Damage";
				label.fontSize = 16.0f;
				label.alignment = TextAlignmentOptions.Center;
				attributeCategoryLabels.Add(label);

				foreach (FCharacterAttribute damage in damageAttributes)
				{
					AddCharacterAttributeLabel(damage);
				}

				label = Instantiate(UILabel, content);
				label.text = "Resistance";
				label.fontSize = 16.0f;
				label.alignment = TextAlignmentOptions.Center;
				attributeCategoryLabels.Add(label);

				foreach (FCharacterAttribute resistance in resistanceAttributes)
				{
					AddCharacterAttributeLabel(resistance);
				}

				label = Instantiate(UILabel, content);
				label.text = "Core";
				label.fontSize = 16.0f;
				label.alignment = TextAlignmentOptions.Center;
				attributeCategoryLabels.Add(label);

				foreach (FCharacterAttribute core in coreAttributes)
				{
					AddCharacterAttributeLabel(core);
				}

				resourceAttributes.Clear();
				damageAttributes.Clear();
				resistanceAttributes.Clear();
				coreAttributes.Clear();
			}
		}
		private void AddCharacterAttributeLabel(FCharacterAttribute attribute)
		{
			attribute.OnAttributeUpdated -= OnAttributeUpdated; // just in case..
			FUIAttribute label = Instantiate(AttributeLabelPrefab, content);
			label.Name.text = attribute.Template.Name;
			label.Value.text = attribute.FinalValue.ToString();
			if (attribute.Template.IsPercentage)
			{
				label.Value.text += "%";
			}
			else if (attribute.Template.IsResourceAttribute)
			{
				FCharacterResourceAttribute resource = attribute as FCharacterResourceAttribute;
				if (resource != null)
				{
					label.Value.text += " / " + resource.FinalValue.ToString();
				}
			}
			attributeLabels.Add(attribute.Template.Name, label);
			attribute.OnAttributeUpdated += OnAttributeUpdated;
		}


		private void SetButtonSlot(FItemContainer container, FUIEquipmentButton button)
		{
			if (container == null || button == null)
			{
				return;
			}

			if (container.TryGetItem((int)button.ItemSlotType, out FItem item))
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

		public void OnEquipmentSlotUpdated(FItemContainer container, FItem item, int equipmentSlot)
		{
			if (container == null || buttons == null)
			{
				return;
			}

			if (!container.IsSlotEmpty(equipmentSlot))
			{
				// update our button display
				FUIEquipmentButton button = buttons[equipmentSlot];
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

		public void OnAttributeUpdated(FCharacterAttribute attribute)
		{
			if (attributeLabels.TryGetValue(attribute.Template.Name, out FUIAttribute label))
			{
				label.Name.text = attribute.Template.Name;
				label.Value.text = attribute.FinalValue.ToString();
				if (attribute.Template.IsPercentage)
				{
					label.Value.text += "%";
				}
				else if (attribute.Template.IsResourceAttribute)
				{
					FCharacterResourceAttribute resource = attribute as FCharacterResourceAttribute;
					if (resource != null)
					{
						label.Value.text += " / " + resource.FinalValue.ToString();
					}
				}
			}
		}
	}
}