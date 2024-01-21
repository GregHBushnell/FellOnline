using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	public class FSceneObjectUID : MonoBehaviour
	{
		public readonly static FSceneObjectUIDDictionary IDs = new FSceneObjectUIDDictionary();

		[Tooltip("Rebuild the World Scene Details Cache in order to generate Unique IDs for Scene Objects.")]
		public int ID = 0;

		protected void Awake()
		{
			IDs[ID] = this;
		}
	}
}