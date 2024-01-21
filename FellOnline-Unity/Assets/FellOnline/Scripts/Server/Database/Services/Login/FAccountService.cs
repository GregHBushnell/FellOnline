using System;
using System.Linq;
using FellOnline.Database.Npgsql;
using FellOnline.Database.Npgsql.Entities;
using FellOnline.Shared;

namespace FellOnline.Server.DatabaseServices
{
    public class FAccountService
    {
        public static FClientAuthenticationResult TryCreate(NpgsqlDbContext dbContext, string accountName, string salt, string verifier)
        {
		if (Constants.Authentication.IsAllowedUsername(accountName) && !string.IsNullOrWhiteSpace(salt) && !string.IsNullOrWhiteSpace(verifier))
			{
				var accountEntity = dbContext.Accounts.FirstOrDefault(a => a.Name == accountName);
				if (accountEntity == null)
				{
					dbContext.Accounts.Add(new AccountEntity()
					{
						Name = accountName,
						Salt = salt,
						Verifier = verifier,
						Created = DateTime.UtcNow,
						Lastlogin = DateTime.UtcNow,
						AccessLevel = (byte)AccessLevel.Player,
					});
					dbContext.SaveChanges();
					return FClientAuthenticationResult.AccountCreated;
				}
			}

			return FClientAuthenticationResult.InvalidUsernameOrPassword;
		}

        public static FClientAuthenticationResult Get(NpgsqlDbContext dbContext, string accountName, out string salt, out string verifier, out AccessLevel accessLevel)
        {
			salt = "";
			verifier = "";
			accessLevel = AccessLevel.Banned;

			if (Constants.Authentication.IsAllowedUsername(accountName))
            {
                var accountEntity = dbContext.Accounts.FirstOrDefault(a => a.Name == accountName);
                if (accountEntity == null)
                {
					return FClientAuthenticationResult.InvalidUsernameOrPassword;
                }
				else if ((AccessLevel)accountEntity.AccessLevel == AccessLevel.Banned)
				{
					return FClientAuthenticationResult.Banned;
				}
				else
				{
					salt = accountEntity.Salt;
					verifier = accountEntity.Verifier;
					accessLevel = (AccessLevel)accountEntity.AccessLevel;
					accountEntity.Lastlogin = DateTime.UtcNow;
					dbContext.SaveChanges();

					// proceed to SrpVerify stage
					return FClientAuthenticationResult.SrpVerify;
				}
            }
			return FClientAuthenticationResult.InvalidUsernameOrPassword;
        }
    }
}