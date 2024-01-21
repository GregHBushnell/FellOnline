namespace FellOnline.Shared
{
	public interface FIStackable<T>
	{
		bool CanAddToStack(T other);
		bool AddToStack(T other);
		bool TryUnstack(uint amount, out T stack);
	}
}