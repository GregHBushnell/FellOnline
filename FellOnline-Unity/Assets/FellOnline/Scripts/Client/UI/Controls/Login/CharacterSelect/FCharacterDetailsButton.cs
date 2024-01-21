using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class FCharacterDetailsButton : MonoBehaviour
	{
		public Button characterButton;
		public TMP_Text characterNameLabel;

		public FCharacterDetails Details;

		public delegate void CharacterSelectEvent(FCharacterDetailsButton button);
		public event CharacterSelectEvent OnCharacterSelected;

		public void Initialize(FCharacterDetails details)
		{
			Details = details;
			characterNameLabel.text = details.CharacterName;
		}

		public void OnClick_CharacterButton()
		{
			OnCharacterSelected?.Invoke(this);
		}

		public void SetLabelColors(Color color)
		{
			characterNameLabel.color = color;
		}
	}
}