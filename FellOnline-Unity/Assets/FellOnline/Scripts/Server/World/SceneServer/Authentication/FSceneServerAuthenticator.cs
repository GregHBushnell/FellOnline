using FellOnline.Database.Npgsql;
using FellOnline.Shared;

namespace FellOnline.Server
{
	// Scene Server Authenticator, Scene Authenticator connects with basic password authentication.
	public class FSceneServerAuthenticator : FLoginServerAuthenticator
	{
		/// <summary>
		/// Executed when a player tries to login to the Scene Server.
		/// </summary>
		internal override FClientAuthenticationResult TryLogin(NpgsqlDbContext dbContext, FClientAuthenticationResult result, string username)
		{
			return FClientAuthenticationResult.SceneLoginSuccess;
		}
	}
}