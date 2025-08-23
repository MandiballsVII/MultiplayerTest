using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpellPickup : MonoBehaviour
{
    [Header("Runtime-assigned")]
    public SpellData data;                    // Lo pone el spawner
    public SpriteRenderer iconRenderer;       // Asigna en el prefab

    // Lo llama el spawner justo despu�s de Instantiate
    public void SetData(SpellData d)
    {
        data = d;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (iconRenderer != null && data != null)
            iconRenderer.sprite = data.icon;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Solo jugadores
        if (!other.TryGetComponent<PlayerController>(out var pc)) return;

        // Solo el mago de la misma escuela
        if (pc.CharacterData == null || data == null || pc.CharacterData.school != data.school) return;

        // OJO: aseg�rate de que el nombre de tu clase es exactamente el mismo que aqu�
        // (si tu script se llama PlayerSpellBook con B may�scula o PlayerSpellbook con b min�scula,
        // usa ese nombre)
        var book = other.GetComponent<PlayerSpellBook>();
        if (book == null) return;

        book.AddSpellToInventory(data);

        // feedback vfx/sfx aqu� si quieres
        Destroy(gameObject);
    }
}
