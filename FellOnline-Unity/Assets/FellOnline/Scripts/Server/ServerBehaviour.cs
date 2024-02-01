using FishNet.Managing.Server;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace FellOnline.Server
{
	public abstract class ServerBehaviour : MonoBehaviour
	{
		private static Dictionary<Type, ServerBehaviour> behaviours = new Dictionary<Type, ServerBehaviour>();

		internal static void Register<T>(T behaviour) where T : ServerBehaviour
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
			Debug.Log("ServerBehaviour: Registered " + type.Name);
			behaviours.Add(type, behaviour);
		}

		internal static void Unregister<T>(T behaviour) where T : ServerBehaviour
		{
			if (behaviour == null)
			{
				return;
			}
			else
			{
				Type type = behaviour.GetType();
				Debug.Log("ServerBehaviour: Unregistered " + type.Name);
				behaviours.Remove(type);
			}
		}

		public static bool TryGet<T>(out T control) where T : ServerBehaviour
		{
			if (behaviours.TryGetValue(typeof(T), out ServerBehaviour result))
			{
				if ((control = result as T) != null)
				{
					return true;
				}
			}
			control = null;
			return false;
		}

		public static T Get<T>() where T : ServerBehaviour
		{
			if (behaviours.TryGetValue(typeof(T), out ServerBehaviour result))
			{
				return result as T;
			}
			return null;
		}
		public static void InitializeOnceInternal(Server server, ServerManager serverManager)
		{
			if (behaviours == null ||
				behaviours.Count == 0)
			{
				return;
			}
			Debug.Log("ServerBehaviour: Initializing");

			foreach (ServerBehaviour behaviour in behaviours.Values)
			{
				behaviour.InternalInitializeOnce(server, serverManager);
			}
			Debug.Log("ServerBehaviour: Initialization Complete");
		}


		public bool Initialized { get; private set; }
		public Server Server { get; private set; }
		public ServerManager ServerManager { get; private set; }
		

		/// <summary>
		/// Initializes the server behaviour. Use this if your system requires both Server and Client management.
		/// </summary>
			internal void InternalInitializeOnce(Server server, ServerManager serverManager)		{
			if (Initialized)
				return;

			Server = server;
			ServerManager = serverManager;
			Initialized = true;

			
			InitializeOnce();
			Debug.Log("ServerBehaviour: Initialized[" + this.GetType().Name + "]");
		}

		public abstract void InitializeOnce();

		private void Awake()
		{
			ServerBehaviour.Register(this);
		}

		private void OnDestroy()
		{
			ServerBehaviour.Unregister(this);
		}
	}
}