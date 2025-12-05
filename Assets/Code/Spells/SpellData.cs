using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spells/SpellData")]
public class SpellData : ScriptableObject
{
    [Header("Meta")]
    public string id;
    public string displayName;
    public Sprite icon;
    public SpellSchool school;
    public SpellType type;

    [Header("Costes y tiempos")]
    public float manaCost = 10f;
    public float cooldown = 1f;

    [Header("Prefabs / parámetros")]
    public GameObject prefab;          // Proyectil / área / invocación (si aplica)
    public float projectileSpeed = 12f; // para Projectile
    public float areaRadius = 2.5f;     // para Area
    public float duration = 3f;         // p.ej. AoE vivo / buff duración
    public float power = 20f;           // daño/curación genérica según tipo
    public bool disableKnockback = false;

    [Header("Curación")]
    public int healAmount = 20;
    public int manaRestoreAmount; // cuanto mana devuelve
    public int healthCost;       // vida que pierde el lanzador
    public bool affectAllPlayers = false;
    public float auraDuration = 1f; // duración del aura
    public GameObject auraPrefab; // el aura verde opcional

    [Header("Área")]
    public AreaShape areaShape;
    public float coneAngle = 45f; // solo para conos

    [Header("Daño periódico")]
    [Tooltip("Cada cuántos segundos aplica daño el área. Ej: 1 = una vez por segundo.")]
    public float damageInterval = 1f;

    [Header("Canalización")]
    [Tooltip("true para conos, rayos o cualquier hechizo de duración prolongada")]
    public bool isChanneled; // true para conos, rayos, etc.

    [Header("Explosión")]
    [Tooltip("Si el proyectil explota al impactar")]
    public GameObject explosionPrefab; // opcional, para proyectiles que explotan
    public float explosionRadius;
    public float explosionDamage;

    [Header("Summon Settings")]
    public GameObject summonPrefab;
    public int summonCount = 1; // Por si quieres invocar más de uno
    public float summonDuration = 10f; // Tiempo que vive el esqueleto

    [Header("Status Effects")]
    [Tooltip("Efectos que se aplican al impactar o al tick del área.")]
    public List<StatusEffectDefinition> statusEffects = new List<StatusEffectDefinition>();

}
