using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class PlayerSpellbook : MonoBehaviour
{
    [Header("Slots")]
    public SpellData[] slots = new SpellData[4];   // Asigna en runtime/pickup
    private float[] slotCooldowns = new float[4];

    [Header("Casteo")]
    public Transform castPoint; // Asigna un child (p.ej. ShootPoint). Si null, usa transform.

    private PlayerController controller;
    private PlayerInput playerInput;
    private readonly List<SpellData> inventory = new(); // Para futuro menú de asignación

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();
        if (castPoint == null) castPoint = transform; // fallback
    }

    void Update()
    {
        for (int i = 0; i < slotCooldowns.Length; i++)
            if (slotCooldowns[i] > 0f) slotCooldowns[i] -= Time.deltaTime;
    }

    // === INPUT (vincula en PlayerInput del prefab del jugador) ===
    public void Ability1(InputAction.CallbackContext ctx) { if (ctx.performed) TryCast(0); }
    public void Ability2(InputAction.CallbackContext ctx) { if (ctx.performed) TryCast(1); }
    public void Ability3(InputAction.CallbackContext ctx) { if (ctx.performed) TryCast(2); }
    public void Ability4(InputAction.CallbackContext ctx) { if (ctx.performed) TryCast(3); }

    public void AddSpellToInventory(SpellData data)
    {
        if (!inventory.Contains(data)) inventory.Add(data);
        // De momento: auto-asigna al primer slot libre
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = data;
                Debug.Log($"[{name}] Asignado {data.displayName} al slot {i + 1}");
                return;
            }
        }
        // Si no hay hueco, podrías reemplazar el 1, o abrir UI de asignación
    }

    private void TryCast(int slotIndex)
    {
        var spell = slots[slotIndex];
        if (spell == null) return;
        if (slotCooldowns[slotIndex] > 0f) return;

        if (!controller.HasMana(spell.manaCost)) return;

        // consumir maná
        controller.UseMana(spell.manaCost);

        // dirección de casteo: usa dónde mira el player (tu PlayerManager rota el sprite)
        Vector2 dir = transform.right;

        // Ejecutar según tipo
        switch (spell.type)
        {
            case SpellType.Projectile:
                CastProjectile(spell, dir);
                break;
            case SpellType.Area:
                CastArea(spell);
                break;
            case SpellType.Self:
                CastSelf(spell);
                break;
            case SpellType.Summon:
                CastSummon(spell);
                break;
        }

        // aplica cooldown
        slotCooldowns[slotIndex] = spell.cooldown;
    }

    private void CastProjectile(SpellData spell, Vector2 dir)
    {
        if (spell.prefab == null) return;
        var go = Instantiate(spell.prefab, castPoint.position, Quaternion.identity);
        if (go.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = dir.normalized * spell.projectileSpeed;
        }
        // Componente opcional de daño en el proyectil (ver más abajo)
        var hit = go.GetComponent<SpellHit>();
        if (hit != null) hit.Init(this, spell);
    }

    private void CastArea(SpellData spell)
    {
        if (spell.prefab == null) return;
        var go = Instantiate(spell.prefab, castPoint.position, Quaternion.identity);
        var aoe = go.GetComponent<AreaSpellRuntime>();
        if (aoe != null) aoe.Init(this, spell);
        Destroy(go, spell.duration);
    }

    private void CastSelf(SpellData spell)
    {
        // ejemplo: curación
        controller.Heal(spell.power);
        // shields/buffs -> aquí o con un prefab buffer si prefieres efectos visuales
    }

    private void CastSummon(SpellData spell)
    {
        if (spell.prefab == null) return;
        Instantiate(spell.prefab, castPoint.position, Quaternion.identity);
        // Si la invocación necesita saber quién la invocó, pásale referencia
        //var summ = GetComponentInChildren<SummonRuntime>();
        // o un script en el prefab de la invocación que acepte “owner”
    }
}
