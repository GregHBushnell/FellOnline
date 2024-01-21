using UnityEngine;

namespace FellOnline.Shared
{
	/// <summary>
	/// We pre-load all templates at runtime so there is no jitter.
	/// These templates contain CONSTANT game data that is required for the game to run.
	/// *NOTE* DO NOT MODIFY VALUES AT RUNTIME.
	/// </summary>
	public class FResourceTemplatePreload : MonoBehaviour
	{
		private void Awake()
		{
			FBaseAbilityTemplate.LoadCache<FBaseAbilityTemplate>();
			FAbilityTemplate.LoadCache<FAbilityTemplate>();
			FAbilityEvent.LoadCache<FAbilityEvent>();
			FSpawnEvent.LoadCache<FSpawnEvent>();
			FHitEvent.LoadCache<FHitEvent>();
			FMoveEvent.LoadCache<FMoveEvent>();
			FAchievementTemplate.LoadCache<FAchievementTemplate>();
			FBuffTemplate.LoadCache<FBuffTemplate>();
			FCharacterAttributeTemplate.LoadCache<FCharacterAttributeTemplate>();
			FItemAttributeTemplate.LoadCache<FItemAttributeTemplate>();
			FBaseItemTemplate.LoadCache<FBaseItemTemplate>();
			FArmorTemplate.LoadCache<FArmorTemplate>();
			FWeaponTemplate.LoadCache<FWeaponTemplate>();
			FConsumableTemplate.LoadCache<FConsumableTemplate>();
			FScrollConsumableTemplate.LoadCache<FScrollConsumableTemplate>();
			FQuestTemplate.LoadCache<FQuestTemplate>();
			FMerchantTemplate.LoadCache<FMerchantTemplate>();
			WorldItemTemplate.LoadCache<WorldItemTemplate>();
			CharacterHairTemplate.LoadCache<CharacterHairTemplate>();
		}

		private void OnApplicationQuit()
		{
			FBaseAbilityTemplate.UnloadCache<FBaseAbilityTemplate>();
			FAbilityTemplate.UnloadCache<FAbilityTemplate>();
			FAbilityEvent.UnloadCache<FAbilityEvent>();
			FSpawnEvent.UnloadCache<FSpawnEvent>();
			FHitEvent.UnloadCache<FHitEvent>();
			FMoveEvent.UnloadCache<FMoveEvent>();
			FAchievementTemplate.UnloadCache<FAchievementTemplate>();
			FBuffTemplate.UnloadCache<FBuffTemplate>();
			FCharacterAttributeTemplate.UnloadCache<FCharacterAttributeTemplate>();
			FItemAttributeTemplate.UnloadCache<FItemAttributeTemplate>();
			FBaseItemTemplate.UnloadCache<FBaseItemTemplate>();
			FArmorTemplate.UnloadCache<FArmorTemplate>();
			FWeaponTemplate.UnloadCache<FWeaponTemplate>();
			FConsumableTemplate.UnloadCache<FConsumableTemplate>();
			FScrollConsumableTemplate.UnloadCache<FScrollConsumableTemplate>();
			FQuestTemplate.UnloadCache<FQuestTemplate>();
			FMerchantTemplate.UnloadCache<FMerchantTemplate>();
			WorldItemTemplate.UnloadCache<WorldItemTemplate>();
			CharacterHairTemplate.UnloadCache<CharacterHairTemplate>();
		}
	}
}