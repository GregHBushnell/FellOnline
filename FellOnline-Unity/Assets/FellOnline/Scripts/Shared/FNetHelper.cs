using System.Net;

namespace FellOnline.Shared
{
	public static class FNetHelper
	{
		public static IPAddress GetExternalIPAddress(string serviceUrl = "https://checkip.amazonaws.com/")
		{
			string ipAddress = null;
			using (WebClient client = new WebClient())
			{
				ipAddress = client.DownloadString(serviceUrl).Replace("\\r\\n", "").Replace("\\n", "").Trim();
			}
			if (IPAddress.TryParse(ipAddress, out IPAddress address))
			{
				return address;
			}
			return null;
		}
	}
}