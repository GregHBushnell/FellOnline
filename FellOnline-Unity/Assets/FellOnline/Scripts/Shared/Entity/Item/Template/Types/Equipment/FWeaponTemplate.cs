using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Weapon", menuName = "FellOnline/Item/Weapon", order = 0)]
	public class FWeaponTemplate : FEquippableItemTemplate
	{
		public FItemAttributeTemplate AttackPower;
		public FItemAttributeTemplate AttackSpeed;
	}
}