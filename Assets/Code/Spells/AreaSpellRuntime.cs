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
        if (other.CompareTag("Enemy"))
        {
            print("Area spell hit an enemy!");
            // Aquí aplicas daño periódico
            //var enemy = other.GetComponent<EnemyHealth>();
            //if (enemy != null)
            //{
            //    enemy.TakeDamage(spell.power * Time.deltaTime); // daño por segundo
            //}
        }
        else
        {
            print("Area spell hit something else or nothing at all!");
        }
    }
}
