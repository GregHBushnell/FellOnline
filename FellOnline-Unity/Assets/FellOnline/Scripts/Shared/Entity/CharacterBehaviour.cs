using FishNet.Object;
using UnityEngine;

namespace FellOnline.Shared
{
	/// <summary>
	/// Simple NetworkBehaviour type that stores a character reference and injects itself into the Character Behaviour mapping.
	/// </summary>
	[RequireComponent(typeof(Character))]
	public abstract class CharacterBehaviour : NetworkBehaviour
	{
		public Character Character { get; protected set; }
		public bool Initialized { get; private set; }

		public void InitializeOnce(Character character)
		{
			if (Initialized || character == null)
				return;

			Initialized = true;
			Character = character;
			Character.RegisterCharacterBehaviour(this);

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
			if (Character != null)
			{
				Character.Unregister(this);
			}
			Character = null;
		}

		public virtual void OnDestroying() { }
	}
} 