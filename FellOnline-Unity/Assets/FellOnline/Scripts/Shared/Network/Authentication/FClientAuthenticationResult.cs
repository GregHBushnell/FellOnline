namespace FellOnline.Shared
{
	public enum FClientAuthenticationResult : byte
	{
		AccountCreated,
		SrpVerify,
		SrpProof,
		InvalidUsernameOrPassword,
		AlreadyOnline,
		Banned,
		LoginSuccess,
		WorldLoginSuccess,
		SceneLoginSuccess,
		ServerFull,
	}
}