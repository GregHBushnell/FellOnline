using TMPro;
using UnityEngine;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class UIChatMessage : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler
	{
		public ChatChannel Channel;
		public TMP_Text CharacterName;
		public TMP_Text Text;
	}
}