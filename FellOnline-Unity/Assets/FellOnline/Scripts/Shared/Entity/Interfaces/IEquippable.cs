namespace FellOnline.Shared
{
	public interface IEquippable<T>
	{
		void Equip(T owner);
		void Unequip();
	}
}