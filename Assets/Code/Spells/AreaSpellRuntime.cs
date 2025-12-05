using System.Collections.Generic;
using UnityEngine;

public class AreaSpellRuntime : MonoBehaviour
{
    private PlayerSpellBook owner;
    public SpellData spell;
    private float timer;

    private readonly HashSet<Health> targetsInside = new();

    private Animator animator;

    public void Init(PlayerSpellBook owner, SpellData spell)
    {
        this.owner = owner;
        this.spell = spell;
        timer = 0f;
    }

    private void Update()
    {
        if (spell == null) return;
        timer -= Time.deltaTime;

        // Solo cuando toca aplicar daño
        if (timer <= 0f)
        {
            ApplyDamageTick();
            timer = spell.damageInterval > 0 ? spell.damageInterval : 1f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var targetHealth = other.GetComponent<Health>();
        if (targetHealth != null)
            targetsInside.Add(targetHealth);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var targetHealth = other.GetComponent<Health>();
        if (targetHealth != null)
            targetsInside.Remove(targetHealth);
    }

    private void ApplyDamageTick()
    {
        var selfFaction = GetComponent<IFactionMember>()?.Faction ?? Faction.Enemy;

        // Creamos una copia temporal para evitar modificación durante iteración
        var snapshot = new List<Health>(targetsInside);

        foreach (var health in snapshot)
        {
            if (health == null)
            {
                targetsInside.Remove(health);
                continue;
            }

            var factionMember = health.GetComponent<IFactionMember>();
            if (factionMember == null)
            {
                targetsInside.Remove(health);
                continue;
            }

            if (factionMember.Faction == selfFaction)
                continue;

            health.TakeDamage(spell.power, transform.position, spell);
            SpellEffectApplier.ApplyStatusEffects(spell, health.GetComponent<Collider2D>(), owner);
        }
    }

    // Para debug visual en editor
    private void OnDrawGizmosSelected()
    {
        if (spell != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spell.areaRadius);
        }
    }

}
