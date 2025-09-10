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

    float StepCost(Node a, Node b)
    {
        bool diagonal = (a.cell.x != b.cell.x) && (a.cell.y != b.cell.y);
        return diagonal ? 1.41421356f : 1f;
    }

    float Heuristic(Node a, Node b)
    {
        int dx = Mathf.Abs(a.cell.x - b.cell.x);
        int dy = Mathf.Abs(a.cell.y - b.cell.y);
        return Mathf.Max(dx, dy) + (1.41421356f - 1f) * Mathf.Min(dx, dy);
    }

    // === Generar ruta ===
    public List<Node> GeneratePath(Node start, Node goal, float agentRadiusInTiles)
    {
        if (start == null || goal == null)
        {
            Debug.LogWarning(" Start o Goal son null, devolviendo lista vacía");
            return new List<Node>();
        }

        // regla de mapeo: si agentRadiusInTiles == 1 -> required = 0 (solo centro)
        int requiredClearance = Mathf.Max(0, Mathf.CeilToInt(agentRadiusInTiles) - 1);

        // intentar reemplazar start/goal por nodos cercanos que cumplan clearance si es necesario
        if (start.clearance < requiredClearance)
        {
            var startWorld = nodeManager.tilemapSuelo.GetCellCenterWorld(start.cell);
            var altStart = nodeManager.GetClosestNodeWithClearance(startWorld, requiredClearance, 8);
            if (altStart != null)
            {
                Debug.Log($"{nameOf(this)}: start no tenía clearance ({start.clearance}) -> usando nodo alternativo con clearance {altStart.clearance}");
                start = altStart;
            }
            else
            {
                Debug.LogWarning($"AStar: start no cumple clearance ({start.clearance} < {requiredClearance}) y no hay alternativa. Ruta imposible.");
                return new List<Node>();
            }
        }

        if (goal.clearance < requiredClearance)
        {
            var goalWorld = nodeManager.tilemapSuelo.GetCellCenterWorld(goal.cell);
            var altGoal = nodeManager.GetClosestNodeWithClearance(goalWorld, requiredClearance, 8);
            if (altGoal != null)
            {
                Debug.Log($"{nameOf(this)}: goal no tenía clearance ({goal.clearance}) -> usando nodo alternativo con clearance {altGoal.clearance}");
                goal = altGoal;
            }
            else
            {
                Debug.LogWarning($"AStar: goal no cumple clearance ({goal.clearance} < {requiredClearance}) y no hay alternativa. Ruta imposible.");
                return new List<Node>();
            }
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
            Node current = open.OrderBy(n => n.FScore).First();

            if (current == goal)
                return ReconstructPath(current);

            open.Remove(current);
            closed.Add(current);

            foreach (var neighbor in current.connections)
            {
                // filtro de clearance
                if (neighbor.clearance < requiredClearance) continue;
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

        Debug.LogWarning(" No se encontró ruta. Devolviendo lista vacía.");
        return new List<Node>();
    }

    // Ayudita para debug (nombre de la instancia)
    string nameOf(MonoBehaviour m) => m == null ? "AStar" : m.name;


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
