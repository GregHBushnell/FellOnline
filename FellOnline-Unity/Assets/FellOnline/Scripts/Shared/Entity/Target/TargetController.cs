﻿#if !UNITY_SERVER
using FellOnline.Client;
#endif
using UnityEngine;
using System;

namespace FellOnline.Shared
{
	public class TargetController : CharacterBehaviour
	{
		public const float MAX_TARGET_DISTANCE = 600f;
		public const float TARGET_UPDATE_RATE = 0.05f;

		public LayerMask LayerMask;
		public TargetInfo LastTarget;
		public TargetInfo Current;

#if !UNITY_SERVER
		private float nextTick = 0.0f;
		private Cached3DLabel targetLabel;

		public event Action<GameObject> OnChangeTarget;
		public event Action<GameObject> OnUpdateTarget;

		void OnDisable()
		{
			LabelMaker.Cache(targetLabel);
			targetLabel = null;
		}

		public override void OnDestroying()
		{
			OnChangeTarget = null;
			OnUpdateTarget = null;
			LastTarget = default;
			Current = default;
		}

		void Update()
		{
			if (Camera.main == null)
			{
				return;
			}

			// update target label for the client
			if (nextTick < 0.0f)
			{
				nextTick = TARGET_UPDATE_RATE;

				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				//Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));

				UpdateTarget(ray.origin, ray.direction, MAX_TARGET_DISTANCE);

				// target has changed
				if (Current.Target != LastTarget.Target)
				{
					// invoke our change target function
					if (Current.Target == null)
					{
						OnChangeTarget?.Invoke(null);
					}
					else
					{
						OnChangeTarget?.Invoke(Current.Target.gameObject);
					}

					// disable the previous outline and target label
					if (LastTarget.Target != null)
					{
						Outline outline = LastTarget.Target.GetComponent<Outline>();
						if (outline != null)
						{
							outline.enabled = false;
						}
						if (targetLabel != null)
						{
							LabelMaker.Cache(targetLabel);
						}
					}

					// construct or enable the labels and outlines
					if (Current.Target != null)
					{
						if (Character != null)
						{
							Vector3 newPos = Current.Target.position;

							Collider collider = Current.Target.GetComponent<Collider>();
							newPos.y += collider.bounds.extents.y + 0.15f;

							string label = Current.Target.name;
							Color color = Color.grey;

							// apply merchant description
							Merchant merchant = Current.Target.GetComponent<Merchant>();
							if (merchant != null &&
								merchant.Template != null)
							{
								label += "\r\n" + merchant.Template.Description;
								newPos.y += 0.15f;
								color = Color.white;
							}
							else
							{
								Banker banker = Current.Target.GetComponent<Banker>();
								if (banker != null)
								{
									label += "\r\n<Banker>";
									newPos.y += 0.15f;
									color = Color.white;
								}
								else
								{
									AbilityCrafter abilityCrafter = Current.Target.GetComponent<AbilityCrafter>();
									if (abilityCrafter != null)
									{
										label += "\r\n<Ability Crafter>";
										newPos.y += 0.15f;
										color = Color.white;
									}else{
										WorldItem worldItem = Current.Target.GetComponent<WorldItem>();
										if(worldItem != null){
											label += "\r\n<Item>";
											newPos.y += 0.15f;
											color = Color.white;
										}else{
											Mob mob = Current.Target.GetComponent<Mob>();
											if(mob != null){
											label += "\r\n<Mob>";
											newPos.y += 0.15f;
											if(mob.mobDamageController.IsAggro){
												color = Color.red;
											}else{color = Color.white;}
											}
										}
									}
								}
							}

							targetLabel = LabelMaker.Display(label, newPos, color, 1.0f, 0.0f, true);
						}

						Outline outline = Current.Target.GetComponent<Outline>();
						if (outline != null)
						{
							outline.enabled = true;
						}
					}

				}
				else
				{
					// invoke our update function
					if (Current.Target != null)
					{
						OnUpdateTarget?.Invoke(Current.Target.gameObject);
					}
				}
			}
			nextTick -= Time.deltaTime;
		}
#endif
		
		/// <summary>
		/// Updates and returns the TargetInfo for the Current target.
		/// </summary>
		public TargetInfo UpdateTarget(Vector3 origin, Vector3 direction, float maxDistance)
		{
			LastTarget = Current;

			float distance = maxDistance.Clamp(0.0f, MAX_TARGET_DISTANCE);
			RaycastHit hit;
#if !UNITY_SERVER
			Ray ray = new Ray(origin, direction);
			if (Physics.Raycast(ray, out hit, distance, LayerMask))
#else
			if (Character.Motor.PhysicsScene.Raycast(origin, direction, out hit, distance, LayerMask))
#endif
			{
				//Debug.DrawLine(ray.origin, hit.point, Color.red, 1);
				//Debug.Log("hit: " + hit.transform.name + " pos: " + hit.point);
				Current = new TargetInfo(hit.transform, hit.point);
			}
			else
			{
#if UNITY_SERVER
				Ray ray = new Ray(origin, direction);
#endif
				Current = new TargetInfo(null, ray.GetPoint(distance));
			}
			return Current;
		}
	}
}