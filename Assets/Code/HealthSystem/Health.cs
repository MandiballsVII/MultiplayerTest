using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Health : MonoBehaviour, IAttackable
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    public bool IsAlive => currentHealth > 0f;

    public event Action<float, float> OnHealthChanged; // (current, max)
    public event Action OnDied;
    public event Action<Vector2, float> OnDamaged; // (direction, amount)

    private Camera mainCamera;

    private void Awake()
    {
        currentHealth = maxHealth;
        mainCamera = Camera.main;
    }
    public void TakeDamage(float amount)
    {
        TakeDamage(amount, null);
    }

    public void TakeDamage(float amount, Vector2? hitSource = null)
    {
        if (!IsAlive) return;

        float prevHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Calcular dirección de impacto si hay fuente
        Vector2 hitDir = Vector2.zero;
        if (hitSource.HasValue)
            hitDir = ((Vector2)transform.position - hitSource.Value).normalized;

        // Notificar daño
        OnDamaged?.Invoke(hitDir, amount);

        // Camera Shake solo si es Player
        if (CompareTag("Player") && mainCamera != null)
        {
            var cameraShake = mainCamera.GetComponent<CameraShake>();
            if (cameraShake != null)
                StartCoroutine(cameraShake.Shake(0.2f, 0.3f));
        }

        // Damage Popup
        var popup = GetComponent<DamagePopupSpawner>();
        if (popup != null)
        {
            float pct = currentHealth / maxHealth;
            popup.CreatePopup(amount, pct);
        }

        // Muerte
        if (currentHealth <= 0)
            Die();
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
