using System;
using UnityEngine;

namespace FellOnline.Shared
{
	[Serializable]
	public class FSceneBoundaryDictionary : SerializableDictionary<string, FSceneBoundaryDetails>
	{
		public bool PointContainedInBoundaries(Vector3 point)
		{
			// In the event we don't have any boundaries, best not to try and enforce them
			if (Count == 0) return true;

			foreach (FSceneBoundaryDetails details in Values)
			{
				if (details.ContainsPoint(point)) return true;
			}

			return false;
		}
	}
}