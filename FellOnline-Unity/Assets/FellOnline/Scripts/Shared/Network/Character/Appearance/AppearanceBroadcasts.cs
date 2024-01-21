using System.Collections.Generic;
using FishNet.Broadcast;

namespace FellOnline.Shared
{
	public struct AppearanceUpdateBroadcast : IBroadcast
	{
        public int SkinColor;
		public int HairID;
		public int HairColor;
	}
}