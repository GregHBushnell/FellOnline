#if !UNITY_SERVER
using FellOnline.Client;
using TMPro;
#endif
using FishNet.Object;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace FellOnline.Shared
{
   
	
	[RequireComponent(typeof(MobController))]
    [RequireComponent(typeof(MobAttributeController))]
    [RequireComponent(typeof(MobDamageController))]
	
	public class Mob : NetworkBehaviour, IPooledResettable
	{
         private Dictionary<Type, MobBehaviour> behaviours = new Dictionary<Type, MobBehaviour>();
		public Transform Transform { get; private set; }

        
		public MobController mobController { get; private set; }

        public MobDamageController mobDamageController { get; private set; }
		void Awake()
		{

			Transform = transform;

			mobController = gameObject.GetComponent<MobController>();
            mobDamageController = gameObject.GetComponent<MobDamageController>();

			MobBehaviour[] c = gameObject.GetComponents<MobBehaviour>();
			if (c != null)
			{
				for (int i = 0; i < c.Length; ++i)
				{
					MobBehaviour behaviour = c[i];
					if (behaviour == null)
					{
						continue;
					}

					behaviour.InitializeOnce(this);
				}
			}

			
			
		}
		void OnDestroy()
		{
				
		}

#if !UNITY_SERVER
		public override void OnStartClient()
		{
			base.OnStartClient();

			if (base.IsOwner)
			{
				InitializeLocal(true);
			}
		}

		public override void OnStopClient()
		{
			base.OnStopClient();
			if (base.IsOwner)
			{
				InitializeLocal(false);
			}
		}

		private void InitializeLocal(bool initializing)
		{
			
		}

		
#endif
public void RegisterMobBehaviour(MobBehaviour behaviour)
		{
			if (behaviour == null)
			{
				return;
			}
			Type type = behaviour.GetType();
			if (behaviours.ContainsKey(type))
			{
				return;
			}
			behaviours.Add(type, behaviour);
		}

		public void Unregister<T>(T behaviour) where T : MobBehaviour
		{
			if (behaviour == null)
			{
				return;
			}
			else
			{
				Type type = behaviour.GetType();
				//Debug.Log(MobName + ": Unregistered " + type.Name);
				behaviours.Remove(type);
			}
		}

		public bool TryGet<T>(out T control) where T : MobBehaviour
		{
			if (behaviours.TryGetValue(typeof(T), out MobBehaviour result))
			{
				if ((control = result as T) != null)
				{
					return true;
				}
			}
			control = null;
			return false;
		}

		public T Get<T>() where T : MobBehaviour
		{
			if (behaviours.TryGetValue(typeof(T), out MobBehaviour result))
			{
				return result as T;
			}
			return null;
		}


		/// <summary>
		/// Resets the Mob values to default for pooling.
		/// </summary>
		public void OnPooledReset()
		{
			
		}

		
	}
}