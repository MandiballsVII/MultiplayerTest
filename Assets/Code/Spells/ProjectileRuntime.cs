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
        if (targetFactionMember != null && projectileFaction != null && targetFactionMember.Faction == projectileFaction.Faction)
            return;

        // Aplicar efectos de estado si hay StatusEffectHandler o aplicar daño directo
        var status = col.GetComponent<StatusEffectHandler>();

        if (status != null)
        {
            SpellEffectApplier.ApplyStatusEffects(spell, col, caster);
            // Si el spell tiene daño directo además de efectos, puedes aplicarlo:
            if (spell.power > 0 && (targetFactionMember == null || targetFactionMember.Faction != projectileFaction.Faction))
                status.ApplyDamage(spell.power, transform.position);
        }
        else if (targetHealth != null && (targetFactionMember == null || targetFactionMember.Faction != projectileFaction.Faction))
        {
            targetHealth.TakeDamage(spell.power, transform.position);
            // además aplicar efectos a colliders sin StatusEffectHandler -> se ignoran
            SpellEffectApplier.ApplyStatusEffects(spell, col, caster);
        }

        // Explosión opcional
        if (spell.explosionPrefab != null)
        {
            var explosion = Instantiate(spell.explosionPrefab, transform.position, Quaternion.identity);
            var explosionRuntime = explosion.GetComponent<ExplosionRuntime>();
            if (explosionRuntime != null)
                explosionRuntime.Init(caster, spell);

            var expFaction = explosion.GetComponent<ProjectileFaction>();
            if (expFaction != null && projectileFaction != null)
                expFaction.Init(projectileFaction.Faction, projectileFaction.Owner);
        }

        if (GetComponent<Arrow>() == null) // conservar comportamiento Arrow
            Destroy(gameObject);
    }
}
