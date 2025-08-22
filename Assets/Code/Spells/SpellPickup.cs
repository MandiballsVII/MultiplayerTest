using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpellPickup : MonoBehaviour
{
    public SpellData data;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerController>(out var pc)) return;

        // Solo el mago de la misma escuela
        if (pc.CharacterData == null || pc.CharacterData.school != data.school) return;

        var book = other.GetComponent<PlayerSpellbook>();
        if (book == null) return;

        book.AddSpellToInventory(data);

        // feedback vfx/sfx aquí
        Destroy(gameObject);
    }
}
