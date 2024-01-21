using FishNet.Broadcast;

namespace FellOnline.Shared
{
	public struct FNamingBroadcast : IBroadcast
	{
		public FNamingSystemType type;
		public long id;
		public string name;
	}
}