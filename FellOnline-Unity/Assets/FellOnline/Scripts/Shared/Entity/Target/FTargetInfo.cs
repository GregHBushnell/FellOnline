using UnityEngine;

namespace FellOnline.Shared
{
	public struct FTargetInfo
	{
		public Transform Target;
		public Vector3 HitPosition;

		public FTargetInfo(Transform target, Vector3 hitPosition)
		{
			Target = target;
			HitPosition = hitPosition;
		}
	}
}