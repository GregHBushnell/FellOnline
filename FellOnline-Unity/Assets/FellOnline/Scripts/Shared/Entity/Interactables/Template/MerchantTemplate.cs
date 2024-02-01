﻿using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Merchant", menuName = "FellOnline/Character/Merchant/Merchant", order = 1)]
	public class MerchantTemplate : CachedScriptableObject<MerchantTemplate>, ICachedObject
	{
		public Sprite icon;
		public string Description;
		public List<AbilityTemplate> Abilities;
		public List<AbilityEvent> AbilityEvents;
		public List<BaseItemTemplate> Items;

		public string Name { get { return this.name; } }

		public Sprite Icon { get { return this.icon; } }
	}
}