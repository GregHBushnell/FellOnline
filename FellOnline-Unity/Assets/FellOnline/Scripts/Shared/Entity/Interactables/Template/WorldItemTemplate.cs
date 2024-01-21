using System.Collections.Generic;
using UnityEngine;

namespace FellOnline.Shared
{
	[CreateAssetMenu(fileName = "New World Item", menuName = "FellOnline/Item/WorldItem", order = 1)]
	public class WorldItemTemplate : FCachedScriptableObject<WorldItemTemplate>, FICachedObject
	{
		public Sprite icon;
		public string Description;
		public FBaseItemTemplate Item;

		public uint amount=1;


		public string Name { get { return this.name; } }

		public Sprite Icon { get { return this.icon; } }
		
	}
}