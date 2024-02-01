using System.Collections.Generic;
using UnityEngine;
using FellOnline.Shared;

namespace FellOnline.Client
{
	public class UIEquipmentHotkeyBar : UICharacterControl
	{
		private const int MAX_EQ_HOTKEYS = 2;

		public RectTransform eqParent;
        public UIHotkeyButton buttonPrefab;
		public List<UIHotkeyButton> EQHotkeys = new List<UIHotkeyButton>();

		public override void OnStarting()
		{
			AddEQHotkeys(MAX_EQ_HOTKEYS);
            Debug.Log("EQHotkeys: " + EQHotkeys.Count);
		}
        public override void SetCharacter(Character character)
		{
			base.SetCharacter(character);
              Debug.Log("Character Set: " + character);
			foreach (UIHotkeyButton hotkey in EQHotkeys)
			{
				if (hotkey != null)
				{
					hotkey.Character = character;
				}
			}
		}
		void Update()
		{
			ValidateHotkeys();
			UpdateInput();
		}

		/// <summary>
		/// Get our hotkey virtual key code. Offset by 1.
		/// </summary>
		public static string GetEQHotkeyIndexKeyMap(int hotkeyIndex)
		{
			switch (hotkeyIndex)
			{
				case 0:
					return "Hotkey Primary";
				case 1:
					return "Hotkey Secondary";
				default:
					return "";
			}
		}

		/// <summary>
		/// Validates all the hotkeys. If an item in your inventory/equipment moves while it's on a hotkey slot it will remove the hotkey.
		/// </summary>
		private void ValidateHotkeys()
		{
			if (Character == null) return;

			for (int i = 0; i < EQHotkeys.Count; ++i)
			{
				if (EQHotkeys[i] == null) continue;

				switch (EQHotkeys[i].Type)
				{
					case ReferenceButtonType.None:
						break;
					case ReferenceButtonType.Inventory:
						if (Character.TryGet(out InventoryController inventoryController) &&
							inventoryController.IsSlotEmpty((int)EQHotkeys[i].ReferenceID))
						{
							EQHotkeys[i].Clear();
						}
						break;
					case ReferenceButtonType.Equipment:
						if (Character.TryGet(out EquipmentController equipmentController) &&
							equipmentController.IsSlotEmpty((int)EQHotkeys[i].ReferenceID))
						{
							EQHotkeys[i].Clear();
						}
						break;
					case ReferenceButtonType.Ability:
						if (Character.TryGet(out AbilityController abilityController) &&
							!abilityController.KnownAbilities.ContainsKey(EQHotkeys[i].ReferenceID))
						{
							EQHotkeys[i].Clear();
						}
						break;
					default:
						break;
				}
			}
		}

		private void UpdateInput()
		{
			if (Character == null || EQHotkeys == null || EQHotkeys.Count < 1)
				return;

			for (int i = 0; i < EQHotkeys.Count; ++i)
			{
				string keyMap = GetEQHotkeyIndexKeyMap(i);
				if (string.IsNullOrWhiteSpace(keyMap)) return;

				if (EQHotkeys[i] != null && InputManager.GetKey(keyMap))
				{
					EQHotkeys[i].Activate();
					return;
				}
			}
		}

		public void AddEQHotkeys(int amount)
		{
			if (eqParent == null || buttonPrefab == null) return;


			for (int i = 0; i < amount && i < MAX_EQ_HOTKEYS; ++i)
			{
				UIHotkeyButton button = Instantiate(buttonPrefab, eqParent);
				button.Character = Character;
				button.KeyMap = GetEQHotkeyIndexKeyMap(i);
				button.ReferenceID = UIReferenceButton.NULL_REFERENCE_ID;
				button.Type = ReferenceButtonType.None;
				EQHotkeys.Add(button);
			}
		}
	}
}