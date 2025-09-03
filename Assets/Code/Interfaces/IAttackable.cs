public interface IAttackable
{
    void TakeDamage(float amount);
    bool IsAlive { get; }
}
