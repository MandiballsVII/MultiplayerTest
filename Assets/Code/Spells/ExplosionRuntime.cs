using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ExplosionRuntime : MonoBehaviour
{
    private PlayerSpellBook caster;
    private SpellData spell;
    private CircleCollider2D circleCollider;

    public void Init(PlayerSpellBook owner, SpellData data)
    {
        caster = owner;
        spell = data;

        // Ajustar el tamaño del collider al radio
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = spell.explosionRadius;

        // Aplicar daño instantáneo
        DoDamage();

        // Destruir tras la duración visual (usa spell.duration o fija uno corto)
        Destroy(gameObject, spell.duration > 0 ? spell.duration : 0.5f);
    }

    private void DoDamage()
    {
        var factionComp = GetComponent<ProjectileFaction>();
        Faction selfFaction = factionComp != null ? factionComp.Faction : Faction.Enemy;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, spell.explosionRadius);

        foreach (var hit in hits)
        {
            Debug.Log("EXPLOSION -> " + hit.name);

            if (hit.gameObject == gameObject)
                continue; // no autogolpearse

            var targetFaction = hit.GetComponent<IFactionMember>();
            if (targetFaction == null)
                continue;

            // evitar daño a la misma facción
            if (targetFaction.Faction == selfFaction)
                continue;

            var status = hit.GetComponent<StatusEffectHandler>();
            var health = hit.GetComponent<Health>();

            // prioridad: StatusEffectHandler
            if (status != null)
            {
                status.ApplyDamage(spell.explosionDamage, transform.position);
                SpellEffectApplier.ApplyStatusEffects(spell, hit, caster);
                continue;
            }

            // fallback de daño si no existe status handler
            if (health != null)
            {
                health.TakeDamage(spell.explosionDamage, transform.position, spell);
                SpellEffectApplier.ApplyStatusEffects(spell, hit, caster);
            }
        }
    }



    // Para ver el radio en la escena
    private void OnDrawGizmosSelected()
    {
        if (spell != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spell.explosionRadius);
        }
    }
}
