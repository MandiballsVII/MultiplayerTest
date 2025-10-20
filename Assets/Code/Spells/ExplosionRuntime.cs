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
        var selfFaction = GetComponent<IFactionMember>()?.Faction ?? Faction.Enemy;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, spell.explosionRadius);

        foreach (var hit in hits)
        {
            var targetFactionMember = hit.GetComponent<IFactionMember>();
            var targetHealth = hit.GetComponent<Health>();
            if (targetFactionMember == null || targetHealth == null) continue;

            if (targetFactionMember.Faction != selfFaction)
            {
                targetHealth.TakeDamage(spell.explosionDamage);
                Debug.Log($"{name} explosion dañó a {hit.name} por {spell.explosionDamage}");
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
