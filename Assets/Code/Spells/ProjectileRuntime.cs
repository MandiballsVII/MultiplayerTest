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

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (spell == null) { Destroy(gameObject); return; }

        var targetFactionMember = col.GetComponent<IFactionMember>();
        var targetHealth = col.GetComponent<Health>();

        // Ignorar aliados completos
        if (targetFactionMember != null && targetFactionMember.Faction == projectileFaction.Faction)
            return;

        // Si es un enemigo, aplicar daño
        if (targetFactionMember != null && targetHealth != null)
        {
            targetHealth.TakeDamage(spell.power, transform.position);
            Debug.Log($"{name} golpeó a {col.name} por {spell.power} de daño.");
        }

        // Explosión opcional
        if (spell.explosionPrefab != null)
        {
            var explosion = Instantiate(spell.explosionPrefab, transform.position, Quaternion.identity);
            var explosionRuntime = explosion.GetComponent<ExplosionRuntime>();
            if (explosionRuntime != null)
                explosionRuntime.Init(caster, spell);

            var expFaction = explosion.GetComponent<ProjectileFaction>();
            if (expFaction != null)
                expFaction.Init(projectileFaction.Faction, projectileFaction.Owner);
        }

        // Que se destruya si no es un Arrow (misma lógica que tú)
        if (GetComponent<Arrow>() == null)
            Destroy(gameObject);
    }

}
