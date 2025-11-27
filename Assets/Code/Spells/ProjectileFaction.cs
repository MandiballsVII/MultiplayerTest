using UnityEngine;

public class ProjectileFaction : MonoBehaviour, IFactionMember
{
    public Faction Faction { get; private set; }
    public Transform Owner { get; private set; }

    public void Init(Faction faction, Transform owner)
    {
        Faction = faction;
        Owner = owner;

        var myCol = GetComponent<Collider2D>();
        var allies = FactionManager.Instance.GetColliders(faction);

        foreach (var ally in allies)
            Physics2D.IgnoreCollision(myCol, ally, true);
    }

}
