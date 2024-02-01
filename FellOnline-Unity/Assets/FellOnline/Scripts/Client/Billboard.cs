﻿using UnityEngine;

namespace FellOnline.Client
{
	public sealed class Billboard : MonoBehaviour
	{
#if !UNITY_SERVER
		private Camera Camera;

		public bool PivotYAxis = false;

		public Transform Transform { get; private set; }

		void Awake()
		{
			SetCamera(Camera.main);
		}

		void LateUpdate()
		{
			if (Camera != null)
			{
				// make the object share the same rotation as the camera
				transform.rotation = Camera.transform.rotation;
				if (PivotYAxis)
				{
					transform.rotation = Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y, 0.0f);
				}
			}
			else
			{
				// try to get the new main camera
				Camera = Camera.main;
			}
		}

		public void SetCamera(Camera target)
		{
			Camera = target;
			Transform = Camera == null ? null : Camera.transform;
		}
#endif
	}
}