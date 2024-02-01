﻿using Cysharp.Text;
using UnityEngine;
using System.Collections.Generic;

namespace FellOnline.Shared
{
	public abstract class BaseAbilityTemplate : CachedScriptableObject<BaseAbilityTemplate>, ITooltip, ICachedObject
	{
		public Sprite icon;
		public string Description;
		public float ActivationTime;
		public float ActiveTime;
		public float Cooldown;
		public float Range;
		public float Speed;
		public long Price;
		public AbilityResourceDictionary Resources = new AbilityResourceDictionary();
		public AbilityResourceDictionary Requirements = new AbilityResourceDictionary();

		public string Name { get { return this.name; } }

		public Sprite Icon { get { return this.icon; } }

		public virtual string Tooltip()
		{
			return PrimaryTooltip(null);
		}

		public virtual string Tooltip(List<ITooltip> combineList)
		{
			return PrimaryTooltip(combineList);
		}

		public virtual string GetFormattedDescription()
		{
			return Description;
		}

		private string PrimaryTooltip(List<ITooltip> combineList)
		{
			using (var sb = ZString.CreateStringBuilder())
			{
				sb.Append(RichText.Format(Name, false, "f5ad6e", "135%"));
				if (!string.IsNullOrWhiteSpace(Description))
				{
					sb.Append("\r\n______________________________\r\n");
					sb.Append(RichText.Format(GetFormattedDescription(), true, "a66ef5FF"));
				}

				float activationTime = ActivationTime;
				float activeTime = ActiveTime;
				float cooldown = Cooldown;
				float range = Range;
				float speed = Speed;
				float price = Price;

				AbilityResourceDictionary resources = new AbilityResourceDictionary();
				resources.CopyFrom(Resources);

				AbilityResourceDictionary requirements = new AbilityResourceDictionary();
				requirements.CopyFrom(Requirements);

				if (combineList != null)
				{
					foreach (BaseAbilityTemplate template in combineList)
					{
						if (template == null ||
							string.IsNullOrWhiteSpace(template.Description))
						{
							continue;
						}
						sb.Append(RichText.Format(template.GetFormattedDescription(), true, "a66ef5FF"));

						activationTime += template.ActivationTime;
						activeTime += template.ActiveTime;
						cooldown += template.Cooldown;
						range += template.Range;
						speed += template.Speed;
						price += template.Price;

						foreach (KeyValuePair<CharacterAttributeTemplate, int> pair in template.Resources)
						{
							if (!string.IsNullOrWhiteSpace(pair.Key.Name))
							{
								resources[pair.Key] += pair.Value;
							}
						}

						foreach (KeyValuePair<CharacterAttributeTemplate, int> pair in template.Requirements)
						{
							if (!string.IsNullOrWhiteSpace(pair.Key.Name))
							{
								requirements[pair.Key] += pair.Value;
							}
						}
					}
				}
				sb.Append("\r\n______________________________\r\n");
				sb.Append(RichText.Format("Activation Time", activationTime, true, "a66ef5FF", "", "s"));
				sb.Append(RichText.Format("Active Time", activeTime, true, "a66ef5FF", "", "s"));
				sb.Append(RichText.Format("Cooldown", cooldown, true, "a66ef5FF", "", "s"));
				sb.Append(RichText.Format("Range", range, true, "a66ef5FF", "", "m"));
				sb.Append(RichText.Format("Speed", speed, true, "a66ef5FF", "", "m/s"));

				if (resources != null && resources.Count > 0)
				{
					sb.Append("\r\n______________________________\r\n");
					sb.Append("<color=#a66ef5>Resource Cost: </color>");

					foreach (KeyValuePair<CharacterAttributeTemplate, int> pair in resources)
					{
						if (!string.IsNullOrWhiteSpace(pair.Key.Name))
						{
							sb.Append(RichText.Format(pair.Key.Name, pair.Value, true, "f5ad6eFF", "", "","120%"));
						}
					}
				}
				if (requirements != null && requirements.Count > 0)
				{
					sb.Append("\r\n______________________________\r\n");
					sb.Append("<color=#a66ef5>Requirements: </color>");

					foreach (KeyValuePair<CharacterAttributeTemplate, int> pair in requirements)
					{
						if (!string.IsNullOrWhiteSpace(pair.Key.Name))
						{
							sb.Append(RichText.Format(pair.Key.Name, pair.Value, true, "f5ad6eFF", "", "", "120%"));
						}
					}
				}
				if (price > 0)
				{
					sb.Append("\r\n______________________________\r\n");
					sb.Append(RichText.Format("Price", price, true, "a66ef5FF"));
				}
				return sb.ToString();
			}
		}
	}
}