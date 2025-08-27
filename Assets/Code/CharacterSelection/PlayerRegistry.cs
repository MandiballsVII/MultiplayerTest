using System.Collections.Generic;
using UnityEngine;

public class PlayerRegistry : MonoBehaviour
{
    public static PlayerRegistry Instance { get; private set; }

    private readonly List<GameObject> players = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterPlayer(GameObject player)
    {
        if (!players.Contains(player))
            players.Add(player);
    }

    public void UnregisterPlayer(GameObject player)
    {
        if (players.Contains(player))
            players.Remove(player);
    }

    public GameObject GetClosestPlayer(Vector3 position)
    {
        GameObject closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var p in players)
        {
            if (p == null) continue;
            float dist = Vector3.Distance(position, p.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = p;
            }
        }

        return closest;
    }
}
