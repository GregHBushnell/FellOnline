using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Damage Hit Event", menuName = "FellOnline/Character/Ability/Hit Event/Damage", order = 1)]
	public sealed class DamageHitEvent : HitEvent
	{
		public int Damage;
		public DamageAttributeTemplate DamageAttributeTemplate;

		public override int Invoke(Character attacker, Character defender,Mob mob, TargetInfo hitTarget, GameObject abilityObject)
		{
			if (defender != null &&
				defender.TryGet(out CharacterDamageController damageController))
			{
				damageController.Damage(attacker, Damage, DamageAttributeTemplate);
			}
			if(mob != null && mob.TryGet(out MobDamageController mobDamageController))
			{
				mobDamageController.Damage(attacker, Damage, DamageAttributeTemplate);
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