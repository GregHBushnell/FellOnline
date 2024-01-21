namespace FellOnline.Shared
{
	public interface FIDamageable
	{
		public void Damage(Character attacker, int amount, FDamageAttributeTemplate damageAttribute);
	}
}