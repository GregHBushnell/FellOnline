using FishNet.Object;
using UnityEngine;

namespace FellOnline.Shared
{
	/// <summary>
	/// Simple NetworkBehaviour type that stores a character reference and injects itself into the Mob Behaviour mapping.
	/// </summary>
	[RequireComponent(typeof(Mob))]
	public abstract class MobBehaviour : NetworkBehaviour
	{
		public Mob Mob { get; protected set; }
		public bool Initialized { get; private set; }

		public void InitializeOnce(Mob character)
		{
			if (Initialized || character == null)
				return;

			Initialized = true;
			Mob = character;
			Mob.RegisterMobBehaviour(this);

			InitializeOnce();
		}

		public virtual void InitializeOnce() { }

		protected void Awake()
		{
			OnAwake();
		}

		public virtual void OnAwake() { }

		private void OnDestroy()
		{
			OnDestroying();
			if (Mob != null)
			{
				Mob.Unregister(this);
			}
			Mob = null;
		}

		public virtual void OnDestroying() { }
	}
} 