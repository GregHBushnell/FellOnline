using UnityEngine;

namespace FellOnline.Shared
{
	public abstract class IBoundary : MonoBehaviour
	{
		public abstract Vector3 GetBoundaryOffset();

		public abstract Vector3 GetBoundarySize();
	}
}