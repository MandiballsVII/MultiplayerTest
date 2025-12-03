using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DamageFeedback : MonoBehaviour
{
    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.25f;

    [Header("Flash Settings")]
    public float flashDuration = 1f;        // Duración total del parpadeo
    public float flashInterval = 0.1f;      // Intervalo entre parpadeos

    private Health health;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isKnocked = false;

    void Awake()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    void OnEnable() => health.OnDamaged += ApplyFeedback;
    void OnDisable() => health.OnDamaged -= ApplyFeedback;

    private void ApplyFeedback(Vector2 hitDir, float dmg)
    {
        if (!health.IsAlive) return;

        if (sr != null)
            StartCoroutine(FlashCoroutine());

        if (rb != null && hitDir != Vector2.zero)
            StartCoroutine(KnockbackCoroutine(hitDir));
    }

    private IEnumerator FlashCoroutine()
    {
        Color original = sr.color;
        float elapsed = 0f;
        bool toggle = false;

        while (elapsed < flashDuration)
        {
            sr.color = toggle ? Color.white : original;
            toggle = !toggle;
            elapsed += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        sr.color = original;
    }

    private IEnumerator KnockbackCoroutine(Vector2 dir)
    {
        if (isKnocked) yield break;
        isKnocked = true;

        var npc = GetComponent<NPC_ControllerBase>();
        var player = GetComponent<PlayerManager>();

        // Desactivar control de movimiento
        if (npc != null) npc.enabled = false;
        if (player != null) player.isStunned = true;

        // Guardar modo original del Rigidbody
        RigidbodyType2D originalBodyType = rb.bodyType;

        // Pasamos a dinámico para aplicar físicas reales
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = Vector2.zero;

        // Aplicar fuerza real
        rb.AddForce(dir.normalized * knockbackForce, ForceMode2D.Impulse);

        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Detener movimiento
        rb.velocity = Vector2.zero;

        // Volver al modo original (kinematic para NPC, dynamic para jugadores)
        rb.bodyType = originalBodyType;

        // Restaurar control
        if (npc != null) npc.enabled = true;
        if (player != null) player.isStunned = false;

        isKnocked = false;
    }

}
