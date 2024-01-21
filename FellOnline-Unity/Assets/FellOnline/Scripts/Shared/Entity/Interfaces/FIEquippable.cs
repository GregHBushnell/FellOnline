namespace FellOnline.Shared
{
	public interface FIEquippable<T>
	{
		T Owner { get; }
		void Equip(T owner);
		void Unequip();
	}
}