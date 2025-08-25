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

        // Ajustar el tama�o del collider al radio
        circleCollider = GetComponent<CircleCollider2D>();
        circleCollider.isTrigger = true;
        circleCollider.radius = spell.explosionRadius;

        // Aplicar da�o instant�neo
        DoDamage();

        // Destruir tras la duraci�n visual (usa spell.duration o fija uno corto)
        Destroy(gameObject, spell.duration > 0 ? spell.duration : 0.5f);
    }

    private void DoDamage()
    {
        // Detecci�n en �rea
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, spell.explosionRadius);

        foreach (var hit in hits)
        {
            //if (hit.CompareTag("Player")) continue; // Evita da�ar al jugador si no quieres FF

            //if (hit.TryGetComponent<EnemyHealth>(out var enemy))
            //{
            //    enemy.TakeDamage(spell.explosionDamage);
            //}
            print($"Hit {hit.name} for {spell.explosionDamage} damage.");
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
