﻿using UnityEngine;
using UnityEngine.UI;
using FellOnline.Shared;
using System.Collections.Generic;

namespace FellOnline.Client
{
	public class UIAbilities : UICharacterControl
	{
		public RectTransform AbilityParent;
		public UIAbilityButton AbilityButtonPrefab;

		public Button AbilitiesButton;
		public Button KnownAbilitiesButton;
		public Button KnownAbilityEventsButton;

		private List<UIAbilityButton> Abilities;
		private List<UIAbilityButton> KnownAbilities;
		private List<UIAbilityButton> KnownAbilityEvents;

		private AbilityTabType CurrentTab = AbilityTabType.Ability;

		public override void OnDestroying()
		{
			ClearAllSlots();
		}

		public void AddAbility(long id, Ability ability)
		{
			if (ability == null)
			{
				return;
			}

			InstantiateButton(id, ability.Template.Icon, ReferenceButtonType.Ability, AbilityTabType.Ability, ability.Tooltip(), ref Abilities);
		}

		public void AddKnownAbility(long id, BaseAbilityTemplate template)
		{
			AbilityEvent abilityEvent = template as AbilityEvent;
			if (abilityEvent != null)
			{
				InstantiateButton(id, template.Icon, ReferenceButtonType.None, AbilityTabType.KnownAbilityEvent, template.Tooltip(), ref KnownAbilityEvents);
			}
			else
			{
				AbilityTemplate abilityTemplate = template as AbilityTemplate;
				if (abilityTemplate != null)
				{
					InstantiateButton(id, template.Icon, ReferenceButtonType.None, AbilityTabType.KnownAbility, template.Tooltip(), ref KnownAbilities);
				}
			}
		}

		private void InstantiateButton(long id, Sprite icon, ReferenceButtonType buttonType, AbilityTabType tabType, string toolTip, ref List<UIAbilityButton> container)
		{
			UIAbilityButton button = Instantiate(AbilityButtonPrefab, AbilityParent);
			button.Character = Character;
			button.ReferenceID = id;
			button.Type = buttonType;
			if (button.DescriptionLabel != null)
			{
				button.DescriptionLabel.text = toolTip;
			}
			if (button.Icon != null)
			{
				button.Icon.sprite = icon;
			}
			if (container == null)
			{
				container = new List<UIAbilityButton>();
			}
			container.Add(button);
			button.gameObject.SetActive(CurrentTab == tabType ? true : false);
		}

		private void ClearAllSlots()
		{
			ClearSlots(ref Abilities);
			ClearSlots(ref KnownAbilities);
			ClearSlots(ref KnownAbilityEvents);
		}

		private void ClearSlots(ref List<UIAbilityButton> slots)
		{
			if (slots != null)
			{
				for (int i = 0; i < slots.Count; ++i)
				{
					if (slots[i] == null)
					{
						continue;
					}
					if (slots[i].gameObject != null)
					{
						slots[i].Clear();
						Destroy(slots[i].gameObject);
					}
				}
				slots.Clear();
			}
		}

		public void Tab_OnClick(int type)
		{
			CurrentTab = (AbilityTabType)type;
			switch (CurrentTab)
			{
				case AbilityTabType.Ability:
					ShowEntries(Abilities);
					ShowEntries(KnownAbilities, false);
					ShowEntries(KnownAbilityEvents, false);
					break;
				case AbilityTabType.KnownAbility:
					ShowEntries(Abilities, false);
					ShowEntries(KnownAbilities);
					ShowEntries(KnownAbilityEvents, false);
					break;
				case AbilityTabType.KnownAbilityEvent:
					ShowEntries(Abilities, false);
					ShowEntries(KnownAbilities, false);
					ShowEntries(KnownAbilityEvents);
					break;
				default: return;
			}
		}

		private void ShowEntries(List<UIAbilityButton> buttons, bool show = true)
		{
			if (buttons == null ||
				buttons.Count < 1)
			{
				return;
			}
			foreach (UIAbilityButton button in buttons)
			{
				button.gameObject.SetActive(show);
			}
		}
	}
}