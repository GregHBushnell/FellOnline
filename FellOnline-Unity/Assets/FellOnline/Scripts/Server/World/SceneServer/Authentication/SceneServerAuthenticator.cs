using FellOnline.Database.Npgsql;
using FellOnline.Shared;

namespace FellOnline.Server
{
	// Scene Server Authenticator, Scene Authenticator connects with basic password authentication.
	public class SceneServerAuthenticator : LoginServerAuthenticator
	{
		/// <summary>
		/// Executed when a player tries to login to the Scene Server.
		/// </summary>
		internal override ClientAuthenticationResult TryLogin(NpgsqlDbContext dbContext, ClientAuthenticationResult result, string username)
		{
			return ClientAuthenticationResult.SceneLoginSuccess;
		}
	}
}