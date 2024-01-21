using FellOnline.Shared;
using TMPro;

namespace FellOnline.Client
{
	public class UIAbilityButton : FUIReferenceButton
	{
		public TMP_Text DescriptionLabel;

		public override void OnLeftClick()
		{
			if (FUIManager.TryGet("UIDragObject", out FUIDragObject dragObject))
			{
				if (Character != null)
				{
					if (dragObject.Visible)
					{
						dragObject.Clear();
					}
					else if (Character.AbilityController.KnownAbilities.ContainsKey(ReferenceID))
					{
						dragObject.SetReference(Icon.sprite, ReferenceID, Type);
					}
				}
			}
		}

		public override void ShowTooltip(Character character, long referenceID)
		{
			// do nothing
		}

		public override void Clear()
		{
			if (Icon != null) Icon.sprite = null;
			if (CooldownText != null) CooldownText.text = "";
			if (AmountText != null) AmountText.text = "";
			if (DescriptionLabel != null) DescriptionLabel.text = "";
		}
	}
}