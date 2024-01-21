using TMPro;
using UnityEngine;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class FUIChatMessage : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler
	{
		public ChatChannel Channel;
		public TMP_Text CharacterName;
		public TMP_Text Text;
	}
}