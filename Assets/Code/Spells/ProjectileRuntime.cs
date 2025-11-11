using UnityEngine;

public class ProjectileRuntime : MonoBehaviour
{
    private PlayerSpellBook caster;
    private SpellData spell;
    private ProjectileFaction projectileFaction;

    public void Init(PlayerSpellBook owner, SpellData data)
    {
        caster = owner;
        spell = data;
        projectileFaction = GetComponent<ProjectileFaction>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (spell == null) { Destroy(gameObject); return; }

        // Intentar obtener la facción del objetivo
        var targetFactionMember = collision.gameObject.GetComponent<IFactionMember>();
        var targetHealth = collision.gameObject.GetComponent<Health>();

        if (targetFactionMember != null && targetHealth != null)
        {
            // Comparar facciones
            if (targetFactionMember.Faction != projectileFaction.Faction)
            {
                targetHealth.TakeDamage(spell.power, transform.position);
                Debug.Log($"{name} golpeó a {collision.gameObject.name} por {spell.power} de daño.");
            }
        }

        // Explosión opcional
        if (spell.explosionPrefab != null)
        {
            var explosion = Instantiate(spell.explosionPrefab, transform.position, Quaternion.identity);
            var explosionRuntime = explosion.GetComponent<ExplosionRuntime>();
            if (explosionRuntime != null)
                explosionRuntime.Init(caster, spell);

            // Asignar también la facción a la explosión

            var expFaction = explosion.GetComponent<ProjectileFaction>();
            if (expFaction != null)
                expFaction.Init(projectileFaction.Faction, projectileFaction.Owner);
        }
        if(gameObject.GetComponent<Arrow>() == null)
        {
            Destroy(gameObject);
        }
    }
}
