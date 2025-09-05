using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IAttackable
{
    [Header("Stats")]
    public float maxHealth = 200f;
    protected float currentHealth;

    public bool IsAlive => currentHealth > 0f;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} recibió {amount} de daño. Vida restante: {currentHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} ha muerto.");
        Destroy(gameObject);
    }
}
