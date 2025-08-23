using UnityEngine;

public class AreaSpellRuntime : MonoBehaviour
{
    private PlayerSpellBook owner;
    private SpellData spell;

    public void Init(PlayerSpellBook owner, SpellData data)
    {
        this.owner = owner;
        this.spell = data;

        // Ajusta el tamaño visual/colisionador al radio
        var col = GetComponent<CircleCollider2D>();
        if (col != null) col.radius = spell.areaRadius;
        transform.localScale = Vector3.one * (spell.areaRadius * 2f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            //dmg.TakeDamage(spell.power);
        }
    }
}
