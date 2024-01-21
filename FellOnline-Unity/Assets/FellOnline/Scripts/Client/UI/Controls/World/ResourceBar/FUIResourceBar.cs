using TMPro;
using UnityEngine.UI;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public abstract class FUIResourceBar : FUICharacterControl
	{
		public Slider slider;
		public TMP_Text resourceValue;

		public FCharacterAttributeTemplate Template;

		public override void OnPreSetCharacter()
		{
			if (Character != null &&
				Character.AttributeController != null &&
				Character.AttributeController.TryGetResourceAttribute(Template, out FCharacterResourceAttribute attribute))
			{
				attribute.OnAttributeUpdated -= CharacterAttribute_OnAttributeUpdated;
			}
		}

		public override void SetCharacter(Character character)
		{
			base.SetCharacter(character);

			if (Character != null &&
				Character.AttributeController != null &&
				Character.AttributeController.TryGetResourceAttribute(Template, out FCharacterResourceAttribute attribute))
			{
				attribute.OnAttributeUpdated += CharacterAttribute_OnAttributeUpdated;
			}
		}

		public void CharacterAttribute_OnAttributeUpdated(FCharacterAttribute attribute)
		{
			if (Character != null &&
				Character.AttributeController.TryGetResourceAttribute(Template, out FCharacterResourceAttribute resource))
			{
				float value = resource.CurrentValue / resource.FinalValue;
				if (slider != null) slider.value = value;
				if (resourceValue != null) resourceValue.text = resource.CurrentValue + "/" + resource.FinalValue;
			}
		}
	}
}