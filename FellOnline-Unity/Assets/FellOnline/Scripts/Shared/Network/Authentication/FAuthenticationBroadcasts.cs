using FishNet.Broadcast;

namespace FellOnline.Shared
{
	public struct FCreateAccountBroadcast : IBroadcast
	{
		public string username;
		public string salt;
		public string verifier;
	}

	public struct SrpVerifyBroadcast : IBroadcast
	{
		public string s;
		public string publicEphemeral;
	}

	public struct SrpProofBroadcast : IBroadcast
	{
		public string proof;
	}

	public struct SrpSuccess : IBroadcast
	{
	}

	public struct ClientAuthResultBroadcast : IBroadcast
	{
		public FClientAuthenticationResult result;
	}
}