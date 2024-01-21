using FellOnline.Shared;

namespace FellOnline.Server
{
	public class FAccountData
	{
		public AccessLevel AccessLevel { get; private set; }
		public FServerSrpData SrpData { get; private set; }

		public FAccountData(AccessLevel accessLevel, FServerSrpData srpData)
		{
			AccessLevel = accessLevel;
			SrpData = srpData;
		}
	}
}