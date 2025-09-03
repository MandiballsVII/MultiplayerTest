using UnityEngine;

public abstract class SummonedBase : MonoBehaviour, IAttackable
{
    [Header("Stats")]
    public float maxHealth = 100f;

    public Transform Owner { get; private set; }   // El mago que lo invocó

    protected float currentHealth;
    public bool IsAlive { get; private set; } = true;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Inicializa este invocado con su invocador.
    /// </summary>
    public void Init(Transform owner)
    {
        Owner = owner;
    }

    // ========== SISTEMA DE VIDA ==========
    public virtual void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Heal(float amount)
    {
        if (!IsAlive) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    protected virtual void Die()
    {
        IsAlive = false;
        // Aquí podemos poner animación de muerte, partículas, etc.
        Destroy(gameObject);
    }
}
