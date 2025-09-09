using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStarManager : MonoBehaviour
{
    public static AStarManager instance;
    public NodeManager nodeManager;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // === Costes y heurística ===
    float StepCost(Node a, Node b)
    {
        bool diagonal = (a.cell.x != b.cell.x) && (a.cell.y != b.cell.y);
        return diagonal ? 1.41421356f : 1f;
    }

    float Heuristic(Node a, Node b)
    {
        int dx = Mathf.Abs(a.cell.x - b.cell.x);
        int dy = Mathf.Abs(a.cell.y - b.cell.y);
        // Distancia octil (perfecta para grid con diagonales)
        return Mathf.Max(dx, dy) + (1.41421356f - 1f) * Mathf.Min(dx, dy);
    }

    // === Generar ruta ===
    public List<Node> GeneratePath(Node start, Node goal, float agentRadiusInTiles)
    {
        if (start == null || goal == null)
        {
            Debug.LogWarning("Start o Goal son null, no se puede generar ruta");
            return null;
        }

        // Reset scores
        foreach (var n in nodeManager.AllNodes())
        {
            n.gScore = float.PositiveInfinity;
            n.hScore = 0;
            n.cameFrom = null;
        }

        var open = new List<Node> { start };
        var closed = new HashSet<Node>();

        start.gScore = 0;
        start.hScore = Heuristic(start, goal);

        while (open.Count > 0)
        {
            // Nodo con menor fScore
            Node current = open.OrderBy(n => n.FScore).First();

            if (current == goal)
                return ReconstructPath(current);

            open.Remove(current);
            closed.Add(current);

            int requiredClearance = Mathf.CeilToInt(agentRadiusInTiles);

            foreach (var neighbor in current.connections)
            {
                if (neighbor.clearance < requiredClearance) continue; // demasiado estrecho, no cabes
                if (closed.Contains(neighbor)) continue;

                float tentativeG = current.gScore + StepCost(current, neighbor);

                if (!open.Contains(neighbor))
                    open.Add(neighbor);
                else if (tentativeG >= neighbor.gScore)
                    continue;

                neighbor.cameFrom = current;
                neighbor.gScore = tentativeG;
                neighbor.hScore = Heuristic(neighbor, goal);
            }
        }

        // No se encontró ruta
        return null;
    }

    List<Node> ReconstructPath(Node current)
    {
        var totalPath = new List<Node>();
        while (current != null)
        {
            totalPath.Insert(0, current);
            current = current.cameFrom;
        }
        return totalPath;
    }
}
