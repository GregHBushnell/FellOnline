using FellOnline.Shared;

namespace FellOnline.Client
{
	public class UIBankButton : FUIReferenceButton
	{
		public override void OnLeftClick()
		{
			if (FUIManager.TryGet("UIDragObject", out FUIDragObject dragObject))
			{
				if (Character != null)
				{
					if (dragObject.Visible)
					{
						// we check the hotkey type because we can swap items in the bank
						if (dragObject.Type == FReferenceButtonType.Bank)
						{
							// swap item slots in the bank
							Character.BankController.SendSwapItemSlotsRequest((int)dragObject.ReferenceID, (int)ReferenceID, InventoryType.Bank);
						}
						// taking an item from inventory and putting it in this bank slot
						else if (dragObject.Type == FReferenceButtonType.Inventory)
						{
							// swap item slots in the bank
							Character.BankController.SendSwapItemSlotsRequest((int)dragObject.ReferenceID, (int)ReferenceID, InventoryType.Inventory);
						}
						// we can also unequip items
						else if (dragObject.Type == FReferenceButtonType.Equipment &&
								 dragObject.ReferenceID >= byte.MinValue && // Equipment slot index is a byte, validate here
								 dragObject.ReferenceID <= byte.MaxValue)
						{
							// unequip the item
							Character.EquipmentController.SendUnequipRequest((byte)dragObject.ReferenceID, InventoryType.Bank);
						}

						// clear the drag object
						dragObject.Clear();
					}
					else
					{
						if (!Character.BankController.IsSlotEmpty((int)ReferenceID))
						{
							dragObject.SetReference(Icon.sprite, ReferenceID, Type);
						}
					}
				}
			}
		}

		public override void Clear()
		{
			if (Icon != null) Icon.sprite = null;
			if (CooldownText != null) CooldownText.text = "";
			if (AmountText != null) AmountText.text = "";
		}
	}
}