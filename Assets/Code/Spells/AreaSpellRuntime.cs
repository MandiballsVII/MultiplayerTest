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
            // Aqu� aplicas da�o peri�dico
            //var enemy = other.GetComponent<EnemyHealth>();
            //if (enemy != null)
            //{
            //    enemy.TakeDamage(spell.power * Time.deltaTime); // da�o por segundo
            //}
        }
        else
        {
            print("Area spell hit something else or nothing at all!");
        }
    }
}
