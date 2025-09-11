using System;
using UnityEngine;

public class Health : MonoBehaviour, IAttackable
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    public bool IsAlive => currentHealth > 0f;

    public event Action<float, float> OnHealthChanged; // (current, max)
    public event Action OnDied;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        OnDied?.Invoke();
        Destroy(gameObject);
    }
}
