using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Merchant", menuName = "FellOnline/Character/Merchant/Merchant", order = 1)]
	public class FMerchantTemplate : FCachedScriptableObject<FMerchantTemplate>, FICachedObject
	{
		public Sprite icon;
		public string Description;
		public List<FAbilityTemplate> Abilities;
		public List<FAbilityEvent> AbilityEvents;
		public List<FBaseItemTemplate> Items;

		public string Name { get { return this.name; } }

		public Sprite Icon { get { return this.icon; } }
	}
}