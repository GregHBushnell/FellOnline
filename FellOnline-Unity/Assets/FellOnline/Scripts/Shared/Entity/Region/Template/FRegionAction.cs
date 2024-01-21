using UnityEngine;

namespace FellOnline.Shared
{
	public abstract class FRegionAction : ScriptableObject
	{
		public abstract void Invoke(Character character, FRegion region);
	}
}