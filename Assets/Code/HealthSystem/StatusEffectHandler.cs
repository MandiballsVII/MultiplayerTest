using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StatusEffectHandler : MonoBehaviour
{
    private Health health;
    private IMovable movable;          // Para modificar velocidad
    private ISpellCaster spellCaster;  // Para silenciar, etc.

    public bool isInvulnerable = false;
    public bool isSilenced = false;
    public bool isStunned = false;

    private float originalSpeed;

    private bool invulActive = false; // control interno para debug

    void Awake()
    {
        health = GetComponent<Health>();
        movable = GetComponent<IMovable>();          // PlayerManager o NPCMovement implementarán IMovable
        spellCaster = GetComponent<ISpellCaster>();  // PlayerSpellBook o NPCSpellAI implementarán ISpellCaster

        if (movable != null)
            originalSpeed = movable.MoveSpeed;
    }


    // ======================
    //  EFECTOS GENERALES
    // ======================
    public void ApplyDamage(float amount, Vector2? hitSource)
    {
        if (isInvulnerable) return;
        health.TakeDamage(amount, hitSource);
    }

    public void ApplySlow(float multiplier, float duration)
    {
        StartCoroutine(SlowCoroutine(multiplier, duration));
    }

    public void ApplyStun(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }

    public void ApplySilence(float duration)
    {
        StartCoroutine(SilenceCoroutine(duration));
    }

    public void ApplyInvulnerability(float duration)
    {
        StartCoroutine(InvulnerabilityCoroutine(duration));
    }
    public void ApplyPoison(float dps, float duration)
    {
        StartCoroutine(PoisonCoroutine(dps, duration));
    }


    // ======================
    //  COROUTINAS
    // ======================
    private IEnumerator SlowCoroutine(float multiplier, float duration)
    {
        if (movable == null) yield break;

        movable.MoveSpeed = originalSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        movable.MoveSpeed = originalSpeed;
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;

        if (movable != null) movable.CanMove = false;

        yield return new WaitForSeconds(duration);

        if (movable != null) movable.CanMove = true;

        isStunned = false;
    }

    private IEnumerator SilenceCoroutine(float duration)
    {
        isSilenced = true;

        if (spellCaster != null) spellCaster.CanCast = false;

        yield return new WaitForSeconds(duration);

        if (spellCaster != null) spellCaster.CanCast = true;

        isSilenced = false;
    }

    private IEnumerator InvulnerabilityCoroutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }
    private IEnumerator PoisonCoroutine(float dps, float duration)
    {
        float timer = duration;
        float tickInterval = 1f;   // puedes hacerlo configurable

        while (timer > 0f)
        {
            timer -= tickInterval;

            if (!isInvulnerable && health != null)
                health.TakeDamage(dps, transform.position);

            yield return new WaitForSeconds(tickInterval);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying) return;

        // --- SILENCE ---
        if (isSilenced && spellCaster != null && spellCaster.CanCast)
        {
            Debug.Log("DEBUG: Silence activado desde el Inspector");
            ApplySilence(999f);    // silencio "infinito" hasta desactivar
        }
        if (!isSilenced && spellCaster != null && !spellCaster.CanCast)
        {
            Debug.Log("DEBUG: Silence DESACTIVADO desde el Inspector");
            spellCaster.CanCast = true;
            StopAllCoroutines();
        }

        // --- STUN ---
        if (isStunned && movable != null && movable.CanMove)
        {
            Debug.Log("DEBUG: Stun activado desde el Inspector");
            ApplyStun(999f);
        }
        if (!isStunned && movable != null && !movable.CanMove)
        {
            Debug.Log("DEBUG: Stun DESACTIVADO desde el Inspector");
            movable.CanMove = true;
            StopAllCoroutines();
        }

        // --- INVULNERABILITY ---
        if (isInvulnerable)
        {
            // Si ya está activado, no hagas nada
            if (!invulActive)
            {
                Debug.Log("DEBUG: Activando invulnerabilidad (modo debug)");
                StartCoroutine(InvulnerabilityCoroutine(999f));
                invulActive = true;
            }
        }
        else
        {
            // Si estaba activa por debug, desactivarla
            if (invulActive)
            {
                Debug.Log("DEBUG: Desactivando invulnerabilidad (modo debug)");
                StopAllCoroutines();
                isInvulnerable = false;
                invulActive = false;
            }
        }

    }
#endif

}
