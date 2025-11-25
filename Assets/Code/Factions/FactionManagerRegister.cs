using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FactionManagerRegister : MonoBehaviour
{
    IFactionMember fac;
    Collider2D col;

    void Awake()
    {
        fac = GetComponent<IFactionMember>();
        col = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        StartCoroutine(RegisterNextFrame());
    }
    IEnumerator RegisterNextFrame()
    {
        yield return null; // espera 1 frame
        FactionManager.Instance?.Register(fac.Faction, col);
    }
    void OnDisable()
    {
        FactionManager.Instance?.Unregister(fac.Faction, col);
    }
}
