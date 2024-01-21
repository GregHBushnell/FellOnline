﻿#if !UNITY_SERVER
using FellOnline.Client;
#endif
using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	[RequireComponent(typeof(Character))]
	public class FAbilityController : NetworkBehaviour
	{
		public const long NO_ABILITY = 0;

		private FAbility currentAbility;
		private bool interruptQueued;
		private long queuedAbilityID;
		private float remainingTime;
		private KeyCode heldKey;
		//private Random currentSeed = 12345;

		public Transform AbilitySpawner;
		public Character Character;
		public FCharacterAttributeTemplate BloodResourceTemplate;
		public FCharacterAttributeTemplate AttackSpeedReductionTemplate;
		public FAbilityEvent BloodResourceConversionTemplate;
		public FAbilityEvent ChargedTemplate;
		public FAbilityEvent ChanneledTemplate;
		public Action<string, float, float> OnUpdate;
		// Invoked when the current ability is Interrupted.
		public Action OnInterrupt;
		// Invoked when the current ability is Cancelled.
		public Action OnCancel;

		public Dictionary<long, FAbility> KnownAbilities { get; private set; }
		public HashSet<int> KnownBaseAbilities { get; private set; }
		public HashSet<int> KnownEvents { get; private set; }
		public HashSet<int> KnownSpawnEvents { get; private set; }
		public HashSet<int> KnownHitEvents { get; private set; }
		public HashSet<int> KnownMoveEvents { get; private set; }
		public bool IsActivating { get { return currentAbility != null; } }
		public bool AbilityQueued { get { return queuedAbilityID != NO_ABILITY; } }

		public override void OnStartNetwork()
		{
			if (base.TimeManager != null)
			{
				base.TimeManager.OnTick += TimeManager_OnTick;
			}
		}

		public override void OnStopNetwork()
		{
			if (base.TimeManager != null)
			{
				base.TimeManager.OnTick -= TimeManager_OnTick;
			}
		}
#if !UNITY_SERVER
		public override void OnStartClient()
		{
			base.OnStartClient();

			if (!base.IsOwner)
			{
				this.enabled = false;
			}
			else
			{
				ClientManager.RegisterBroadcast<KnownAbilityAddBroadcast>(OnClientKnownAbilityAddBroadcastReceived);
				ClientManager.RegisterBroadcast<KnownAbilityAddMultipleBroadcast>(OnClientKnownAbilityAddMultipleBroadcastReceived);
				ClientManager.RegisterBroadcast<AbilityAddBroadcast>(OnClientAbilityAddBroadcastReceived);
				ClientManager.RegisterBroadcast<AbilityAddMultipleBroadcast>(OnClientAbilityAddMultipleBroadcastReceived);

				if (FUIManager.TryGet("UICastBar", out FUICastBar uiCastBar))
				{
					OnUpdate += uiCastBar.OnUpdate;
					OnCancel += uiCastBar.OnCancel;
				}
			}
		}

		public override void OnStopClient()
		{
			base.OnStopClient();

			if (base.IsOwner)
			{
				ClientManager.UnregisterBroadcast<KnownAbilityAddBroadcast>(OnClientKnownAbilityAddBroadcastReceived);
				ClientManager.UnregisterBroadcast<KnownAbilityAddMultipleBroadcast>(OnClientKnownAbilityAddMultipleBroadcastReceived);
				ClientManager.UnregisterBroadcast<AbilityAddBroadcast>(OnClientAbilityAddBroadcastReceived);
				ClientManager.UnregisterBroadcast<AbilityAddMultipleBroadcast>(OnClientAbilityAddMultipleBroadcastReceived);

				if (FUIManager.TryGet("UICastBar", out FUICastBar uiCastBar))
				{
					OnUpdate -= uiCastBar.OnUpdate;
					OnCancel -= uiCastBar.OnCancel;
				}
			}
		}

		/// <summary>
		/// Server sent an add known ability broadcast.
		/// </summary>
		private void OnClientKnownAbilityAddBroadcastReceived(KnownAbilityAddBroadcast msg, Channel channel)
		{
			FBaseAbilityTemplate baseAbilityTemplate = FBaseAbilityTemplate.Get<FBaseAbilityTemplate>(msg.templateID);
			if (baseAbilityTemplate != null)
			{
				LearnBaseAbilities(new List<FBaseAbilityTemplate>() { baseAbilityTemplate });
				if (FUIManager.TryGet("UIAbilities", out FUIAbilities uiAbilities))
				{
					uiAbilities.AddKnownAbility(baseAbilityTemplate.ID, baseAbilityTemplate);
				}
			}
		}

		/// <summary>
		/// Server sent an add known ability broadcast.
		/// </summary>
		private void OnClientKnownAbilityAddMultipleBroadcastReceived(KnownAbilityAddMultipleBroadcast msg, Channel channel)
		{
			List<FBaseAbilityTemplate> templates = new List<FBaseAbilityTemplate>();
			foreach (KnownAbilityAddBroadcast knownAbility in msg.abilities)
			{
				FBaseAbilityTemplate baseAbilityTemplate = FBaseAbilityTemplate.Get<FBaseAbilityTemplate>(knownAbility.templateID);
				if (baseAbilityTemplate != null)
				{
					templates.Add(baseAbilityTemplate);
					if (FUIManager.TryGet("UIAbilities", out FUIAbilities uiAbilities))
					{
						uiAbilities.AddKnownAbility(baseAbilityTemplate.ID, baseAbilityTemplate);
					}
				}
			}
			LearnBaseAbilities(templates);
		}

		/// <summary>
		/// Server sent an add ability broadcast.
		/// </summary>
		private void OnClientAbilityAddBroadcastReceived(AbilityAddBroadcast msg, Channel channel)
		{
			FAbilityTemplate abilityTemplate = FAbilityTemplate.Get<FAbilityTemplate>(msg.templateID);
			if (abilityTemplate != null)
			{
				FAbility newAbility = new FAbility(msg.id, abilityTemplate, msg.events);
				LearnAbility(newAbility);

				if (FUIManager.TryGet("UIAbilities", out FUIAbilities uiAbilities))
				{
					uiAbilities.AddAbility(newAbility.ID, newAbility);
				}
			}
		}

		/// <summary>
		/// Server sent an add multiple ability broadcast.
		/// </summary>
		private void OnClientAbilityAddMultipleBroadcastReceived(AbilityAddMultipleBroadcast msg, Channel channel)
		{
			foreach (AbilityAddBroadcast ability in msg.abilities)
			{
				FAbilityTemplate abilityTemplate = FAbilityTemplate.Get<FAbilityTemplate>(ability.templateID);
				if (abilityTemplate != null)
				{
					FAbility newAbility = new FAbility(ability.id, abilityTemplate, ability.events);
					LearnAbility(newAbility);

					if (FUIManager.TryGet("UIAbilities", out FUIAbilities uiAbilities))
					{
						uiAbilities.AddAbility(newAbility.ID, newAbility);
					}
				}
			}
		}
#endif

		private void TimeManager_OnTick()
		{
			FAbilityActivationReplicateData activationData = HandleCharacterInput();
			Replicate(activationData);
			if (base.IsServerStarted)
			{
				
				FAbilityReconcileData state = new FAbilityReconcileData(interruptQueued,
																	  currentAbility == null ? NO_ABILITY : currentAbility.ID,
																	  remainingTime);
				Reconcile(state);
			}
		}

		private FAbilityActivationReplicateData HandleCharacterInput()
		{
			if (!base.IsOwner)
			{
				return default;
			}

			FAbilityActivationReplicateData activationEventData = new FAbilityActivationReplicateData(interruptQueued,
																									queuedAbilityID,
																									heldKey);
			// clear the queued ability
			interruptQueued = false;
			queuedAbilityID = NO_ABILITY;

			return activationEventData;
		}

		[ReplicateV2]
		private void Replicate(FAbilityActivationReplicateData activationData, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
		{
			if (activationData.InterruptQueued)
			{
				OnInterrupt?.Invoke();
				Cancel();
			}
			else if (IsActivating)
			{
				remainingTime -= (float)base.TimeManager.TickDelta;
				if (remainingTime > 0.0f)
				{

					// handle ability update here, display cast bar, display hitbox telegraphs, etc
				OnUpdate?.Invoke(currentAbility.Name, remainingTime, currentAbility.ActivationTime * CalculateSpeedReduction(currentAbility.Template.ActivationSpeedReductionAttribute));
					// handle held ability updates
					if (heldKey != KeyCode.None)
					{
						// a held ability hotkey was released or the FCharacter can no longer activate the ability
						if (activationData.HeldKey == KeyCode.None || !CanActivate(currentAbility))
						{
							// add ability to cooldowns
							AddCooldown(currentAbility);

							Cancel();
						}
						// channeled abilities like beam effects or a charge rush that are continuously updating or spawning objects should be handled here
						else if (ChanneledTemplate != null &&
								 currentAbility.HasAbilityEvent(ChanneledTemplate.ID))
						{
							// get target info
							FTargetInfo targetInfo = Character.TargetController.UpdateTarget(Character.CharacterController.VirtualCameraPosition,
																							Character.CharacterController.VirtualCameraRotation * Vector3.forward,
																							currentAbility.Range);
							// spawn the ability object
							if (FAbilityObject.TrySpawn(currentAbility, Character, this, AbilitySpawner, targetInfo))
							{
								// channeled abilities consume resources during activation
								currentAbility.ConsumeResources(Character.AttributeController, BloodResourceConversionTemplate, BloodResourceTemplate);
							}
						}
					}
					return;
				}

				// this will allow for charged abilities to remain held for aiming purposes
				if (ChargedTemplate != null &&
					currentAbility.HasAbilityEvent(ChargedTemplate.ID) &&
					heldKey != KeyCode.None &&
					activationData.HeldKey != KeyCode.None)
				{
					return;
				}

				// complete the final activation of the ability
				if (CanActivate(currentAbility))
				{
					// get target info
					FTargetInfo targetInfo = Character.TargetController.UpdateTarget(Character.CharacterController.VirtualCameraPosition,
																					Character.CharacterController.VirtualCameraRotation * Vector3.forward,
																					currentAbility.Range);
					// spawn the ability object
					if (FAbilityObject.TrySpawn(currentAbility, Character, this, AbilitySpawner, targetInfo))
					{
						// consume resources
						currentAbility.ConsumeResources(Character.AttributeController, BloodResourceConversionTemplate, BloodResourceTemplate);

						// add ability to cooldowns
						AddCooldown(currentAbility);
					}
				}
				// reset ability data
				Cancel();
			}
			else if (activationData.QueuedAbilityID != NO_ABILITY &&
					 KnownAbilities.TryGetValue(activationData.QueuedAbilityID, out FAbility validatedAbility) &&
					 CanActivate(validatedAbility))
			{
				interruptQueued = false;
				currentAbility = validatedAbility;
				remainingTime = validatedAbility.ActivationTime * CalculateSpeedReduction(validatedAbility.Template.ActivationSpeedReductionAttribute);
				heldKey = activationData.HeldKey;
			}
		}

		[ReconcileV2]
		private void Reconcile(FAbilityReconcileData rd, Channel channel = Channel.Unreliable)
		{
			if (rd.Interrupt ||
				rd.AbilityID == NO_ABILITY ||
				!KnownAbilities.TryGetValue(rd.AbilityID, out FAbility ability))
			{
				OnInterrupt?.Invoke();
				Cancel();
			}
			else
			{
				currentAbility = ability;
				remainingTime = rd.RemainingTime;
			}
		}

		public float CalculateSpeedReduction(FCharacterAttributeTemplate attribute)
		{
			if (attribute != null)
			{
				FCharacterAttribute speedReduction;
				if (Character.AttributeController.TryGetAttribute(attribute.ID, out speedReduction))
				{
					return 1.0f - ((speedReduction.FinalValueAsPct).Clamp(0.0f, 1.0f));
				}
			}
			return 1.0f;
		}

		public void Interrupt(Character attacker)
		{
			interruptQueued = true;
		}


		public void Activate(long referenceID, KeyCode heldKey)
		{
#if !UNITY_SERVER
			// validate UI controls are focused so we aren't casting spells when hovering over interfaces.
			if (/*FInputManager.MouseMode ||*/
				!CanManipulate())
			{
				return;
			}

			if (!IsActivating && !interruptQueued)
			{
				queuedAbilityID = referenceID;
				this.heldKey = heldKey;
			}
#endif
		}

		/// <summary>
		/// Validates that we can activate the ability and returns it if successful.
		/// </summary>
		private bool CanActivate(FAbility ability)
		{
			return CanManipulate() &&
				   KnownAbilities.TryGetValue(ability.ID, out FAbility knownAbility) &&
				   !Character.CooldownController.IsOnCooldown(knownAbility.Template.Name) &&
				   knownAbility.MeetsRequirements(Character) &&
				   knownAbility.HasResource(Character, BloodResourceConversionTemplate, BloodResourceTemplate);
		}

		internal void Cancel()
		{
			interruptQueued = false;
			queuedAbilityID = NO_ABILITY;
			currentAbility = null;
			remainingTime = 0.0f;
			heldKey = KeyCode.None;

			OnCancel?.Invoke();
		}

		internal void AddCooldown(FAbility ability)
		{
			FAbilityTemplate currentAbilityTemplate = ability.Template;
			if (ability.Cooldown > 0.0f)
			{
				float cooldownReduction = CalculateSpeedReduction(currentAbilityTemplate.CooldownReductionAttribute);
				float cooldown = ability.Cooldown * cooldownReduction;

				Character.CooldownController.AddCooldown(currentAbilityTemplate.Name, new FCooldownInstance(cooldown));
			}
		}

		public void RemoveAbility(int referenceID)
		{
			KnownAbilities.Remove(referenceID);
		}

		public bool CanManipulate()
		{
			if (Character == null ||
				Character.IsTeleporting ||
				!Character.IsSpawned)
				return false;

			/*if (!FCharacter.IsSafeZone &&
				  (FCharacter.State == FCharacterState.Idle ||
				  FCharacter.State == FCharacterState.Moving) &&
				  FCharacter.State != FCharacterState.UsingObject &&
				  FCharacter.State != FCharacterState.IsFrozen &&
				  FCharacter.State != FCharacterState.IsStunned &&
				  FCharacter.State != FCharacterState.IsMesmerized) return true;
			*/
			return true;
		}

		public bool KnowsAbility(int abilityID)
		{
			if ((KnownBaseAbilities != null && KnownBaseAbilities.Contains(abilityID)) ||
				(KnownEvents != null && KnownEvents.Contains(abilityID)))
			{
				return true;
			}
			return false;
		}

		public bool LearnBaseAbilities(List<FBaseAbilityTemplate> abilityTemplates = null)
		{
			if (abilityTemplates == null)
			{
				return false;
			}

			for (int i = 0; i < abilityTemplates.Count; ++i)
			{
				// if the template is an ability event we add them to their mapped containers
				FAbilityEvent abilityEvent = abilityTemplates[i] as FAbilityEvent;
				if (abilityEvent != null)
				{
					// add the event to the global events map
					if (KnownEvents == null)
					{
						KnownEvents = new HashSet<int>();
					}
					if (!KnownEvents.Contains(abilityEvent.ID))
					{
						KnownEvents.Add(abilityEvent.ID);
					}

					// figure out what kind of event it is and add to the respective category
					if (abilityEvent is FHitEvent)
					{
						if (KnownHitEvents == null)
						{
							KnownHitEvents = new HashSet<int>();
						}
						if (!KnownHitEvents.Contains(abilityEvent.ID))
						{
							KnownHitEvents.Add(abilityEvent.ID);
						}
					}
					else if (abilityEvent is FMoveEvent)
					{
						if (KnownMoveEvents == null)
						{
							KnownMoveEvents = new HashSet<int>();
						}
						if (!KnownMoveEvents.Contains(abilityEvent.ID))
						{
							KnownMoveEvents.Add(abilityEvent.ID);
						}
					}
					else if (abilityEvent is FSpawnEvent)
					{
						if (KnownSpawnEvents == null)
						{
							KnownSpawnEvents = new HashSet<int>();
						}
						if (!KnownSpawnEvents.Contains(abilityEvent.ID))
						{
							KnownSpawnEvents.Add(abilityEvent.ID);
						}
					}
				}
				else
				{
					FAbilityTemplate abilityTemplate = abilityTemplates[i] as FAbilityTemplate;
					if (abilityTemplate != null)
					{
						if (KnownBaseAbilities == null)
						{
							KnownBaseAbilities = new HashSet<int>();
						}
						if (!KnownBaseAbilities.Contains(abilityTemplate.ID))
						{
							KnownBaseAbilities.Add(abilityTemplate.ID);
						}
					}
				}
			}
			return true;
		}

		public void LearnAbility(FAbility ability)
		{
			if (ability == null)
			{
				return;
			}

			if (KnownAbilities == null)
			{
				KnownAbilities = new Dictionary<long, FAbility>();
			}
			KnownAbilities[ability.ID] = ability;
		}
	}
}