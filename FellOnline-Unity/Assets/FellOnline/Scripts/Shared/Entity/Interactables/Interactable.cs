﻿using System;
using FishNet.Transporting;
using FishNet.Object;
using UnityEngine;
using System.Numerics;
#if !UNITY_SERVER
using static FellOnline.Client.Client;
#endif
namespace FellOnline.Shared
{
	[RequireComponent(typeof(SceneObjectUID))]
	public abstract class Interactable : NetworkBehaviour ,IInteractable
	{
		private const double INTERACT_RATE_LIMIT = 60.0f;

		private SceneObjectUID uid;

		public float InteractionRange = 600f;

		private float interactionRangeSqr;


		public int ID
		{
			get
			{
				return uid.ID;
			}
		}
		
		public Transform Transform { get; private set; }

		void Awake()
		{
			uid = gameObject.GetComponent<SceneObjectUID>();
			Transform = transform;
			interactionRangeSqr = InteractionRange * InteractionRange;
			OnStarting();
		}

		public virtual void OnStarting() { }

		public bool InRange(Transform transform)
		{
			if (transform == null)
			{
				return false;
			}
			if (Transform == null)
			{
				return false;
			}
		if ((Transform.position - transform.position).sqrMagnitude < interactionRangeSqr)
			{
				Debug.Log("In range");
				return true;
			}
			return false;
		}


		public virtual bool OnInteract(Character character)
		{
			if (character == null)
			{
				return false;
			}
			if (character.NextInteractTime < DateTime.UtcNow && InRange(character.Transform))
			{
				character.NextInteractTime = DateTime.UtcNow.AddMilliseconds(INTERACT_RATE_LIMIT);
#if !UNITY_SERVER
				Broadcast(new InteractableBroadcast()
				{
					interactableID = ID,
				}, Channel.Reliable);
#endif
				return true;
			}
			return false;
		}
	}
}