﻿using UnityEngine.UI;

namespace FellOnline.Client
{
	public class FUICrosshair : FUIControl
	{
		public Image Image;
		public override void OnStarting()
		{
			//FInputManager.OnToggleMouseMode += OnToggleMouseMode;
		}

		public override void OnDestroying()
		{
			//FInputManager.OnToggleMouseMode -= OnToggleMouseMode;
		}

		// public void OnToggleMouseMode(bool mouseMode)
		// {
		// 	if (mouseMode)
		// 	{
		// 		Hide();
		// 	}
		// 	else
		// 	{
		// 		Show();
		// 	}
		// }
	}
}