using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [HideInInspector] public Vector3Int cell;       // celda en el grid
    public List<Node> connections = new List<Node>(); // vecinos

    // A* (se resetea por búsqueda)
    [HideInInspector] public Node cameFrom;
    [HideInInspector] public float gScore;
    [HideInInspector] public float hScore;
    public float FScore => gScore + hScore;

    public int clearance = 1; // número de tiles libres que caben alrededor (min dist a pared en tiles)

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        foreach (var n in connections)
            if (n) Gizmos.DrawLine(transform.position, n.transform.position);
    }
}
