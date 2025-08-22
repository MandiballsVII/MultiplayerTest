using UnityEngine;

public class SpellHit : MonoBehaviour
{
    private PlayerSpellbook owner;
    private SpellData spell;

    public void Init(PlayerSpellbook owner, SpellData data)
    {
        this.owner = owner;
        this.spell = data;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            //dmg.TakeDamage(spell.power);
            // VFX, sonido, etc.
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Walls"))
        {
            Destroy(gameObject);
        }
    }
}
