using FishNet.Managing.Client;
using FishNet.Managing.Server;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace FellOnline.Server
{
	public abstract class FServerBehaviour : MonoBehaviour
	{
		private static Dictionary<Type, FServerBehaviour> behaviours = new Dictionary<Type, FServerBehaviour>();

		internal static void Register<T>(T behaviour) where T : FServerBehaviour
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
			//Debug.Log("UIManager: Registered " + control.Name);
			behaviours.Add(type, behaviour);
		}

		internal static void Unregister<T>(T behaviour) where T : FServerBehaviour
		{
			if (behaviour == null)
			{
				return;
			}
			else
			{
				//Debug.Log("UIManager: Unregistered " + control.Name);
				behaviours.Remove(behaviour.GetType());
			}
		}

		public static bool TryGet<T>(out T control) where T : FServerBehaviour
		{
			if (behaviours.TryGetValue(typeof(T), out FServerBehaviour result))
			{
				if ((control = result as T) != null)
				{
					return true;
				}
			}
			control = null;
			return false;
		}

		public static T Get<T>() where T : FServerBehaviour
		{
			if (behaviours.TryGetValue(typeof(T), out FServerBehaviour result))
			{
				return result as T;
			}
			return null;
		}

		public bool Initialized { get; private set; }
		public FServer Server { get; private set; }
		public ServerManager ServerManager { get; private set; }
		// ClientManager is used on the servers for Server<->Server communication. *NOTE* Check if null!
		public ClientManager ClientManager { get; private set; }

		/// <summary>
		/// Initializes the server behaviour. Use this if your system requires only Server management.
		/// </summary>
		internal void InternalInitializeOnce(FServer server, ServerManager serverManager)
		{
			InternalInitializeOnce(server, serverManager, null);
		}

		/// <summary>
		/// Initializes the server behaviour. Use this if your system requires both Server and Client management.
		/// </summary>
		internal void InternalInitializeOnce(FServer server, ServerManager serverManager, ClientManager clientManager)
		{
			if (Initialized)
				return;

			Server = server;
			ServerManager = serverManager;
			ClientManager = clientManager;
			Initialized = true;

			Debug.Log("Server: Initialized[" + this.GetType().Name + "]");

			InitializeOnce();
		}

		public abstract void InitializeOnce();

		private void Awake()
		{
			FServerBehaviour.Register(this);
		}

		private void OnDestroy()
		{
			FServerBehaviour.Unregister(this);
		}
	}
}