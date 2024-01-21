using System.Collections.Generic;


namespace FellOnline.Shared
{
	public class FAbility
	{
		public long ID;
		public float ActivationTime;
		public float ActiveTime;
		public float Cooldown;
		public float Range;
		public float Speed;

		public FAbilityTemplate Template { get; private set; }
		public string Name { get; set; }
		public string CachedTooltip { get; private set; }
		public FAbilityResourceDictionary Resources { get; private set; }
		public FAbilityResourceDictionary Requirements { get; private set; }

		public Dictionary<int, FAbilityEvent> AbilityEvents { get; private set; }
		public Dictionary<int, FSpawnEvent> PreSpawnEvents { get; private set; }
		public Dictionary<int, FSpawnEvent> SpawnEvents { get; private set; }
		public Dictionary<int, FMoveEvent> MoveEvents { get; private set; }
		public Dictionary<int, FHitEvent> HitEvents { get; private set; }

		/// <summary>
		/// Cache of all active ability Objects. <ContainerID, <AbilityObjectID, AbilityObject>>
		/// </summary>
		public Dictionary<int, Dictionary<int, FAbilityObject>> Objects { get; set; }

		public int TotalResourceCost
		{
			get
			{
				int totalCost = 0;
				foreach (int cost in Resources.Values)
				{
					totalCost += cost;
				}
				return totalCost;
			}
		}

		public FAbility(FAbilityTemplate template, List<int> abilityEvents = null)
		{
			ID = -1;
			Template = template;
			Name = Template.Name;
			CachedTooltip = null;

			InternalAddTemplateModifiers(Template);

			if (abilityEvents != null)
			{
				for (int i = 0; i < abilityEvents.Count; ++i)
				{
					FAbilityEvent abilityEvent = FAbilityEvent.Get<FAbilityEvent>(abilityEvents[i]);
					if (abilityEvent == null)
					{
						continue;
					}
					AddAbilityEvent(abilityEvent);
				}
			}
		}

		public FAbility(long abilityID, int templateID, List<int> abilityEvents = null)
		{
			ID = abilityID;
			Template = FAbilityTemplate.Get<FAbilityTemplate>(templateID);
			Name = Template.Name;
			CachedTooltip = null;

			InternalAddTemplateModifiers(Template);

			if (abilityEvents != null)
			{
				for (int i = 0; i < abilityEvents.Count; ++i)
				{
					FAbilityEvent abilityEvent = FAbilityEvent.Get<FAbilityEvent>(abilityEvents[i]);
					if (abilityEvent == null)
					{
						continue;
					}
					AddAbilityEvent(abilityEvent);
				}
			}
		}

		public FAbility(long abilityID, FAbilityTemplate template, List<int> abilityEvents = null)
		{
			ID = abilityID;
			Template = template;
			Name = Template.Name;
			CachedTooltip = null;

			InternalAddTemplateModifiers(Template);

			if (abilityEvents != null)
			{
				for (int i = 0; i < abilityEvents.Count; ++i)
				{
					FAbilityEvent abilityEvent = FAbilityEvent.Get<FAbilityEvent>(abilityEvents[i]);
					if (abilityEvent == null)
					{
						continue;
					}
					AddAbilityEvent(abilityEvent);
				}
			}
		}

		internal void InternalAddTemplateModifiers(FAbilityTemplate template)
		{
			ActivationTime += template.ActivationTime;
			ActiveTime += template.ActiveTime;
			Cooldown += template.Cooldown;
			Range += template.Range;
			Speed += template.Speed;

			if (Resources == null)
			{
				Resources = new FAbilityResourceDictionary();
			}

			if (Requirements == null)
			{
				Requirements = new FAbilityResourceDictionary();
			}

			foreach (KeyValuePair<FCharacterAttributeTemplate, int> pair in template.Resources)
			{
				if (!Resources.ContainsKey(pair.Key))
				{
					Resources[pair.Key] = pair.Value;
				}
				else
				{
					Resources[pair.Key] += pair.Value;
				}
			}

			foreach (KeyValuePair<FCharacterAttributeTemplate, int> pair in template.Requirements)
			{
				if (!Requirements.ContainsKey(pair.Key))
				{
					Requirements[pair.Key] = pair.Value;
				}
				else
				{
					Requirements[pair.Key] += pair.Value;
				}
			}
		}

		public bool TryGetAbilityEvent<T>(int templateID, out T modifier) where T : FAbilityEvent
		{
			if (AbilityEvents.TryGetValue(templateID, out FAbilityEvent result))
			{
				if ((modifier = result as T) != null)
				{
					return true;
				}
			}
			modifier = null;
			return false;
		}

		public bool HasAbilityEvent(int templateID)
		{
			return AbilityEvents.ContainsKey(templateID);
		}

		public void AddAbilityEvent(FAbilityEvent abilityEvent)
		{
			if (AbilityEvents == null)
			{
				AbilityEvents = new Dictionary<int, FAbilityEvent>();
			}
			if (!AbilityEvents.ContainsKey(abilityEvent.ID))
			{
				CachedTooltip = null;

				AbilityEvents.Add(abilityEvent.ID, abilityEvent);

				FSpawnEvent spawnEvent = abilityEvent as FSpawnEvent;
				if (spawnEvent != null)
				{
					if (PreSpawnEvents == null)
					{
						PreSpawnEvents = new Dictionary<int, FSpawnEvent>();
					}
					if (SpawnEvents == null)
					{
						SpawnEvents = new Dictionary<int, FSpawnEvent>();
					}
					switch (spawnEvent.SpawnEventType)
					{
						case SpawnEventType.OnPreSpawn:
							if (!PreSpawnEvents.ContainsKey(spawnEvent.ID))
							{
								PreSpawnEvents.Add(spawnEvent.ID, spawnEvent);
							}
							break;
						case SpawnEventType.OnSpawn:
							if (!SpawnEvents.ContainsKey(spawnEvent.ID))
							{
								SpawnEvents.Add(spawnEvent.ID, spawnEvent);
							}
							break;
						default:
							break;
					}
				}
				else
				{
					FHitEvent hitEvent = abilityEvent as FHitEvent;
					if (hitEvent != null)
					{
						if (HitEvents == null)
						{
							HitEvents = new Dictionary<int, FHitEvent>();
						}
						HitEvents.Add(abilityEvent.ID, hitEvent);
					}
					else
					{
						FMoveEvent moveEvent = abilityEvent as FMoveEvent;
						if (moveEvent != null)
						{
							if (MoveEvents == null)
							{
								MoveEvents = new Dictionary<int, FMoveEvent>();
							}
							MoveEvents.Add(abilityEvent.ID, moveEvent);
						}
					}
				}

				ActivationTime += abilityEvent.ActivationTime;
				ActiveTime += abilityEvent.ActiveTime;
				Cooldown += abilityEvent.Cooldown;
				Range += abilityEvent.Range;
				Speed += abilityEvent.Speed;
				foreach (KeyValuePair<FCharacterAttributeTemplate, int> pair in abilityEvent.Resources)
				{
					if (!Resources.ContainsKey(pair.Key))
					{
						Resources[pair.Key] = pair.Value;
					}
					else
					{
						Resources[pair.Key] += pair.Value;
					}
				}
				foreach (KeyValuePair<FCharacterAttributeTemplate, int> pair in abilityEvent.Requirements)
				{
					if (!Requirements.ContainsKey(pair.Key))
					{
						Requirements[pair.Key] = pair.Value;
					}
					else
					{
						Requirements[pair.Key] += pair.Value;
					}
				}
			}
		}

		public void RemoveAbilityEvent(FAbilityEvent abilityEvent)
		{
			if (AbilityEvents.ContainsKey(abilityEvent.ID))
			{
				CachedTooltip = null;

				AbilityEvents.Remove(abilityEvent.ID);

				FSpawnEvent spawnEvent = abilityEvent as FSpawnEvent;
				if (spawnEvent != null)
				{
					switch (spawnEvent.SpawnEventType)
					{
						case SpawnEventType.OnPreSpawn:
							PreSpawnEvents.Remove(spawnEvent.ID);
							break;
						case SpawnEventType.OnSpawn:
							SpawnEvents.Remove(spawnEvent.ID);
							break;
						default:
							break;
					}
				}
				else
				{
					FHitEvent hitEvent = abilityEvent as FHitEvent;
					if (hitEvent != null)
					{
						HitEvents.Remove(abilityEvent.ID);
					}
					else
					{
						FMoveEvent moveEvent = abilityEvent as FMoveEvent;
						if (moveEvent != null)
						{
							MoveEvents.Remove(abilityEvent.ID);
						}
					}
				}

				ActivationTime -= abilityEvent.ActivationTime;
				ActiveTime -= abilityEvent.ActiveTime;
				Cooldown -= abilityEvent.Cooldown;
				Range -= abilityEvent.Range;
				Speed -= abilityEvent.Speed;
				foreach (KeyValuePair<FCharacterAttributeTemplate, int> pair in abilityEvent.Resources)
				{
					if (Resources.ContainsKey(pair.Key))
					{
						Resources[pair.Key] -= pair.Value;
					}
				}
				foreach (KeyValuePair<FCharacterAttributeTemplate, int> pair in abilityEvent.Requirements)
				{
					if (Requirements.ContainsKey(pair.Key))
					{
						Requirements[pair.Key] += pair.Value;
					}
				}
			}
		}

		public bool MeetsRequirements(Character character)
		{
			foreach (KeyValuePair<FCharacterAttributeTemplate, int> pair in Requirements)
			{
				if (!character.AttributeController.TryGetResourceAttribute(pair.Key.ID, out FCharacterResourceAttribute requirement) ||
					requirement.CurrentValue < pair.Value)
				{
					return false;
				}
			}
			return true;
		}

		public bool HasResource(Character character, FAbilityEvent bloodResourceConversion, FCharacterAttributeTemplate bloodResource)
		{
			if (AbilityEvents != null &&
				bloodResourceConversion != null &&
				bloodResource != null &&
				AbilityEvents.ContainsKey(bloodResourceConversion.ID))
			{
				int totalCost = TotalResourceCost;

				FCharacterResourceAttribute resource;
				if (!character.AttributeController.TryGetResourceAttribute(bloodResource.ID, out resource) ||
					resource.CurrentValue < totalCost)
				{
					return false;
				}
			}
			else
			{
				foreach (KeyValuePair<FCharacterAttributeTemplate, int> pair in Resources)
				{
					FCharacterResourceAttribute resource;
					if (!character.AttributeController.TryGetResourceAttribute(pair.Key.ID, out resource) ||
						resource.CurrentValue < pair.Value)
					{
						return false;
					}
				}
			}
			return true;
		}

		public void ConsumeResources(FCharacterAttributeController attributeController, FAbilityEvent bloodResourceConversion, FCharacterAttributeTemplate bloodResource)
		{
			if (AbilityEvents != null &&
				bloodResourceConversion != null &&
				bloodResource != null &&
				AbilityEvents.ContainsKey(bloodResourceConversion.ID))
			{
				int totalCost = TotalResourceCost;

				FCharacterResourceAttribute resource;
				if (bloodResource != null && attributeController.TryGetResourceAttribute(bloodResource.ID, out resource) &&
					resource.CurrentValue >= totalCost)
				{
					resource.Consume(totalCost);
				}
			}
			else
			{
				foreach (KeyValuePair<FCharacterAttributeTemplate, int> pair in Resources)
				{
					FCharacterResourceAttribute resource;
					if (attributeController.TryGetResourceAttribute(pair.Key.ID, out resource) &&
						resource.CurrentValue < pair.Value)
					{
						resource.Consume(pair.Value);
					}
				}
			}
		}

		public void RemoveAbilityObject(int containerID, int objectID)
		{
			if (Objects.TryGetValue(containerID, out Dictionary<int, FAbilityObject> container))
			{
				container.Remove(objectID);
			}
		}

		public string Tooltip()
		{
			if (!string.IsNullOrWhiteSpace(CachedTooltip))
			{
				return CachedTooltip;
			}
			CachedTooltip = Template.Tooltip(new List<FITooltip>(AbilityEvents.Values));
			return CachedTooltip;

					
		}
}
}