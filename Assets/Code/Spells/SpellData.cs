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

    [Header("Prefabs / par�metros")]
    public GameObject prefab;          // Proyectil / �rea / invocaci�n (si aplica)
    public float projectileSpeed = 12f; // para Projectile
    public float areaRadius = 2.5f;     // para Area
    public float duration = 3f;         // p.ej. AoE vivo / buff duraci�n
    public float power = 20f;           // da�o/curaci�n gen�rica seg�n tipo

    [Header("Curaci�n")]
    public float healAmount = 20f;
    public float manaRestoreAmount; // cuanto mana devuelve
    public float healthCost;       // vida que pierde el lanzador
    public bool affectAllPlayers = false;
    public float auraDuration = 1f; // duraci�n del aura
    public GameObject auraPrefab; // el aura verde opcional

    [Header("�rea")]
    public AreaShape areaShape;
    public float coneAngle = 45f; // solo para conos

    [Header("Canalizaci�n")]
    [Tooltip("true para conos, rayos o cualquier hechizo de duraci�n prolongada")]
    public bool isChanneled; // true para conos, rayos, etc.

    [Header("Explosi�n")]
    [Tooltip("Si el proyectil explota al impactar")]
    public GameObject explosionPrefab; // opcional, para proyectiles que explotan
    public float explosionRadius;
    public float explosionDamage;


}
