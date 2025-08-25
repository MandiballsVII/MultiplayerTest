using UnityEngine;

public class ProjectileRuntime : MonoBehaviour
{
    private PlayerSpellBook caster;
    private SpellData spell;

    public void Init(PlayerSpellBook owner, SpellData data)
    {
        caster = owner;
        spell = data;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Instanciar explosión si existe
        if (spell.explosionPrefab != null)
        {
            var explosion = Instantiate(spell.explosionPrefab, transform.position, Quaternion.identity);
            var explosionRuntime = explosion.GetComponent<ExplosionRuntime>();
            if (explosionRuntime != null)
                explosionRuntime.Init(caster, spell);
            // Aplica daño en área
            var hits = Physics2D.OverlapCircleAll(transform.position, spell.explosionRadius);
            foreach (var hit in hits)
            {
                //if (hit.TryGetComponent<EnemyHealth>(out var enemy))
                //    enemy.TakeDamage(spell.explosionDamage);
                print($"Hit {hit.name} for {spell.explosionDamage} damage.");
            }

            Destroy(explosion, 1.5f); // o spell.explosionDuration
        }

        // Feedback: VFX, sonido
        Destroy(gameObject);
    }
}
