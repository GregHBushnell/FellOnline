using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New Name Cache", menuName = "FellOnline/Character/Name Cache", order = 1)]
	public class NameCache : FCachedScriptableObject<NameCache>, FICachedObject
	{
		public List<string> Names;
	}
}