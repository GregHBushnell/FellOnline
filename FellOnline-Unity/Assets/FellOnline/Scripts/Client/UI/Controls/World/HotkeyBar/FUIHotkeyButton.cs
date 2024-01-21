using FellOnline.Shared;

namespace FellOnline.Client
{
	public class FUIHotkeyButton : FUIReferenceButton
	{
		public string KeyMap = "";

		public override void OnLeftClick()
		{
			if (FUIManager.TryGet("UIDragObject", out FUIDragObject dragObject) &&
				dragObject.Visible)
			{
				if (dragObject.Type != FReferenceButtonType.Bank)
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
			if (FUIManager.TryGet("UIDragObject", out FUIDragObject dragObject) && ReferenceID != NULL_REFERENCE_ID)
			{
				dragObject.SetReference(Icon.sprite, ReferenceID, Type);
				Clear();
			}
		}

		public void Activate()
		{
			if (Character != null && !string.IsNullOrWhiteSpace(KeyMap))
			{
				switch (Type)
				{
					case FReferenceButtonType.None:
						break;
					case FReferenceButtonType.Inventory:
						Character.InventoryController.Activate((int)ReferenceID);
						break;
					case FReferenceButtonType.Equipment:
						Character.EquipmentController.Activate((int)ReferenceID);
						break;
					case FReferenceButtonType.Bank:
						break;
					case FReferenceButtonType.Ability:
						Character.AbilityController.Activate(ReferenceID, FInputManager.GetKeyCode(KeyMap));
						break;
					default:
						return;
				};
			}
		}
	}
}