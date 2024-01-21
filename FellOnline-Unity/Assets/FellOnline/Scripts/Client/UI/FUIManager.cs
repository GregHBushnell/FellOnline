using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using FellOnline.Shared;

namespace FellOnline.Client
{
	/// <summary>
	/// Helper class for our UI
	/// </summary>
	public static class FUIManager
	{
		private static Dictionary<string, FUIControl> controls = new Dictionary<string, FUIControl>();
		private static Dictionary<string, FUICharacterControl> characterControls = new Dictionary<string, FUICharacterControl>();
		
		private static Client _client;

		/// <summary>
		/// Dependency injection for the Client.
		/// </summary>
		internal static void SetClient(Client client)
		{
			_client = client;
		}

		internal static void SetCharacter(Character character)
		{
			foreach (FUICharacterControl control in characterControls.Values)
			{
				control.SetCharacter(character);
			}
		}

		internal static void Register(FUIControl control)
		{
			if (control == null)
			{
				return;
			}
			if (controls.ContainsKey(control.Name))
			{
				return;
			}

			// character controls are mapped separately for ease of use
			FUICharacterControl characterControl = control as FUICharacterControl;
			if (characterControl != null)
			{
				characterControls.Add(characterControl.Name, characterControl);
			}

			control.SetClient(_client);

			Debug.Log("UIManager: Registered[" + control.Name + "]");
			controls.Add(control.Name, control);
		}

		internal static void Unregister(FUIControl control)
		{
			if (control == null)
			{
				return;
			}
			else
			{
				Debug.Log("UIManager: Unregistered[" + control.Name + "]");
				controls.Remove(control.Name);
				characterControls.Remove(control.Name);
			}
		}

		public static bool TryGet<T>(string name, out T control) where T : FUIControl
		{
			if (controls.TryGetValue(name, out FUIControl result))
			{
				control = result as T;
				if (control != null)
				{
					return true;
				}
			}
			control = null;
			return false;
		}

		public static bool Exists(string name)
		{
			if (controls.ContainsKey(name))
			{
				return true;
			}
			return false;
		}

		public static void ToggleVisibility(string name)
		{
			if (controls.TryGetValue(name, out FUIControl result))
			{
				result.ToggleVisibility();
			}
		}

		public static void Show(string name)
		{
			if (controls.TryGetValue(name, out FUIControl result))
			{
				result.Show();
			}
		}

		public static void Hide(string name)
		{
			if (controls.TryGetValue(name, out FUIControl result) && result.Visible)
			{
				result.Hide();
			}
		}

		public static void HideAll()
		{
			foreach (KeyValuePair<string, FUIControl> p in controls)
			{
				p.Value.Hide();
			}
		}

		public static void ShowAll()
		{
			foreach (KeyValuePair<string, FUIControl> p in controls)
			{
				p.Value.Show();
			}
		}

		public static bool ControlHasFocus()
		{
			if (EventSystem.current.currentSelectedGameObject != null)
			{
				return true;
			}
			foreach (FUIControl control in controls.Values)
			{
				if (control.Visible &&
					control.HasFocus)
				{
					return true;
				}
			}
			return false;
		}

		public static bool InputControlHasFocus()
		{
			foreach (FUIControl control in controls.Values)
			{
				if (control.Visible &&
					control.InputField != null &&
					control.InputField.isFocused)
				{
					return true;
				}
			}
			return false;
		}
	}
}