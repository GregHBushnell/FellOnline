namespace FellOnline.Client
{
	public class FUIMenu : FUIControl
	{
		public override void OnStarting()
		{
		}

		public override void OnDestroying()
		{
		}

		public void OnButtonOptions()
		{
		}

		public void OnButtonQuitToLogin()
		{
			Client.QuitToLogin();
		}

		public void OnButtonQuit()
		{
			Client.Quit();
		}
	}
}