using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FellOnline.Shared;

namespace FellOnline.Client
{
	/// <summary>
	/// Advanced button class used by the inventory, equipment, and hotkey buttons.
	/// </summary>
	public class FUIReferenceButton : Button
	{
		public const long NULL_REFERENCE_ID = -1;

		protected FUITooltip currentUITooltip;

		/// <summary>
		/// ReferenceID is equal to the inventory slot, equipment slot, or ability id based on Reference Type.
		/// </summary>
		public long ReferenceID = NULL_REFERENCE_ID;
		public FReferenceButtonType Type = FReferenceButtonType.None;
		[SerializeField]
		public Image Icon;
		[SerializeField]
		public TMP_Text CooldownText;
		[SerializeField]
		public TMP_Text AmountText;

		public Character Character;

		protected override void OnDisable()
		{
			base.OnDisable();

			ClearTooltip();
		}

		public virtual void OnLeftClick() { }
		public virtual void OnRightClick() { }

		public override void OnPointerEnter(PointerEventData eventData)
		{
			base.OnPointerEnter(eventData);

		ShowTooltip(Character, ReferenceID);
		}

		public virtual void ShowTooltip(Character character, long referenceID)
		{
			if (character == null ||
				referenceID < 0)
			{
				return;
			}
			switch (Type)
			{
				case FReferenceButtonType.None:
					break;
				case FReferenceButtonType.Inventory:
					if (character.InventoryController != null &&
						character.InventoryController.TryGetItem((int)referenceID, out FItem inventoryItem) &&
						FUIManager.TryGet("UITooltip", out currentUITooltip))
					{
						currentUITooltip.Open(inventoryItem.Tooltip());
					}
					break;
				case FReferenceButtonType.Equipment:
					if (character.EquipmentController != null &&
						character.EquipmentController.TryGetItem((int)referenceID, out FItem equippedItem) &&
						FUIManager.TryGet("UITooltip", out currentUITooltip))
					{
						currentUITooltip.Open(equippedItem.Tooltip());
					}
					break;
				case FReferenceButtonType.Bank:
					if (character.BankController != null &&
						character.BankController.TryGetItem((int)referenceID, out FItem bankItem) &&
						FUIManager.TryGet("UITooltip", out currentUITooltip))
					{
						currentUITooltip.Open(bankItem.Tooltip());
					}
					break;
				case FReferenceButtonType.Ability:
					if (character.AbilityController.KnownAbilities != null &&
						character.AbilityController.KnownAbilities.TryGetValue(referenceID, out FAbility ability) &&
						FUIManager.TryGet("UITooltip", out currentUITooltip))
					{
						currentUITooltip.Open(ability.Tooltip());
					}
					break;
				default:
					return;
			};
		}

		public override void OnPointerExit(PointerEventData eventData)
		{
			base.OnPointerExit(eventData);

			ClearTooltip();
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);

			if (eventData.button == PointerEventData.InputButton.Left)
			{
				OnLeftClick();
			}
			else if (eventData.button == PointerEventData.InputButton.Right)
			{
				OnRightClick();
			}
		}

		private void ClearTooltip()
		{
			if (currentUITooltip != null)
			{
				currentUITooltip.Hide();
				currentUITooltip = null;
			}
		}

		public virtual void Clear()
		{
			ReferenceID = NULL_REFERENCE_ID;
			Type = FReferenceButtonType.None;
			if (Icon != null) Icon.sprite = null;
			if (CooldownText != null) CooldownText.text = "";
			if (AmountText != null) AmountText.text = "";
			ClearTooltip();
		}
	}
}