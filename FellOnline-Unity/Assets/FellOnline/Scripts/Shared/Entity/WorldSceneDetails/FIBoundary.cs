using UnityEngine;

namespace FellOnline.Shared
{
	public abstract class FIBoundary : MonoBehaviour
	{
		public abstract Vector3 GetBoundaryOffset();

		public abstract Vector3 GetBoundarySize();
	}
}