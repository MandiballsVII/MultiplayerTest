using UnityEngine;

public class ProjectileFaction : MonoBehaviour, IFactionMember
{
    public Faction Faction { get; private set; }
    public Transform Owner { get; private set; }

    public void Init(Faction faction, Transform owner)
    {
        Faction = faction;
        Owner = owner;
    }
}
