using UnityEngine;

public static class SpellEffectApplier
{
    // Aplica todos los statusEffects de spell al target (Collider2D).
    // caster puede ser null (por ejemplo para explosiones de entorno).
    public static void ApplyStatusEffects(SpellData spell, Collider2D target, PlayerSpellBook caster = null)
    {
        if (spell == null || target == null || spell.statusEffects == null || spell.statusEffects.Count == 0) return;

        var status = target.GetComponent<StatusEffectHandler>();
        var health = target.GetComponent<Health>();

        foreach (var eff in spell.statusEffects)
        {
            if (eff == null || eff.type == StatusType.None) continue;

            switch (eff.type)
            {
                case StatusType.Slow:
                    status?.ApplySlow(eff.magnitude, eff.duration);
                    break;

                case StatusType.Accelerate:
                    status?.ApplyAccelerate(eff.magnitude, eff.duration);
                    break;

                case StatusType.Stun:
                    status?.ApplyStun(eff.duration);
                    break;

                case StatusType.Silence:
                    status?.ApplySilence(eff.duration);
                    break;

                case StatusType.Invulnerability:
                    status?.ApplyInvulnerability(eff.duration);
                    break;

                case StatusType.DamageReduction:
                    status?.ApplyDamageReduction(eff.magnitude, eff.duration);
                    break;

                case StatusType.Poison:
                    // magnitude = damage per tick (segundo) || ajustar si prefieres total
                    status?.ApplyPoison(eff.magnitude, eff.duration);
                    break;
            }
        }
    }
    public static void ApplyRandomEffect(SpellData spell, Collider2D target, Vector2 hitPos)
    {
        if (spell == null || spell.statusEffects == null || spell.statusEffects.Count == 0)
            return;

        var status = target.GetComponent<StatusEffectHandler>();
        var health = target.GetComponent<Health>();

        // Elegir un efecto aleatorio
        var eff = spell.statusEffects[Random.Range(0, spell.statusEffects.Count)];

        switch (eff.type)
        {
            case StatusType.None:
                if (health != null)
                    health.TakeDamage(spell.power, hitPos, spell);
                break;

            case StatusType.Slow:
                status?.ApplySlow(eff.magnitude, eff.duration);
                break;

            case StatusType.Stun:
                status?.ApplyStun(eff.duration);
                break;

            case StatusType.Silence:
                status?.ApplySilence(eff.duration);
                break;

            case StatusType.Poison:
                status?.ApplyPoison(eff.magnitude, eff.duration);
                break;
        }
    }

}
