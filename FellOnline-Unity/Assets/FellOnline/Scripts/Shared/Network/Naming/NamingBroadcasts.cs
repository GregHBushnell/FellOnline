using FishNet.Broadcast;

namespace FellOnline.Shared
{
	public struct NamingBroadcast : IBroadcast
	{
		public NamingSystemType type;
		public long id;
		public string name;
	}
}