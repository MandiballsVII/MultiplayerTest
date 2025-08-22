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
}
