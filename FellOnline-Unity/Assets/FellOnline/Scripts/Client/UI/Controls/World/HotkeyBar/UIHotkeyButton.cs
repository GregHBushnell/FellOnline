﻿using FellOnline.Shared;

namespace FellOnline.Client
{
	public class UIHotkeyButton : UIReferenceButton
	{
		public string KeyMap = "";

		public override void OnLeftClick()
		{
			if (UIManager.TryGet("UIDragObject", out UIDragObject dragObject) &&
				dragObject.Visible)
			{
				if (dragObject.Type != ReferenceButtonType.Bank)
				{
					Type = dragObject.Type;
					ReferenceID = dragObject.ReferenceID;
					if (Icon != null)
					{
						Icon.sprite = dragObject.Icon.sprite;
					}
				}

				// clear the drag object no matter what
				dragObject.Clear();
			}
			else
			{
				Activate();
			}
		}

		public override void OnRightClick()
		{
			if (UIManager.TryGet("UIDragObject", out UIDragObject dragObject) && ReferenceID != NULL_REFERENCE_ID)
			{
				dragObject.SetReference(Icon.sprite, ReferenceID, Type);
				Clear();
			}
		}

		public void Activate()
		{
			if (Character != null && !string.IsNullOrWhiteSpace(KeyMap))
			{
				if (Character.TryGet(out EquipmentController equipmentController) && Character.TryGet(out AbilityController abilityController)&&(Character.TryGet(out InventoryController inventoryController)))
				{
				switch (Type)
				{
					case ReferenceButtonType.None:
						break;
					case ReferenceButtonType.Inventory:
							inventoryController.Activate((int)ReferenceID);
						break;
					case ReferenceButtonType.Equipment:
							equipmentController.Activate((int)ReferenceID);
							abilityController.Activate(ReferenceID, InputManager.GetKeyCode(KeyMap));
						break;
					case ReferenceButtonType.Bank:
						break;
					case ReferenceButtonType.Ability:
							abilityController.Activate(ReferenceID, InputManager.GetKeyCode(KeyMap));
						break;
					default:
						return;
				};
				}
			}
		}
	}
}