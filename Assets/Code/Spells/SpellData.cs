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
}
