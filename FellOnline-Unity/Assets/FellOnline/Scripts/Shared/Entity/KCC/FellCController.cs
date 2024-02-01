﻿using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using FellOnline.Client;

namespace FellOnline.Shared
{
	public enum KCCCharacterState
	{
		Default,
	}

	public enum OrientationMethod
	{
		TowardsCamera,
		TowardsMovement,
	}

	public struct AICharacterInputs
	{
		public Vector3 MoveVector;
		public Vector3 LookVector;
	}

	public enum BonusOrientationMethod
	{
		None,
		TowardsGravity,
		TowardsGroundSlopeAndGravity,
	}

	public class FellCController : MonoBehaviour, ICharacterController
	{
		public const float StableSprintSpeedConstant = 6.0f;
		public const float StableMoveSpeedConstant = 4.0f;
		public const float StableCrouchSpeedConstant = 2.0f;
		public const float StableJumpUpSpeedConstant = 6.5f;
		public static readonly Vector3 GravityConstant = new Vector3(0, -18.0f, 0);

		public Character Character;
		public KinematicCharacterMotor Motor;

		[Header("Stable Movement")]
		public float StableMovementSharpness = 20f;
		public float OrientationSharpness = 10f;
		public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;

		[Header("Air Movement")]
		public float MaxAirMoveSpeed = 6f;
		public float AirAccelerationSpeed = 0f;
		public float Drag = 0.1f;

		[Header("Jumping")]
		public bool AllowJumpingWhenSliding = false;
		public float JumpScalableForwardSpeed = 0f;
		public float JumpPreGroundingGraceTime = 0f;
		public float JumpPostGroundingGraceTime = 0f;

		[Header("Misc")]
		public List<Collider> IgnoredColliders = new List<Collider>();
		public BonusOrientationMethod BonusOrientationMethod = BonusOrientationMethod.None;
		public float BonusOrientationSharpness = 10f;
		public Transform MeshRoot;
		public Transform CameraFollowPoint;
		public float CrouchedCapsuleHeight = 0.5f;
		public float FullCapsuleHeight = 2f;
		public float CapsuleBaseOffset = 1f;
		public CharacterAttributeTemplate MoveSpeedTemplate;
		public CharacterAttributeTemplate RunSpeedTemplate;
		public CharacterAttributeTemplate JumpSpeedTemplate;
		public CharacterAttributeTemplate SwimSpeedTemplate;
		public CharacterAttributeTemplate FastFallSpeedTemplate;
		public CharacterAttributeTemplate GravityTemplate;

		public KCCCharacterState CurrentCharacterState { get; private set; }

		private Collider[] _probedColliders = new Collider[8];

		private Animator _animator = null;

		// Current frame input state
		private Vector3 _moveInputVector;
		public Vector3 _lookInputVector;
		public Vector3 _lastMovementVector;
		public Vector3 _clicktargetPos;
		public bool _moving;
		public bool _inPos = true;
		// Quang: add camera rotation field for smooth rotate
		private Quaternion _cameraRotation;
		private bool _crouchInputDown = false;
		private bool _jumpRequested = false;
		private bool _sprintInputDown = false;

		// Multi Frame State, this needs to be synchronized
		private float _timeSinceJumpRequested = float.MaxValue;
		private float _timeSinceLastAbleToJump = 0f;
		private bool _isCrouching = false;

		public Vector3 VirtualCameraPosition { get; private set; }
		public Quaternion VirtualCameraRotation { get; private set; }
		public bool IsJumping { get; private set; }

		private void Awake()
		{
			// Handle initial state
			TransitionToState(KCCCharacterState.Default);

			_animator = GetComponentInChildren<Animator>();
		}

		private void OnEnable()
		{
			_moveInputVector = Vector3.zero;
			_lookInputVector = Vector3.zero;
		}

		/// <summary>
		/// Handles movement state transitions and enter/exit callbacks
		/// </summary>
		public void TransitionToState(KCCCharacterState newState)
		{
			KCCCharacterState tmpInitialState = CurrentCharacterState;
			OnStateExit(tmpInitialState, newState);
			CurrentCharacterState = newState;
			OnStateEnter(newState, tmpInitialState);
		}

		/// <summary>
		/// Event when entering a state
		/// </summary>
		public void OnStateEnter(KCCCharacterState state, KCCCharacterState fromState)
		{
			switch (state)
			{
				case KCCCharacterState.Default:
					{
						break;
					}
			}
		}

		/// <summary>
		/// Event when exiting a state
		/// </summary>
		public void OnStateExit(KCCCharacterState state, KCCCharacterState toState)
		{
			switch (state)
			{
				case KCCCharacterState.Default:
					{
						break;
					}
			}
		}

		public void ApplyState(KinematicCharacterMotorState state)
		{
			// Take any state needed for the controller here
			_timeSinceLastAbleToJump = state.TimeSinceLastAbleToJump;
			_isCrouching = state.IsCrouching;
			_timeSinceJumpRequested = state.TimeSinceJumpRequested;

			Motor.ApplyState(state);
		}

		public KinematicCharacterMotorState GetState()
		{
			KinematicCharacterMotorState baseState = Motor.GetState();

			// Apply state from controller here.
			baseState.TimeSinceLastAbleToJump = _timeSinceLastAbleToJump;
			baseState.IsCrouching = _isCrouching;
			baseState.TimeSinceJumpRequested = _timeSinceJumpRequested;

			return baseState;
		}

		/// <summary>
		/// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
		/// </summary>
		public void SetInputs(ref FKCCInputReplicateData inputs)
		{
			//VirtualCameraPosition = inputs.CameraPosition;
			//VirtualCameraRotation = inputs.CameraRotation;

			// Clamp input
			Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);


			switch (CurrentCharacterState)
			{
				case KCCCharacterState.Default:
					{
						// Move and look inputs
						_moveInputVector = moveInputVector;
						
						if (_moveInputVector != Vector3.zero)
						{
							//_animator.SetBool("Walking", true);
							_moving = true;
							_lastMovementVector = _moveInputVector.normalized;
						}else{
							//_animator.SetBool("Walking", false);
							_moving = false;
						}
						
						switch (OrientationMethod)
						{
							case OrientationMethod.TowardsMovement:
								if(_lastMovementVector != Vector3.zero)
								{
									_lookInputVector = _lastMovementVector;
								}else{
									_lookInputVector = _moveInputVector.normalized;
								}
								
								break;
						}

						// Jumping input
						if (inputs.MoveFlags.IsFlagged(KCCMoveFlags.Jump))
						{
							_timeSinceJumpRequested = 0f;
							_jumpRequested = true;
						}

						// Crouching input
						_crouchInputDown = inputs.MoveFlags.IsFlagged(KCCMoveFlags.Crouch);

						// Sprinting input
						_sprintInputDown = inputs.MoveFlags.IsFlagged(KCCMoveFlags.Sprint);

						

						break;
					}
			}
		}

		/// <summary>
		/// This is called every frame by the AI script in order to tell the character what its inputs are
		/// </summary>
		public void SetInputs(ref AICharacterInputs inputs)
		{
			_moveInputVector = inputs.MoveVector;
			_lookInputVector = inputs.LookVector;
		}

		/// <summary>
		/// (Called by KinematicCharacterMotor during its update cycle)
		/// This is called before the character begins its movement update
		/// </summary>
		public void BeforeCharacterUpdate(float deltaTime)
		{
		}

		/// <summary>
		/// (Called by KinematicCharacterMotor during its update cycle)
		/// This is where you tell your character what its rotation should be right now. 
		/// This is the ONLY place where you should set the character's rotation
		/// </summary>
		/// 
		 private void HandleClickToMove()
		{
			Ray ray =  Character.f_Player.CharacterCamera.Camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				_inPos = false;
				_animator.SetBool("Walking", true);

				Vector3 destination = hit.point;
				//Vector3 tilePosition = new Vector3(Mathf.Round(destination.x / 1) * 1, 0, Mathf.Round(destination.z / 1) * 1);
				_clicktargetPos = destination;
			}
		}
		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
		{
			switch (CurrentCharacterState)
			{
				case KCCCharacterState.Default:
					{
						if (_lookInputVector.sqrMagnitude > 0f && OrientationSharpness > 0f)
						{
							// Smoothly interpolate from current to target look direction
							//Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

							// Set the current rotation (which will be used by the KinematicCharacterMotor)
							//currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);

							float targetRotationY = Mathf.Atan2(_lookInputVector.x, _lookInputVector.z) * Mathf.Rad2Deg + _cameraRotation.eulerAngles.y;

							Quaternion targetQuarternion = Quaternion.Euler(0.0f, targetRotationY, 0.0f);

							if (Mathf.Abs(Quaternion.Angle(transform.rotation, targetQuarternion) - 180f) <= 3f)
							{
								//Debug.Log("180 degree detected");
								targetRotationY -= 10f;
							}

							currentRotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0.0f, targetRotationY, 0.0f), 1 - Mathf.Exp(-OrientationSharpness * deltaTime));
						}

						Vector3 currentUp = (currentRotation * Vector3.up);
						if (BonusOrientationMethod == BonusOrientationMethod.TowardsGravity)
						{
							// Rotate from current up to invert gravity
							Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -GravityConstant.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
							currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
						}
						else if (BonusOrientationMethod == BonusOrientationMethod.TowardsGroundSlopeAndGravity)
						{
							if (Motor.GroundingStatus.IsStableOnGround)
							{
								Vector3 initialCharacterBottomHemiCenter = Motor.TransientPosition + (currentUp * Motor.Capsule.radius);

								Vector3 smoothedGroundNormal = Vector3.Slerp(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
								currentRotation = Quaternion.FromToRotation(currentUp, smoothedGroundNormal) * currentRotation;

								// Move the position to create a rotation around the bottom hemi center instead of around the pivot
								Motor.SetTransientPosition(initialCharacterBottomHemiCenter + (currentRotation * Vector3.down * Motor.Capsule.radius));
							}
							else
							{
								Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, -GravityConstant.normalized, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
								currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
							}
						}
						else
						{
							Vector3 smoothedGravityDir = Vector3.Slerp(currentUp, Vector3.up, 1 - Mathf.Exp(-BonusOrientationSharpness * deltaTime));
							currentRotation = Quaternion.FromToRotation(currentUp, smoothedGravityDir) * currentRotation;
						}
						break;
					}
			}
		}

		/// <summary>
		/// (Called by KinematicCharacterMotor during its update cycle)
		/// This is where you tell your character what its velocity should be right now. 
		/// This is the ONLY place where you can set the character's velocity
		/// </summary>
		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
		{
			// if(FInputManager.GetKeyDown("Fire1")){
			// 	HandleClickToMove();
			// }
			switch (CurrentCharacterState)
			{
				case KCCCharacterState.Default:
					{
						// Ground movement
						if (Motor.GroundingStatus.IsStableOnGround)
						{
							float currentVelocityMagnitude = currentVelocity.magnitude;

							Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;

							// Reorient velocity on slope
							currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

							// Calculate target velocity
							Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
							Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;

							float targetSpeed = StableMoveSpeedConstant;

							if (Character != null &&
								Character.TryGet(out CharacterAttributeController attributeController))
							{
								if (_isCrouching)
								{
									targetSpeed = StableCrouchSpeedConstant;
								}
								else if (_sprintInputDown &&
									RunSpeedTemplate != null &&
									attributeController.TryGetAttribute(RunSpeedTemplate, out CharacterAttribute runSpeedModifier))
								{
									targetSpeed = StableSprintSpeedConstant * runSpeedModifier.FinalValueAsPct;
								}
								else if (MoveSpeedTemplate != null &&
									attributeController.TryGetAttribute(MoveSpeedTemplate, out CharacterAttribute moveSpeedModifier))
								{
									targetSpeed = StableMoveSpeedConstant * moveSpeedModifier.FinalValueAsPct;
								}

								/*if (_swimming &&
									SwimSpeedTemplate != null &&
									Character.AttributeController.TryGetAttribute(SwimSpeedTemplate, out CharacterAttribute swimSpeed))
								{

								}*/
							}
							else
							{
								if (_isCrouching)
								{
									targetSpeed = StableCrouchSpeedConstant;
								}
								else if (_sprintInputDown)
								{
									targetSpeed = StableSprintSpeedConstant;
								}
							}

							Vector3 targetMovementVelocity = reorientedInput * targetSpeed;

							// Smooth movement Velocity
							if(!_inPos && !_moving ){
								currentVelocity = Vector3.Lerp(currentVelocity, _clicktargetPos, StableMovementSharpness * deltaTime);
							}else{
							currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, StableMovementSharpness * deltaTime);
							}
						}
						// Air movement
						else
						{
							// Add move input
							if (_moveInputVector.sqrMagnitude > 0f)
							{
								Vector3 addedVelocity = _moveInputVector * AirAccelerationSpeed * deltaTime;

								Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(currentVelocity, Motor.CharacterUp);

								// Limit air velocity from inputs
								if (currentVelocityOnInputsPlane.magnitude < MaxAirMoveSpeed)
								{
									// clamp addedVel to make total vel not exceed max vel on inputs plane
									Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, MaxAirMoveSpeed);
									addedVelocity = newTotal - currentVelocityOnInputsPlane;
								}
								else
								{
									// Make sure added vel doesn't go in the direction of the already-exceeding velocity
									if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
									{
										addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
									}
								}

								// Prevent air-climbing sloped walls
								if (Motor.GroundingStatus.FoundAnyGround)
								{
									if (Vector3.Dot(currentVelocity + addedVelocity, addedVelocity) > 0f)
									{
										Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
										addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
									}
								}

								// Apply added velocity
								currentVelocity += addedVelocity;
							}

							// Gravity
							if (Character.TryGet(out CharacterAttributeController attributeController))
							{
								// Character Independant Gravity
								if (GravityTemplate != null &&
									attributeController.TryGetAttribute(GravityTemplate, out CharacterAttribute gravityModifier))
								{
									currentVelocity += GravityConstant * gravityModifier.FinalValueAsPct * deltaTime;
								}

								// Fast Fall
								if (_isCrouching &&
									FastFallSpeedTemplate != null &&
									attributeController.TryGetAttribute(FastFallSpeedTemplate, out CharacterAttribute fastFallModifier))
								{
									currentVelocity.y += GravityConstant.y * fastFallModifier.FinalValueAsPct * deltaTime;
								}
							}
							else
							{
								currentVelocity += GravityConstant * deltaTime;
							}

							// Drag
							currentVelocity *= (1f / (1f + (Drag * deltaTime)));
						}

						// Handle jumping
						_timeSinceJumpRequested += deltaTime;
						if (_jumpRequested)
						{
							// See if we actually are allowed to jump
							if (((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) && _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
							{
								// Calculate jump direction before ungrounding
								Vector3 jumpDirection = Motor.CharacterUp;
								if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
								{
									jumpDirection = Motor.GroundingStatus.GroundNormal;
								}

								// Makes the character skip ground probing/snapping on its next update. 
								// If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
								Motor.ForceUnground();

								// Add to the return velocity and reset jump state
								float jumpSpeed = StableJumpUpSpeedConstant;
								if (JumpSpeedTemplate != null &&
									Character.TryGet(out CharacterAttributeController attributeController) &&
									attributeController.TryGetAttribute(JumpSpeedTemplate, out CharacterAttribute jumpSpeedModifier))
								{
									jumpSpeed *= jumpSpeedModifier.FinalValueAsPct;
								}
								currentVelocity += (jumpDirection * jumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
								currentVelocity += (_moveInputVector * JumpScalableForwardSpeed);

								_jumpRequested = false;

								IsJumping = true;
							}
						}

						// Take into account additive velocity
						//if (_internalVelocityAdd.sqrMagnitude > 0f)
						//{
						//    currentVelocity += _internalVelocityAdd;
						//    _internalVelocityAdd = Vector3.zero;
						//}
						break;
					}
			}
		}

		/// <summary>
		/// (Called by KinematicCharacterMotor during its update cycle)
		/// This is called after the character has finished its movement update
		/// </summary>
		public void AfterCharacterUpdate(float deltaTime)
		{
			switch (CurrentCharacterState)
			{
				case KCCCharacterState.Default:
					{
						// if (Vector3.Distance(Motor.Transform.position, _clicktargetPos) < 0.1f)
						// {
						// 	_inPos = true;
						// 	_animator.SetBool("Walking", false);
						// 	_clicktargetPos = Vector3.zero;
						// }
							

						// Handle jump-related values
						{
							if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
							{
								_jumpRequested = false;
							}

							if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
							{
								// If we're on a ground surface, reset jumping values
								_timeSinceLastAbleToJump = 0f;
							}
							else
							{
								// Keep track of time since we were last able to jump (for grace period)
								_timeSinceLastAbleToJump += deltaTime;
							}
						}

						// Handle uncrouching
						if (!_isCrouching && _crouchInputDown)
						{
							_isCrouching = true;
							Motor.SetCapsuleDimensions(Motor.Capsule.radius, CrouchedCapsuleHeight, CapsuleBaseOffset / (FullCapsuleHeight / CrouchedCapsuleHeight));
						}
						else if (_isCrouching && !_crouchInputDown)
						{
							// Do an overlap test with the character's standing height to see if there are any obstructions
							Motor.SetCapsuleDimensions(Motor.Capsule.radius, FullCapsuleHeight, CapsuleBaseOffset);
							if (Motor.CharacterOverlap(
								Motor.TransientPosition,
								Motor.TransientRotation,
								_probedColliders,
								Motor.CollidableLayers,
								QueryTriggerInteraction.Ignore) > 0)
							{
								// If obstructions, just stick to crouching dimensions
								// This is offset to ensure the crouch goes towards the feet
								// instead of towards the head. Otherwise we can't uncrouch!
								//MeshRoot.localScale = new Vector3(1f, CrouchedCapsuleHeight / FullCapsuleHeight, 1f);
								Motor.SetCapsuleDimensions(Motor.Capsule.radius, CrouchedCapsuleHeight, CapsuleBaseOffset / (FullCapsuleHeight / CrouchedCapsuleHeight));
							}
							else
							{
								// If no obstructions, uncrouch
								_isCrouching = false;
							}
						}

						if (_animator != null)
						{
							_animator.SetBool("Crouching", _isCrouching);
							_animator.SetBool("OnGround", Motor.GroundingStatus.FoundAnyGround);

							if (!_isCrouching && !IsJumping && Motor.GroundingStatus.FoundAnyGround && _moveInputVector.sqrMagnitude > 0f)
							{
								if(_isCrouching){_animator.SetFloat("Speed", .1f);}
								else
								if(_sprintInputDown){
									_animator.SetFloat("Speed", 1f);	
								}else{
									_animator.SetFloat("Speed", .5f);
								}
								 
								
								//_animator.SetFloat("Rotation", _lookInputVector.magnitude);
								
 
       			 				_animator.SetFloat("Horizontal", _lookInputVector.x);
        						_animator.SetFloat("Vertical", _lookInputVector.z);
							}else{
								_animator.SetFloat("Speed", 0f);
							}
						}
						break;
					}
			}
		}

		public void PostGroundingUpdate(float deltaTime)
		{
			// Handle landing and leaving ground
			if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
			{
				OnLanded();
			}
			else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
			{
				OnLeaveStableGround();
			}
		}

		public bool IsColliderValidForCollisions(Collider coll)
		{
			if (IgnoredColliders.Count == 0)
			{
				return true;
			}

			if (IgnoredColliders.Contains(coll))
			{
				return false;
			}

			return true;
		}

		public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
		{
		}

		public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
		{
		}

		public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
		{
		}

		protected void OnLanded()
		{
			IsJumping = false;
		}

		protected void OnLeaveStableGround()
		{
		}

		public void OnDiscreteCollisionDetected(Collider hitCollider)
		{
		}
	}
}