using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Managing.Logging;
using FishNet.Transporting;
using UnityEngine;
using FellOnline.Client;
using KinematicCharacterController;

namespace FellOnline.Shared
{
	public class FellPlayer : NetworkBehaviour
	{
		public FellCController CharacterController;
		public KinematicCharacterMotor Motor;
		public FKCCCamera CharacterCamera;
		//public KinematicCharacterMotor Motor;

		//Quang: Old input system member
		private const string MouseXInput = "Mouse X";
		private const string MouseYInput = "Mouse Y";
		private const string MouseScrollInput = "Mouse ScrollWheel";
		private const string HorizontalInput = "Horizontal";
		private const string VerticalInput = "Vertical";
		private const string FireOne = "Fire1";
		private const string JumpInput = "Jump";
		private const string CrouchInput = "Crouch";
		private const string RunInput = "Run";
		//private const string ToggleFirstPersonInput = "ToggleFirstPerson";

		private Vector3 _desiredPosition;
		private bool _isMoving = false;
		private bool _jumpQueued = false;
		private bool _crouchInputActive = false;
		private bool _sprintInputActive = false;

		void Awake()
		{
			KinematicCharacterSystem.EnsureCreation();

			//Quang: Using manual simultion instead of KCC auto simulation
			KinematicCharacterSystem.Settings.AutoSimulation = false;
		}
		public override void OnStartNetwork()
		{
			//Quang: Subscribe to tick event, this will replace FixedUpdate
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

			if (base.IsOwner)
			{
				Camera mc = Camera.main;
				if (mc != null)
				{
					CharacterCamera = mc.gameObject.GetComponent<FKCCCamera>();
					if (CharacterCamera != null)
					{
						CharacterCamera.SetFollowTransform(CharacterController.CameraFollowPoint);
					}
				}
			}
			//Quang: The remote client objects must not have movement related 
			//logic code, destroy it. Network transform will handle the movements
			/*else
			{
				KinematicCharacterMotor motor = GetComponent<KinematicCharacterMotor>();
				if (motor != null)
				{
					motor.enabled = false;
				}
				
				FellCController controller = GetComponent<FellCController>();
				
				if (controller != null)
				{
					controller.enabled = false;
				}
				Rigidbody rb = GetComponent<Rigidbody>();
				if (rb != null)
				{
					rb.isKinematic = true;
				}
			}*/
		}
#endif
		private void TimeManager_OnTick()
		{
			Replicate(HandleCharacterInput());
			if (base.IsServerStarted)
			{
				Reconcile(CharacterController.GetState());
			}
		}
		[Client(Logging = LoggingType.Off)]
		//private bool CanUpdateInput()
		//{
			//return !FInputManager.MouseMode;
		//}

		private FKCCInputReplicateData HandleCharacterInput()
		{
			if (!base.IsOwner)
			{
				return default;
			}
			/*if (!CanUpdateInput())
			{
				FKCCInputReplicateData characterInputs = default;

				// always handle rotation
				//characterInputs.CameraRotation = CharacterCamera.Transform.rotation;

				return characterInputs;
			}*/
			
			int moveFlags = 0;
			if (_jumpQueued)
			{
				moveFlags.EnableBit(KCCMoveFlags.Jump);
				_jumpQueued = false;
			}
			if (_crouchInputActive)
			{
				moveFlags.EnableBit(KCCMoveFlags.Crouch);
			}
			if (_sprintInputActive)
			{
				moveFlags.EnableBit(KCCMoveFlags.Sprint);
			}

			return new FKCCInputReplicateData(FInputManager.GetAxis(VerticalInput),
											 FInputManager.GetAxis(HorizontalInput),
											 moveFlags);

											 
		}

		[ReplicateV2]
		private void Replicate(FKCCInputReplicateData input, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
		{
			CharacterController.SetInputs(ref input);

			SimulateMotor((float)base.TimeManager.TickDelta);
		}


		private void SimulateMotor(float deltaTime)
		{
			Motor.UpdatePhase1(deltaTime);
			Motor.UpdatePhase2(deltaTime);

			//Motor.SetPositionAndRotation(Motor.TransientPosition, Motor.TransientRotation);
			Motor.SetPositionAndRotation(Motor.TransientPosition, Motor.TransientRotation);
		}

		[ReconcileV2]
		private void Reconcile(KinematicCharacterMotorState rd, Channel channel = Channel.Unreliable)
		{
			//Quang: Note - KCCMotorState has Rigidbody field, this component is not serialized, 
			// and doesn't have to be reconciled, so we build a new Reconcile data that exclude Rigidbody field
			CharacterController.ApplyState(rd);
		}
	
		private void Update()
		{
			if (!base.IsOwner)
			{
				return;
			}
			
				if (FInputManager.GetKeyDown(JumpInput) && !CharacterController.IsJumping)
			{
				_jumpQueued = true;
			}

			_crouchInputActive = FInputManager.GetKey(CrouchInput);

			_sprintInputActive = FInputManager.GetKey(RunInput);
		}

		private void LateUpdate()
		{
			if (!base.IsOwner)
			{
				return;
			}
			if (CharacterCamera == null)
			{
				return;
			}
			

			//HandleMovement();
			HandleCameraInput();
		}
		private void HandleCameraInput()
		{
			if (!base.IsOwner) return;
			float scrollInput = 0.0f;
#if !UNITY_WEBGL
			
				scrollInput = -FInputManager.GetAxis(MouseScrollInput);
			
#endif
			if (CharacterCamera == null)
			{
				return;
			}
			// Apply inputs to the camera
			CharacterCamera.UpdateWithInput((float)base.TimeManager.TickDelta, scrollInput);
		}
		
	}
}