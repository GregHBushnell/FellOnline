#if !UNITY_SERVER
using FellOnline.Client;
using TMPro;
#endif
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using KinematicCharacterController;
using UnityEngine;
using System;

namespace FellOnline.Shared
{
	/// <summary>
	/// Character contains references to all of the controllers associated with the character.
	/// </summary>
	#region KCC

	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(FellCController))]
	[RequireComponent(typeof(FellPlayer))]
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(KinematicCharacterMotor))]

	#endregion
	[RequireComponent(typeof(FCharacterAttributeController))]
	[RequireComponent(typeof(FTargetController))]
	[RequireComponent(typeof(FCooldownController))]
	[RequireComponent(typeof(FInventoryController))]
	[RequireComponent(typeof(FEquipmentController))]
	[RequireComponent(typeof(BankController))]
	[RequireComponent(typeof(FAbilityController))]
	[RequireComponent(typeof(FAchievementController))]
	[RequireComponent(typeof(FBuffController))]
	[RequireComponent(typeof(FQuestController))]
	[RequireComponent(typeof(FCharacterDamageController))]
	[RequireComponent(typeof(FGuildController))]
	[RequireComponent(typeof(FPartyController))]
	[RequireComponent(typeof(FFriendController))]
	[RequireComponent(typeof(WorldItemController))]
	public class Character : NetworkBehaviour, FIPooledResettable
	{
		public Transform Transform { get; private set; }

		#region KCC
		public KinematicCharacterMotor Motor { get; private set; }
		public FellCController CharacterController { get; private set; }
		public FellPlayer f_Player { get; private set; }
		#endregion

		public FCharacterAttributeController AttributeController { get; private set; }
		public FCharacterDamageController DamageController { get; private set; }
		public FTargetController TargetController { get; private set; }
		public FCooldownController CooldownController { get; private set; }
		public FInventoryController InventoryController { get; private set; }
		public FEquipmentController EquipmentController { get; private set; }
		public BankController BankController { get; private set; }
		
		public FAbilityController AbilityController { get; private set; }
		public FAchievementController AchievementController { get; private set; }
		public FBuffController BuffController { get; private set; }
		public FQuestController QuestController { get; private set; }
		public FGuildController GuildController { get; private set; }
		public FPartyController PartyController { get; private set; }
		public FFriendController FriendController { get; private set; }
		public WorldItemController WorldItemController { get; private set; }
		public CharacterAppearanceController AppearanceController { get; private set; }
#if !UNITY_SERVER
		public FLocalInputController LocalInputController { get; private set; }
		public TextMeshPro CharacterNameLabel;
		public TextMeshPro CharacterGuildLabel;
#endif
		// accountID for reference
	public readonly SyncVar<long> ID = new SyncVar<long>(new SyncTypeSetting()
		{
			SendRate = 0.0f,
			Channel = Channel.Reliable,
			ReadPermission = ReadPermission.Observers,
			WritePermission = WritePermission.ServerOnly,
		});
		private void OnCharacterIDChanged(long prev, long next, bool asServer)
		{
#if !UNITY_SERVER
			FClientNamingSystem.SetName(FNamingSystemType.CharacterName, next, (n) =>
			{
				gameObject.name = n;
				CharacterName = n;
				CharacterNameLower = n.ToLower();

				if (CharacterNameLabel != null)
					CharacterNameLabel.text = n;
			});
#endif
		}

		/// <summary>
		/// The characters real name. Use this if you are referencing a character by name. Avoid character.name unless you want the name of the game object.
		/// </summary>
		public string CharacterName;
		public string CharacterNameLower;
		public string Account;
		public long WorldServerID;
		public AccessLevel AccessLevel = AccessLevel.Player;
		public bool IsTeleporting = false;
		public readonly SyncVar<long> Currency = new SyncVar<long>(new SyncTypeSetting()
		{
			SendRate = 0.0f,
			Channel = Channel.Unreliable,
			ReadPermission = ReadPermission.OwnerOnly,
			WritePermission = WritePermission.ServerOnly,
		});
		public readonly SyncVar<int> RaceID = new SyncVar<int>(new SyncTypeSetting()
		{
			SendRate = 0.0f,
			Channel = Channel.Unreliable,
			ReadPermission = ReadPermission.OwnerOnly,
			WritePermission = WritePermission.ServerOnly,
		});
		public readonly SyncVar<string> RaceName = new SyncVar<string>(new SyncTypeSetting()
		{
			SendRate = 0.0f,
			Channel = Channel.Unreliable,
			ReadPermission = ReadPermission.OwnerOnly,
			WritePermission = WritePermission.ServerOnly,
		});
			
		public readonly SyncVar<string> SceneName = new SyncVar<string>(new SyncTypeSetting()
		{
			SendRate = 0.0f,
			Channel = Channel.Unreliable,
			ReadPermission = ReadPermission.OwnerOnly,
			WritePermission = WritePermission.ServerOnly,
		});
		public int SceneHandle;
		public string LastChatMessage = "";
		public DateTime NextChatMessageTime = DateTime.UtcNow;
		public DateTime NextInteractTime = DateTime.UtcNow;

		void Awake()
		{

			ID.OnChange += OnCharacterIDChanged;
			Transform = transform;

			#region KCC
			Motor = gameObject.GetComponent<KinematicCharacterMotor>();

			CharacterController = gameObject.GetComponent<FellCController>();
			CharacterController.Character = this;
			CharacterController.Motor = Motor;
			Motor.CharacterController = CharacterController;
			

			f_Player = gameObject.GetComponent<FellPlayer>();
			f_Player.CharacterController = CharacterController;
			f_Player.Motor = Motor;
			
			
			#endregion

			AttributeController = gameObject.GetComponent<FCharacterAttributeController>();
			DamageController = gameObject.GetComponent<FCharacterDamageController>();
			DamageController.Character = this;
			TargetController = gameObject.GetComponent<FTargetController>();
			TargetController.Character = this;
			CooldownController = gameObject.GetComponent<FCooldownController>();
			InventoryController = gameObject.GetComponent<FInventoryController>();
			InventoryController.Character = this;
			EquipmentController = gameObject.GetComponent<FEquipmentController>();
			EquipmentController.Character = this;
			BankController = gameObject.GetComponent<BankController>();
			BankController.Character = this;
			AbilityController = gameObject.GetComponent<FAbilityController>();
			AbilityController.Character = this;
			AchievementController = gameObject.GetComponent<FAchievementController>();
			AchievementController.Character = this;
			BuffController = gameObject.GetComponent<FBuffController>();
			BuffController.Character = this;
			QuestController = gameObject.GetComponent<FQuestController>();
			QuestController.Character = this;
			GuildController = gameObject.GetComponent<FGuildController>();
			GuildController.Character = this;
			PartyController = gameObject.GetComponent<FPartyController>();
			PartyController.Character = this;
			FriendController = gameObject.GetComponent<FFriendController>();
			FriendController.Character = this;
			WorldItemController = gameObject.GetComponent<WorldItemController>();
			WorldItemController.Character = this;
			AppearanceController = gameObject.GetComponent<CharacterAppearanceController>();
			AppearanceController.Character = this;
			
		}
		void OnDestroy()
		{
				ID.OnChange -= OnCharacterIDChanged;
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
			//FInputManager.MouseMode = true;

			 LocalInputController = gameObject.GetComponent<FLocalInputController>();
			 if (LocalInputController == null)
			 {
			 	LocalInputController = gameObject.AddComponent<FLocalInputController>();
			 }
			 LocalInputController.Initialize(this);

			InitializeUI(initializing);
		}

		private void InitializeUI(bool initializing)
		{
			if (initializing)
			{
				FUIManager.SetCharacter(this);

				if (TargetController != null &&
					FUIManager.TryGet("UITarget", out FUITarget uiTarget))
				{
					TargetController.OnChangeTarget += uiTarget.OnChangeTarget;
					TargetController.OnUpdateTarget += uiTarget.OnUpdateTarget;
				}

				CharacterController.MeshRoot.gameObject.layer = Constants.Layers.LocalEntity;
			}
			else
			{
				FUIManager.SetCharacter(null);

				if (TargetController != null &&
					FUIManager.TryGet("UITarget", out FUITarget uiTarget))
				{
					TargetController.OnChangeTarget -= uiTarget.OnChangeTarget;
					TargetController.OnUpdateTarget -= uiTarget.OnUpdateTarget;
				}
				CharacterController.MeshRoot.gameObject.layer = Constants.Layers.Default;
			}
		}
#endif

		/// <summary>
		/// Resets the Character values to default for pooling.
		/// </summary>
		public void OnPooledReset()
		{
			ID.Value = -1;
			CharacterName = "";
			Account = "";
			WorldServerID = 0;
			AccessLevel = AccessLevel.Player;
			IsTeleporting = false;
			Currency.Value = 0;
			RaceID.Value = 0;
			RaceName.Value = "";
			SceneName.Value = "";
			SceneHandle = 0;
			LastChatMessage = "";
			NextChatMessageTime = DateTime.UtcNow;
			NextInteractTime = DateTime.UtcNow;
			Motor.SetPositionAndRotationAndVelocity(Vector3.zero, Quaternion.identity, Vector3.zero);
		}

		public void SetGuildName(string guildName)
		{
#if !UNITY_SERVER
			if (CharacterGuildLabel != null)
			{
				CharacterGuildLabel.text = !string.IsNullOrWhiteSpace(guildName) ? "[" + guildName + "]" : "";
			}
#endif
		}
	}
}