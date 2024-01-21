using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Ability", menuName = "FellOnline/Character/Ability/Ability", order = 1)]
	public class FAbilityTemplate : FBaseAbilityTemplate, FITooltip
	{
		public GameObject FXPrefab;
		public AbilitySpawnTarget AbilitySpawnTarget;
		public bool RequiresTarget;
		public byte EventSlots;
		public int HitCount;
		public FCharacterAttributeTemplate ActivationSpeedReductionAttribute;
		public FCharacterAttributeTemplate CooldownReductionAttribute;
	}
}