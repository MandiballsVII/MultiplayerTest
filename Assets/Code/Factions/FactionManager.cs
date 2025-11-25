using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    public static FactionManager Instance;

    private Dictionary<Faction, List<Collider2D>> members = new Dictionary<Faction, List<Collider2D>>();

    void Awake()
    {
        Instance = this;

        // Inicializa para todas tus facciones
        foreach (Faction f in System.Enum.GetValues(typeof(Faction)))
            members[f] = new List<Collider2D>();
    }

    public void Register(Faction faction, Collider2D col)
    {
        if (!members[faction].Contains(col))
            members[faction].Add(col);
    }

    public void Unregister(Faction faction, Collider2D col)
    {
        members[faction].Remove(col);
    }

    public List<Collider2D> GetColliders(Faction faction)
    {
        return members[faction];
    }
}
