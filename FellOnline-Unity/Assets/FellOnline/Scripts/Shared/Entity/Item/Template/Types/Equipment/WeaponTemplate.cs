using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Weapon", menuName = "FellOnline/Character/Item/Weapon", order = 1)]
	public class WeaponTemplate : BaseWeaponTemplate
	{
		public GameObject FXPrefab;
		public bool RequiresTarget;
		public byte EventSlots;
		public int HitCount;
		public CharacterAttributeTemplate ActivationSpeedReductionAttribute;
		public CharacterAttributeTemplate CooldownReductionAttribute;
        public ItemAttributeTemplate AttackPower;
		public ItemAttributeTemplate AttackSpeed;
	}
}