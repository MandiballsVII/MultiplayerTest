using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class PlayerSpellBook : MonoBehaviour
{
    [Header("Slots")]
    public SpellData[] slots = new SpellData[4];   // Asigna en runtime/pickup
    private float[] slotCooldowns = new float[4];

    [Header("Casteo")]
    public Transform castPoint; // Asigna un child (p.ej. ShootPoint). Si null, usa transform.

    private PlayerController controller;
    private PlayerInput playerInput;
    private PlayerAim playerAim;
    private readonly List<SpellData> inventory = new(); // Para futuro menú de asignación

    private bool isCastingChannel; // indica si hay un hechizo activo canalizado
    private GameObject activeChannelGO; // referencia al prefab instanciado
    private SpellData activeChannelSpell; // referencia al hechizo

    public SpellData defaultProjectileSpell;

    private PlayerSpellUI spellUI; // Referencia al HUD

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();
        playerAim = GetComponent<PlayerAim>();
        if (castPoint == null) castPoint = playerAim.shootPoint; // fallback

        spellUI = FindObjectOfType<PlayerSpellUI>(); // Busca la UI en escena
    }

    void Start()
    {
        if (slots[0] == null && defaultProjectileSpell != null)
            StartCoroutine(AssignDefaultSpellNextFrame());
    }

    private IEnumerator AssignDefaultSpellNextFrame()
    {
        yield return null; // espera 1 frame
        AddSpellToInventory(defaultProjectileSpell);
    }

    void Update()
    {
        for (int i = 0; i < slotCooldowns.Length; i++)
            if (slotCooldowns[i] > 0f) slotCooldowns[i] -= Time.deltaTime;
    }

    // === INPUT (vincula en PlayerInput del prefab del jugador) ===
    public void Projectile(InputAction.CallbackContext ctx) { if (ctx.performed) TryCast(0); }
    public void Area(InputAction.CallbackContext ctx) { if (ctx.performed) TryCast(1); }
    public void Self(InputAction.CallbackContext ctx) { if (ctx.performed) TryCast(2); }
    public void Summon(InputAction.CallbackContext ctx) { if (ctx.performed) TryCast(3); }

    public void AddSpellToInventory(SpellData spell)
    {
        if (!inventory.Contains(spell)) inventory.Add(spell);

        int slotIndex = GetSlotIndexForType(spell.type);
        if (slotIndex < 0 || slotIndex >= slots.Length)
        {
            Debug.LogWarning($"[{name}] No hay slot para el tipo {spell.type}");
            return;
        }

        slots[slotIndex] = spell;
        Debug.Log($"[{name}] Asignado {spell.displayName} al slot {slotIndex + 1}");

        // Actualizar UI
        var spellUI = HUDManager.Instance.GetSpellUI(controller.PlayerInput);
        if (spellUI != null)
        {
            spellUI.UpdateSpellIcon(spell.type, spell.icon);
        }
        else
        {
            Debug.LogWarning("No se encontró PlayerSpellUI para este jugador");
        }
    }

    private int GetSlotIndexForType(SpellType type)
    {
        return type switch
        {
            SpellType.Projectile => 0,
            SpellType.Area => 1,
            SpellType.Self => 2,
            SpellType.Summon => 3,
            _ => -1
        };
    }

    private void TryCast(int slotIndex)
    {
        if (isCastingChannel)
            return; // No puedes castear otro hechizo mientras canalizas
        var spell = slots[slotIndex];
        if (spell == null) return;
        //print($"[{name}] Intentando lanzar hechizo del slot {slotIndex + 1}");
        if (slotCooldowns[slotIndex] > 0f) return;

        if (!controller.HasMana(spell.manaCost)) return;

        // consumir maná
        controller.UseMana(spell.manaCost);

        // dirección de casteo: usa dónde mira el player (tu PlayerManager rota el sprite)
        Vector2 dir = castPoint.right;

        //print($"[{name}] Lanzando {spell.displayName} del slot {slotIndex + 1} hacia {dir}");
        // Ejecutar según tipo
        switch (spell.type)
        {
            case SpellType.Projectile:
                print("Pew pew!");
                //playerAim.HandleShooting();
                CastProjectile(spell, dir);
                break;
            case SpellType.Area:
                print("Boom!");
                CastArea(spell);
                break;
            case SpellType.Self:
                print("Ahhh!");
                CastSelf(spell);
                break;
            case SpellType.Summon:
                print("Here, minion!");
                CastSummon(spell);
                break;
        }

        // aplica cooldown
        slotCooldowns[slotIndex] = spell.cooldown;
    }

    private void CastProjectile(SpellData spell, Vector2 dir)
    {
        if (spell.prefab == null) return;
        var projectile = Instantiate(spell.prefab, castPoint.position, Quaternion.identity);

        // **IMPORTANTE: inicializar runtime**
        var projRuntime = projectile.GetComponent<ProjectileRuntime>();
        if (projRuntime != null)
            projRuntime.Init(this, spell);

        if (projectile.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = dir.normalized * spell.projectileSpeed;
            //rb.AddForce(shootPoint.right * bulletForce, ForceMode2D.Impulse);
        }
        // Componente opcional de daño en el proyectil (ver más abajo)
        var hit = projectile.GetComponent<SpellHit>();
        if (hit != null) hit.Init(this, spell);
    }

    private void CastArea(SpellData spell)
    {
        if (spell.prefab == null) return;

        Vector3 spawnPos;
        Quaternion rot;

        switch (spell.areaShape)
        {
            case AreaShape.Circle:
                spawnPos = transform.position;
                rot = Quaternion.identity;
                break;
            case AreaShape.Cone:
                spawnPos = castPoint.position;
                rot = castPoint.rotation;
                break;
            default:
                spawnPos = castPoint.position;
                rot = Quaternion.identity;
                break;
        }

        var aoeGO = Instantiate(spell.prefab, spawnPos, rot);

        if (spell.areaShape == AreaShape.Cone)
            aoeGO.transform.SetParent(castPoint);

        var aoeRuntime = aoeGO.GetComponent<AreaSpellRuntime>();
        if (aoeRuntime != null)
            aoeRuntime.Init(this, spell);

        if (spell.isChanneled)
        {
            isCastingChannel = true;
            activeChannelGO = aoeGO;
            activeChannelSpell = spell;

            // Cuando termine la duración, liberamos
            StartCoroutine(EndChannelAfterDuration(spell.duration));
        }
        else
        {
            Destroy(aoeGO, spell.duration);
        }
    }

    private IEnumerator EndChannelAfterDuration(float time)
    {
        yield return new WaitForSeconds(time);

        if (activeChannelGO != null)
            Destroy(activeChannelGO);

        isCastingChannel = false;
        activeChannelGO = null;
        activeChannelSpell = null;
    }


    private void CastSelf(SpellData spell)
    {
        if (spell == null) return;

        // Coste en vida
        if (spell.healthCost > 0)
            controller.TakeDamage(spell.healthCost);

        // Determinar objetivos
        PlayerController[] targets;
        if (spell.affectAllPlayers)
            targets = FindObjectsOfType<PlayerController>();
        else
            targets = new PlayerController[] { controller };

        foreach (var player in targets)
        {
            // Curación
            if (spell.healAmount > 0)
                player.Heal(spell.healAmount);

            // Restaurar mana
            if (spell.manaRestoreAmount > 0)
                player.RestoreMana(spell.manaRestoreAmount);

            // Aura visual
            if (spell.auraPrefab != null)
            {
                var vfx = Instantiate(spell.auraPrefab, player.transform.position, Quaternion.identity, player.transform);
                Destroy(vfx, spell.auraDuration > 0 ? spell.auraDuration : 0.5f);
            }
        }
    }


    private void CastSummon(SpellData spell)
    {
        if (spell == null || spell.summonPrefab == null) return;

        for (int i = 0; i < spell.summonCount; i++)
        {
            var summon = Instantiate(
                spell.summonPrefab,
                castPoint.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f),
                Quaternion.identity
            );

            Destroy(summon, spell.summonDuration);
        }
        // Si la invocación necesita saber quién la invocó, pásale referencia
        //var summ = GetComponentInChildren<SummonRuntime>();
        // o un script en el prefab de la invocación que acepte “owner”
    }
}
