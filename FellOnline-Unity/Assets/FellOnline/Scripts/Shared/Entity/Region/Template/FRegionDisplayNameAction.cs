using UnityEngine;

namespace FellOnline.Shared
{
	public class FRegionDisplayNameAction : FRegionAction
	{
		public Color displayColor;

		public override void Invoke( Character character, FRegion region)
		{
			if (region == null || character == null)
			{
				return;
			}
			//UILabel3D.Create(region.regionName, 24, displayColor, true, character.transform);
		}
	}
}