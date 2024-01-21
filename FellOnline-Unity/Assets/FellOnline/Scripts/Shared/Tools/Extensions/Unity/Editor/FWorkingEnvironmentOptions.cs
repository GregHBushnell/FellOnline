using System.IO;
using UnityEditor;

namespace FellOnline.Shared
{
	public enum FWorkingEnvironmentState
	{
		Development = 0,
		Release,
	}

	[InitializeOnLoad]
	public class FWorkingEnvironmentOptions
	{
		[MenuItem("FellOnline/Build/Environment/Release")]
		static void WorkingEnvironmentToggleOption0()
		{
			EditorPrefs.SetInt("FellOnlineWorkingEnvironmentToggle", (int)FWorkingEnvironmentState.Release);
		}

		[MenuItem("FellOnline/Build/Environment/Development")]
		static void WorkingEnvironmentToggleOption1()
		{
			EditorPrefs.SetInt("FellOnlineWorkingEnvironmentToggle", (int)FWorkingEnvironmentState.Development);
		}

		[MenuItem("FellOnline/Build/Environment/Release", true)]
		static bool WorkingEnvironmentValidation()
		{
			//Here, we uncheck all options before we show them
			Menu.SetChecked("FellOnline/Build/Environment/Development", false);
			Menu.SetChecked("FellOnline/Build/Environment/Release", false);

			FWorkingEnvironmentState status = (FWorkingEnvironmentState)EditorPrefs.GetInt("FellOnlineWorkingEnvironmentToggle");

			//Here, we put the checkmark on the current value of WorkingEnvironmentToggle
			switch (status)
			{
				case FWorkingEnvironmentState.Development:
					Menu.SetChecked("FellOnline/Build/Environment/Development", true);
					break;
				case FWorkingEnvironmentState.Release:
					Menu.SetChecked("FellOnline/Build/Environment/Release", true);
					break;
			}
			return true;
		}

		public static string AppendEnvironmentToPath(string path)
		{
			FWorkingEnvironmentState envState = (FWorkingEnvironmentState)EditorPrefs.GetInt("FellOnlineWorkingEnvironmentToggle");
			switch (envState)
			{
				case FWorkingEnvironmentState.Release:
					return Path.Combine(path, "Release");
				case FWorkingEnvironmentState.Development:
					return Path.Combine(path, "Development");
				default:
					return path;
			}
		}
	}
}