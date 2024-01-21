using FellOnline.Database.Npgsql;
using FellOnline.Server.DatabaseServices;
using FellOnline.Shared;

namespace FellOnline.Server
{
	// World that allows clients to connect with basic password authentication.
	public class FWorldServerAuthenticator : FLoginServerAuthenticator
	{
		public uint MaxPlayers = 5000;

		public FWorldSceneSystem WorldSceneSystem { get; set; }

		internal override FClientAuthenticationResult TryLogin(NpgsqlDbContext dbContext, FClientAuthenticationResult result, string username)
		{
			if (WorldSceneSystem != null && WorldSceneSystem.ConnectionCount >= MaxPlayers)
			{
				return FClientAuthenticationResult.ServerFull;
			}
			else if (dbContext == null)
			{
				return FClientAuthenticationResult.InvalidUsernameOrPassword;
			}
			else if (result == FClientAuthenticationResult.LoginSuccess &&
					 FCharacterService.GetSelected(dbContext, username))
			{
				// update the characters world
				FCharacterService.SetWorld(dbContext, username, WorldSceneSystem.Server.WorldServerSystem.ID);

				return FClientAuthenticationResult.WorldLoginSuccess;
			}
			return result;
		}
	}
}