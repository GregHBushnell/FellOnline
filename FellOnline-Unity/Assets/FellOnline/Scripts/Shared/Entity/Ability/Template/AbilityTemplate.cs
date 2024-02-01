﻿using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Ability", menuName = "FellOnline/Character/Ability/Ability", order = 1)]
	public class AbilityTemplate : BaseAbilityTemplate, ITooltip
	{
		public GameObject FXPrefab;
		public AbilitySpawnTarget AbilitySpawnTarget;
		public bool RequiresTarget;
		public byte EventSlots;
		public int HitCount;
		public CharacterAttributeTemplate ActivationSpeedReductionAttribute;
		public CharacterAttributeTemplate CooldownReductionAttribute;
	}
}