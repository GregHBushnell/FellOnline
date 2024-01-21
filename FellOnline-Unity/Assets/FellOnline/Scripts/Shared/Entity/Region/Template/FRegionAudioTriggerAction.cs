using UnityEngine;

namespace FellOnline.Shared
{
	public class FRegionAudioTriggerAction : FRegionAction
	{
		public AudioClip clip;

		public override void Invoke(Character character, FRegion region)
		{
			if (clip == null || character == null)
			{
				return;
			}
			// play audio clip?
		}
	}
}