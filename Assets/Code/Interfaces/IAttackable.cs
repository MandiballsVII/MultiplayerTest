public interface IAttackable
{
    void TakeDamage(float amount);

    void Heal(float amount);
    bool IsAlive { get; }
}
