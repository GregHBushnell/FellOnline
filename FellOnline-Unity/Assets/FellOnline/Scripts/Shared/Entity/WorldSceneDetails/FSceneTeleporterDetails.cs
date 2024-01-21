using System;
using UnityEngine;

namespace FellOnline.Shared
{
	[Serializable]
	public class FSceneTeleporterDetails
	{
		internal string From;
		public string ToScene;
		public Vector3 ToPosition;
		public Quaternion ToRotation;
		public Sprite SceneTransitionImage;
	}
}