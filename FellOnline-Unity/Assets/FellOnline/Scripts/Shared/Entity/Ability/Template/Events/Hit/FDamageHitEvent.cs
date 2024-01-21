using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Damage Hit Event", menuName = "FellOnline/Character/Ability/Hit Event/Damage", order = 1)]
	public sealed class FDamageHitEvent : FHitEvent
	{
		public int Damage;
		public FDamageAttributeTemplate DamageAttributeTemplate;

		public override int Invoke(Character attacker, Character defender, FTargetInfo hitTarget, GameObject abilityObject)
		{
			if (defender != null && defender.DamageController != null)
			{
				defender.DamageController.Damage(attacker, Damage, DamageAttributeTemplate);
			}
			return 1;
		}

		public override string GetFormattedDescription()
		{
			return Description.Replace("$DAMAGE$", "<size=125%><color=#" + DamageAttributeTemplate.DisplayColor.ToHex() + ">" + Damage + "</color></size>")
							  .Replace("$ELEMENT$", "<size=125%><color=#" + DamageAttributeTemplate.DisplayColor.ToHex() + ">" + DamageAttributeTemplate.Name + "</color></size>");
		}
	}
}