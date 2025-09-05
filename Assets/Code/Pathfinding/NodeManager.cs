using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NodeManager : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap tilemapSuelo;
    public Tilemap tilemapParedes;

    [Header("Node")]
    public Node nodePrefab;

    [Header("Conexiones")]
    public bool allowDiagonals = true;
    public bool allowCornerCutting = false; // si false: bloquea diagonales si “chocan” con esquinas

    // Acceso O(1) por celda y lista solo si la necesitas para iterar
    public readonly Dictionary<Vector3Int, Node> nodeDict = new();
    public readonly List<Node> nodeList = new();

    static readonly Vector3Int[] ORTHO = { new(1, 0, 0), new(-1, 0, 0), new(0, 1, 0), new(0, -1, 0) };
    static readonly Vector3Int[] DIAG = { new(1, 1, 0), new(-1, 1, 0), new(1, -1, 0), new(-1, -1, 0) };

    void Awake()
    {
        BuildNodes();
        BuildConnections();
    }

    void BuildNodes()
    {
        nodeDict.Clear();
        nodeList.Clear();

        foreach (var cell in tilemapSuelo.cellBounds.allPositionsWithin)
        {
            // Solo suelo, nunca pared
            if (tilemapSuelo.GetTile(cell) == null) continue;
            if (tilemapParedes && tilemapParedes.GetTile(cell) != null) continue;

            Vector3 world = tilemapSuelo.GetCellCenterWorld(cell);
            var n = Instantiate(nodePrefab, world, Quaternion.identity, transform);
            n.cell = cell;

            nodeDict[cell] = n;
            nodeList.Add(n);
        }
    }

    void BuildConnections()
    {
        foreach (var kv in nodeDict)
        {
            var cell = kv.Key;
            var a = kv.Value;

            // 4 vecinos ortogonales
            foreach (var d in ORTHO)
                TryConnect(a, cell + d);

            // Diagonales (opcional)
            if (allowDiagonals)
            {
                foreach (var d in DIAG)
                {
                    if (!allowCornerCutting)
                    {
                        // Bloquear si alguna ortogonal adyacente no existe (evita cortar esquinas)
                        var o1 = new Vector3Int(d.x, 0, 0);
                        var o2 = new Vector3Int(0, d.y, 0);
                        if (!nodeDict.ContainsKey(cell + o1) || !nodeDict.ContainsKey(cell + o2))
                            continue;
                    }
                    TryConnect(a, cell + d);
                }
            }
        }
    }

    void TryConnect(Node a, Vector3Int targetCell)
    {
        if (!nodeDict.TryGetValue(targetCell, out var b)) return;
        if (a == b) return;

        // Evitar duplicados
        if (!a.connections.Contains(b)) a.connections.Add(b);
        if (!b.connections.Contains(a)) b.connections.Add(a);
    }

    // === API útil para la IA ===

    // Todos los nodos
    public IEnumerable<Node> AllNodes() => nodeList;

    // Nodo exactamente bajo una posición del mundo (O(1))
    public Node GetNodeAtWorld(Vector3 world)
    {
        var cell = tilemapSuelo.WorldToCell(world);
        nodeDict.TryGetValue(cell, out var n);
        return n;
    }

    // Nodo más cercano en caso de que no estés justo en una celda
    public Node GetClosestNode(Vector3 world, int maxRadius = 12)
    {
        var cell = tilemapSuelo.WorldToCell(world);
        if (nodeDict.TryGetValue(cell, out var exact)) return exact;

        float bestDist = Mathf.Infinity;
        Node closest = null;

        foreach (var n in nodeList)
        {
            float dist = Vector2.SqrMagnitude(world - n.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = n;
            }
        }
        return closest;
    }

    // Nodo más lejano a una posición
    public Node GetFurthestNode(Vector3 worldPosition)
    {
        Node furthest = null;
        float bestDist = 0f;

        foreach (var node in nodeList)
        {
            float dist = Vector2.SqrMagnitude(worldPosition - node.transform.position);
            if (dist > bestDist)
            {
                bestDist = dist;
                furthest = node;
            }
        }
        return furthest;
    }

    // Nodo aleatorio
    public Node GetRandomNode()
    {
        if (nodeList.Count == 0) return null;
        return nodeList[Random.Range(0, nodeList.Count)];
    }

}
