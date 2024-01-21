using UnityEngine;

namespace FellOnline.Shared
{
	public class FRegionChangeFogAction : FRegionAction
	{
		public bool fogEnabled = false;
		public FogMode fogMode = FogMode.Exponential;
		public Color fogColor = Color.gray;
		public float fogDensity = 0.0f;
		public float fogStartDistance = 0.0f;
		public float fogEndDistance = 0.0f;

		public override void Invoke(Character character, FRegion region)
		{
			RenderSettings.fog = fogEnabled;
			RenderSettings.fogMode = fogMode;
			RenderSettings.fogColor = fogColor;
			RenderSettings.fogDensity = fogDensity;
			RenderSettings.fogStartDistance = fogStartDistance;
			RenderSettings.fogEndDistance = fogEndDistance;
		}
	}
}