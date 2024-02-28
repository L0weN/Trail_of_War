using UnityEngine;
public interface IDamageable
{
    public int CurrentHealth { get; }
    public int MaxHealth { get; }

    public int CurrentArmor { get; }

    public int MaxArmor { get; }

    public delegate void TakeDamageEvent(int damage);
    public event TakeDamageEvent OnTakeDamage;

    public delegate void HealEvent(int heal);
    public event HealEvent OnHeal;

    public delegate void ArmorUpEvent(int armor);
    public event ArmorUpEvent OnArmorUp;

    public delegate void DeathEvent(Vector3 Position);
    public event DeathEvent OnDeath;

    public void TakeDamage(int damage);
    public void Heal(int heal);
    public void ArmorUp(int armor);
}
