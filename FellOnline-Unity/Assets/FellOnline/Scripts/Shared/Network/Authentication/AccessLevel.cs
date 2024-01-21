namespace FellOnline.Shared
{
	public enum AccessLevel : byte
	{
		Banned = 0,
		Player,
		Guide,
		GameMaster,
		Administrator,
	}
}