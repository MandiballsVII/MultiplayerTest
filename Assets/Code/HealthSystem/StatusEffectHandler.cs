using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StatusEffectHandler : MonoBehaviour
{
    private Health health;
    private IMovable movable;          // Para modificar velocidad
    private ISpellCaster spellCaster;  // Para silenciar, etc.

    public bool isInvulnerable = false;
    public bool isSilenced = false;
    public bool isStunned = false;

    private float originalSpeed;

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
}
