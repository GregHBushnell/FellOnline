using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Buff Hit Event", menuName = "FellOnline/Character/Ability/Hit Event/Buff", order = 1)]
	public sealed class BuffHitEvent : HitEvent
	{
		public int Stacks;
		public BuffTemplate BuffTemplate;

		public override int Invoke(Character attacker, Character defender,Mob mob, TargetInfo hitTarget, GameObject abilityObject)
		{
			if (defender != null &&
				defender.TryGet(out BuffController buffController))
			{
				buffController.Apply(BuffTemplate);
			}
			// if(mob != null && mob.TryGet(out BuffController mobBuffController))
			// {
			// 	mobBuffController.Apply(BuffTemplate);
			// }

			// a buff or debuff does not count as a hit so we return 0
			return 0;
		}

		public override string GetFormattedDescription()
		{
		return Description.Replace("$BUFF$", BuffTemplate.Name)
							  .Replace("$STACKS$", Stacks.ToString());
		}
	}
}