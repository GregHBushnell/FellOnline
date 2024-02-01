﻿using UnityEngine;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class LocalInputController : MonoBehaviour
	{
#if !UNITY_SERVER
		public Character Character { get; private set; }

		public void Initialize(Character character)
		{
			Character = character;
		}

		private void OnEnable()
		{
			UIManager.Show("UIHealthBar");
			UIManager.Show("UIManaBar");
			UIManager.Show("UIStaminaBar");
			UIManager.Show("UIHotkeyBar");
			UIManager.Show("UIChat");
		}

		private void OnDisable()
		{
			UIManager.Hide("UIHealthBar");
			UIManager.Hide("UIManaBar");
			UIManager.Hide("UIStaminaBar");
			UIManager.Hide("UIHotkeyBar");
			UIManager.Hide("UIChat");
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
				UIManager.InputControlHasFocus())
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
			if (
				InputManager.GetKeyDown("Interact") &&
				Character.TryGet(out TargetController targetController))
			{
				Transform target = targetController.Current.Target;
				if (target != null)
				{
					IInteractable interactable = target.GetComponent<IInteractable>();
					if (interactable != null)
					{
						Debug.Log("Interacting with " + target.name + "");
						interactable.OnInteract(Character);
					}
				}
			}
			else // UI windows should be able to open/close freely
			{
				if (InputManager.GetKeyDown("Inventory"))
				{
					UIManager.ToggleVisibility("UIInventory");
				}

				if (InputManager.GetKeyDown("Abilities"))
				{
					UIManager.ToggleVisibility("UIAbilities");
				}

				if (InputManager.GetKeyDown("Equipment"))
				{
					UIManager.ToggleVisibility("UIEquipment");
				}

				if (InputManager.GetKeyDown("Guild"))
				{
					UIManager.ToggleVisibility("UIGuild");
				}

				if (InputManager.GetKeyDown("Party"))
				{
					UIManager.ToggleVisibility("UIParty");
				}

				if (InputManager.GetKeyDown("Friends"))
				{
					UIManager.ToggleVisibility("UIFriendList");

				}
				if (InputManager.GetKeyDown("Menu"))
				{
					UIManager.ToggleVisibility("UIMenu");
				}
			}
		}
#endif
	}
}