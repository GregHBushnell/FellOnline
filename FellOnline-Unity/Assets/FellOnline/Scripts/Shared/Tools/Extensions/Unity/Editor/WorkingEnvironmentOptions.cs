using System.IO;
using UnityEditor;

namespace FellOnline.Shared
{
	public enum WorkingEnvironmentState
	{
		Development = 0,
		Release,
	}

	[InitializeOnLoad]
	public class WorkingEnvironmentOptions
	{
		[MenuItem("FellOnline/Build/Environment/Release")]
		static void WorkingEnvironmentToggleOption0()
		{
			EditorPrefs.SetInt("FellOnlineWorkingEnvironmentToggle", (int)WorkingEnvironmentState.Release);
		}

		[MenuItem("FellOnline/Build/Environment/Development")]
		static void WorkingEnvironmentToggleOption1()
		{
			EditorPrefs.SetInt("FellOnlineWorkingEnvironmentToggle", (int)WorkingEnvironmentState.Development);
		}

		[MenuItem("FellOnline/Build/Environment/Release", true)]
		static bool WorkingEnvironmentValidation()
		{
			//Here, we uncheck all options before we show them
			Menu.SetChecked("FellOnline/Build/Environment/Development", false);
			Menu.SetChecked("FellOnline/Build/Environment/Release", false);

			WorkingEnvironmentState status = (WorkingEnvironmentState)EditorPrefs.GetInt("FellOnlineWorkingEnvironmentToggle");

			//Here, we put the checkmark on the current value of WorkingEnvironmentToggle
			switch (status)
			{
				case WorkingEnvironmentState.Development:
					Menu.SetChecked("FellOnline/Build/Environment/Development", true);
					break;
				case WorkingEnvironmentState.Release:
					Menu.SetChecked("FellOnline/Build/Environment/Release", true);
					break;
			}
			return true;
		}

		public static string AppendEnvironmentToPath(string path)
		{
			WorkingEnvironmentState envState = (WorkingEnvironmentState)EditorPrefs.GetInt("FellOnlineWorkingEnvironmentToggle");
			switch (envState)
			{
				case WorkingEnvironmentState.Release:
					return Path.Combine(path, "Release");
				case WorkingEnvironmentState.Development:
					return Path.Combine(path, "Development");
				default:
					return path;
			}
		}
	}
}