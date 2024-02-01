namespace FellOnline.Shared
{
	public interface IDamageable
	{
		public void Damage(Character attacker, int amount, DamageAttributeTemplate damageAttribute);
	}
}