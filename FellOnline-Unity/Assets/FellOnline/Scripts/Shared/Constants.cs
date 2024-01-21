using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace FellOnline.Shared
{
	public static class Constants
	{
		public static class Configuration
		{
			public static readonly string ProjectName = "FellOnline";
			public static readonly string SetupDirectory = "FellOnline-Setup";

			public static readonly string DatabaseDirectory = "FellOnline-Database";
			public static readonly string DatabaseProjectDirectory = "FellOnline-DB";
			public static readonly string DatabaseMigratorProjectDirectory = "FellOnline-DB-Migrator";
			public static readonly string ProjectPath = "." + Path.DirectorySeparatorChar + "FellOnline-Database" + Path.DirectorySeparatorChar + "FellOnline-DB" + Path.DirectorySeparatorChar + "FellOnline-DB.csproj";
			public static readonly string StartupProject = "." + Path.DirectorySeparatorChar + "FellOnline-Database" + Path.DirectorySeparatorChar + "FellOnline-DB-Migrator" + Path.DirectorySeparatorChar + "FellOnline-DB-Migrator.csproj";


			public static readonly string InstallerPath = "Assets" + Path.DirectorySeparatorChar + "FellOnline" + Path.DirectorySeparatorChar + "Scenes" + Path.DirectorySeparatorChar + "Installer.unity";
			public static readonly string BootstrapScenePath = "Assets/FellOnline/Scenes/Bootstraps/";
			public static readonly string WorldScenePath = "Assets/FellOnline/Scenes/WorldScene/";
		}

		public static class Layers
		{
			public static readonly LayerMask Default = LayerMask.NameToLayer("Default");
			public static readonly LayerMask Ground = LayerMask.NameToLayer("Ground");
			public static readonly LayerMask LocalEntity = LayerMask.NameToLayer("LocalEntity");
		}

		public static class Authentication
		{
			public const int AccountNameMinLength = 3;
			public const int AccountNameMaxLength = 32;

			public const int AccountPasswordMinLength = 3;
			public const int AccountPasswordMaxLength = 32;

			public const int CharacterNameMinLength = 3;
			public const int CharacterNameMaxLength = 32;

			public const int MaxGuildNameLength = 64;

			public static bool IsAllowedUsername(string accountName)
			{
				return !string.IsNullOrWhiteSpace(accountName) &&
					   accountName.Length >= AccountNameMinLength &&
					   accountName.Length <= AccountNameMaxLength &&
					   Regex.IsMatch(accountName, @"^[a-zA-Z0-9_]+$");
			}

			public static bool IsAllowedPassword(string accountPassword)
			{
				return !string.IsNullOrWhiteSpace(accountPassword) &&
					   accountPassword.Length >= AccountPasswordMinLength &&
					   accountPassword.Length <= AccountPasswordMaxLength &&
					   Regex.IsMatch(accountPassword, @"^[a-zA-Z0-9_]+$");
			}

			public static bool IsAllowedCharacterName(string characterName)
			{
				return !string.IsNullOrWhiteSpace(characterName) &&
					   characterName.Length >= CharacterNameMinLength &&
					   characterName.Length <= CharacterNameMaxLength &&
					   Regex.IsMatch(characterName, @"^[A-Za-z]+(?: [A-Za-z]+){0,2}$");
			}

			public static bool IsAllowedGuildName(string guildName)
			{
				return !string.IsNullOrWhiteSpace(guildName) &&
						guildName.Length <= MaxGuildNameLength &&
						Regex.IsMatch(guildName, @"^[A-Za-z]+(?: [A-Za-z]+){0,2}$");
			}
		}
	}
}