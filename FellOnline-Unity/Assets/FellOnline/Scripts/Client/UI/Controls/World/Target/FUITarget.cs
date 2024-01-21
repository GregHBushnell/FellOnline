using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class FUITarget : FUIControl
	{
		public TMP_Text NameLabel;
		public Slider HealthSlider;
		public FCharacterAttributeTemplate HealthAttribute;

		public override void OnStarting()
		{
		}

		public override void OnDestroying()
		{
		}

		public void OnChangeTarget(GameObject obj)
		{
			if (obj == null)
			{
				// hide the UI
				Hide();
				return;
			}

			if (NameLabel != null)
			{
				NameLabel.text = obj.name;
			}
			FCharacterAttributeController characterAttributeController = obj.GetComponent<FCharacterAttributeController>();
			if (characterAttributeController != null)
			{
				if (characterAttributeController.TryGetResourceAttribute(HealthAttribute, out FCharacterResourceAttribute health))
				{
					HealthSlider.value = health.CurrentValue / health.FinalValue;
				}
			}
			else
			{
				HealthSlider.value = 0;
			}

			// make the UI visible
			Show();
		}

		public void OnUpdateTarget(GameObject obj)
		{
			if (obj == null)
			{
				// hide the UI
				Hide();
				return;
			}

			// update the health slider
			FCharacterAttributeController characterAttributeController = obj.GetComponent<FCharacterAttributeController>();
			if (characterAttributeController != null)
			{
				if (characterAttributeController.TryGetResourceAttribute(HealthAttribute, out FCharacterResourceAttribute health))
				{
					HealthSlider.value = health.CurrentValue / health.FinalValue;
				}
			}
		}
	}
}