using FellOnline.Shared;

namespace FellOnline.Server
{
	public class AccountData
	{
		public AccessLevel AccessLevel { get; private set; }
		public ServerSrpData SrpData { get; private set; }

		public AccountData(AccessLevel accessLevel, ServerSrpData srpData)
		{
			AccessLevel = accessLevel;
			SrpData = srpData;
		}
	}
}