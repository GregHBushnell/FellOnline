﻿using UnityEngine;

namespace FellOnline.Shared
{
	/// <summary>
	/// We pre-load all templates at runtime so there is no jitter.
	/// These templates contain CONSTANT game data that is required for the game to run.
	/// *NOTE* DO NOT MODIFY VALUES AT RUNTIME.
	/// </summary>
	public class ResourceTemplatePreload : MonoBehaviour
	{
		private void Awake()
		{
			BaseAbilityTemplate.LoadCache<BaseAbilityTemplate>();
			AbilityTemplate.LoadCache<AbilityTemplate>();
			AbilityEvent.LoadCache<AbilityEvent>();
			SpawnEvent.LoadCache<SpawnEvent>();
			HitEvent.LoadCache<HitEvent>();
			MoveEvent.LoadCache<MoveEvent>();
			AchievementTemplate.LoadCache<AchievementTemplate>();
			BuffTemplate.LoadCache<BuffTemplate>();
			CharacterAttributeTemplate.LoadCache<CharacterAttributeTemplate>();
			MobAttributeTemplate.LoadCache<MobAttributeTemplate>();
			ItemAttributeTemplate.LoadCache<ItemAttributeTemplate>();
			BaseItemTemplate.LoadCache<BaseItemTemplate>();
			BaseWeaponTemplate.LoadCache<BaseWeaponTemplate>();
			//ArmorTemplate.LoadCache<ArmorTemplate>();
			WeaponTemplate.LoadCache<WeaponTemplate>();
			ConsumableTemplate.LoadCache<ConsumableTemplate>();
			ScrollConsumableTemplate.LoadCache<ScrollConsumableTemplate>();
			QuestTemplate.LoadCache<QuestTemplate>();
			MerchantTemplate.LoadCache<MerchantTemplate>();
			WorldItemTemplate.LoadCache<WorldItemTemplate>();
			CharacterHairTemplate.LoadCache<CharacterHairTemplate>();
		}

		private void OnApplicationQuit()
		{
			BaseAbilityTemplate.UnloadCache<BaseAbilityTemplate>();
			AbilityTemplate.UnloadCache<AbilityTemplate>();
			AbilityEvent.UnloadCache<AbilityEvent>();
			SpawnEvent.UnloadCache<SpawnEvent>();
			HitEvent.UnloadCache<HitEvent>();
			MoveEvent.UnloadCache<MoveEvent>();
			AchievementTemplate.UnloadCache<AchievementTemplate>();
			BuffTemplate.UnloadCache<BuffTemplate>();
			CharacterAttributeTemplate.UnloadCache<CharacterAttributeTemplate>();
			MobAttributeTemplate.UnloadCache<MobAttributeTemplate>();
			ItemAttributeTemplate.UnloadCache<ItemAttributeTemplate>();
			BaseItemTemplate.UnloadCache<BaseItemTemplate>();
			BaseWeaponTemplate.UnloadCache<BaseWeaponTemplate>();
			//ArmorTemplate.UnloadCache<ArmorTemplate>();
			WeaponTemplate.UnloadCache<WeaponTemplate>();
			ConsumableTemplate.UnloadCache<ConsumableTemplate>();
			ScrollConsumableTemplate.UnloadCache<ScrollConsumableTemplate>();
			QuestTemplate.UnloadCache<QuestTemplate>();
			MerchantTemplate.UnloadCache<MerchantTemplate>();
			WorldItemTemplate.UnloadCache<WorldItemTemplate>();
			CharacterHairTemplate.UnloadCache<CharacterHairTemplate>();
		}
	}
}