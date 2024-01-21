using UnityEngine;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class FLocalInputController : MonoBehaviour
	{
#if !UNITY_SERVER
		public Character Character { get; private set; }

		public void Initialize(Character character)
		{
			Character = character;
		}

		private void OnEnable()
		{
			FUIManager.Show("UIHealthBar");
			FUIManager.Show("UIManaBar");
			FUIManager.Show("UIStaminaBar");
			FUIManager.Show("UIHotkeyBar");
			FUIManager.Show("UIChat");
		}

		private void OnDisable()
		{
			FUIManager.Hide("UIHealthBar");
			FUIManager.Hide("UIManaBar");
			FUIManager.Hide("UIStaminaBar");
			FUIManager.Hide("UIHotkeyBar");
			FUIManager.Hide("UIChat");
		}

		private void Update()
		{
			UpdateInput();
		}

		/// <summary>
		/// We handle UI input here because we completely disable UI elements when toggling visibility.
		/// </summary>
		private void UpdateInput()
		{
			// if an input has focus we should skip input otherwise things will happen while we are typing!
			if (Character == null ||
				FUIManager.InputControlHasFocus())
			{
				return;
			}

			// mouse mode can toggle at any time other than input focus
			// if (FInputManager.GetKeyDown("Mouse Mode"))
			// {
			// 	FInputManager.ToggleMouseMode();
			// }
			
			// we can interact with things as long as the UI doesn't have focus
			//!FUIManager.ControlHasFocus() && 
			if (FInputManager.GetKeyDown("Interact"))
			{
				Transform target = Character.TargetController.Current.Target;
				if (target != null)
				{
					FIInteractable interactable = target.GetComponent<FIInteractable>();
					if (interactable != null)
					{
						Debug.Log("Interacting with " + target.name + "");
						interactable.OnInteract(Character);
					}
				}
			}
			else // UI windows should be able to open/close freely
			{
				if (FInputManager.GetKeyDown("Inventory"))
				{
					FUIManager.ToggleVisibility("UIInventory");
				}

				if (FInputManager.GetKeyDown("Abilities"))
				{
					FUIManager.ToggleVisibility("UIAbilities");
				}

				if (FInputManager.GetKeyDown("Equipment"))
				{
					FUIManager.ToggleVisibility("UIEquipment");
				}

				if (FInputManager.GetKeyDown("Guild"))
				{
					FUIManager.ToggleVisibility("UIGuild");
				}

				if (FInputManager.GetKeyDown("Party"))
				{
					FUIManager.ToggleVisibility("UIParty");
				}

				if (FInputManager.GetKeyDown("Friends"))
				{
					FUIManager.ToggleVisibility("UIFriendList");

				}
				if (FInputManager.GetKeyDown("Menu"))
				{
					FUIManager.ToggleVisibility("UIMenu");
				}
			}
		}
#endif
	}
}