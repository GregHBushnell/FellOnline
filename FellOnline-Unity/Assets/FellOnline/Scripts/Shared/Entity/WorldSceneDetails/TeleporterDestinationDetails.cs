using System;
using UnityEngine;

namespace FellOnline.Shared
{
	[Serializable]
	public class TeleporterDestinationDetails
	{
		public string Scene;
		public Vector3 Position;
		public Quaternion Rotation;
	}
}