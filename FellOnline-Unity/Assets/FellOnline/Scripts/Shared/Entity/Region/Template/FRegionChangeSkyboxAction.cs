using UnityEngine;

namespace FellOnline.Shared
{
	public class FRegionChangeSkyboxAction : FRegionAction
	{
		public Material material;

		public override void Invoke(Character character, FRegion region)
		{
			if (material != null)
				RenderSettings.skybox = material;
		}
	}
}