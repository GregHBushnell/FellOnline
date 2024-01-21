using FishNet.Broadcast;

namespace FellOnline.Shared
{
	public struct ChatBroadcast : IBroadcast
	{
		public ChatChannel channel;
		public long senderID;
		public string text;
	}
}