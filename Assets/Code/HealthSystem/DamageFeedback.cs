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

        // Bloquear control temporalmente: usar isStunned para players
        var player = GetComponent<PlayerManager>();
        var npc = GetComponent<NPC_ControllerBase>();
        if (player != null) player.isStunned = true;
        if (npc != null) npc.enabled = false;

        // Aplicar impulso (usar rb si existe)
        if (rb != null)
        {
            rb.velocity = dir * knockbackForce;
        }
        else
        {
            // fallback: mover por transform si no hay rigidbody
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + (Vector3)(dir * knockbackForce);
            while (elapsed < knockbackDuration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, targetPos, elapsed / knockbackDuration);
                yield return null;
            }
        }

        yield return new WaitForSeconds(knockbackDuration);

        // Fin del knockback
        if (rb != null) rb.velocity = Vector2.zero;

        if (player != null) player.isStunned = false;
        if (npc != null) npc.enabled = true;

        isKnocked = false;
    }
}
