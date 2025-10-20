using UnityEngine;

public class AreaSpellRuntime : MonoBehaviour
{
    private PlayerSpellBook owner;
    private SpellData spell;

    public void Init(PlayerSpellBook owner, SpellData spell)
    {
        this.owner = owner;
        this.spell = spell;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (spell == null) return;

        var selfFaction = GetComponent<IFactionMember>()?.Faction ?? Faction.Enemy;
        var targetFactionMember = other.GetComponent<IFactionMember>();
        var targetHealth = other.GetComponent<Health>();

        if (targetFactionMember == null || targetHealth == null) return;

        if (targetFactionMember.Faction != selfFaction)
        {
            targetHealth.TakeDamage(spell.power * Time.deltaTime);
        }
    }

}
