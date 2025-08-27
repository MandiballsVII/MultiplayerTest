using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance;

    [Header("Grid Setup")]
    public Tilemap obstacleTilemap;   // Tilemap con la layer Obstacles
    public Vector2Int gridSize = new Vector2Int(50, 50); // Ajusta según tu escena
    public float cellSize = 1f;

    private Node[,] grid;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (obstacleTilemap == null)
            obstacleTilemap = GameObject.FindObjectOfType<Tilemap>(); // busca automáticamente
        if (obstacleTilemap == null)
        {
            Debug.LogError("PathfindingManager: no se encontró ningún Tilemap de obstáculos");
            enabled = false; // desactivar para no lanzar errores
            return;
        }

        CreateGrid();
    }

    private void CreateGrid()
    {
        grid = new Node[gridSize.x, gridSize.y];
        Vector3 origin = obstacleTilemap.transform.position -
                         new Vector3(gridSize.x, gridSize.y, 0) * cellSize * 0.5f;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPos = origin + new Vector3(x + 0.5f, y + 0.5f, 0) * cellSize;
                Vector3Int cellPos = obstacleTilemap.WorldToCell(worldPos);
                bool walkable = !obstacleTilemap.HasTile(cellPos);
                grid[x, y] = new Node(walkable, worldPos, x, y);
            }
        }
    }

    public List<Vector3> FindPath(Vector3 startWorld, Vector3 targetWorld)
    {
        Node startNode = GetNodeFromWorldPoint(startWorld);
        Node targetNode = GetNodeFromWorldPoint(targetWorld);
        if (startNode == null || targetNode == null) return null;

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    currentNode = openSet[i];
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newCost = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // no path found
    }

    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Vector3> path = new List<Vector3>();
        Node current = endNode;
        while (current != startNode)
        {
            path.Add(current.worldPosition);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }

    private Node GetNodeFromWorldPoint(Vector3 worldPos)
    {
        float percentX = (worldPos.x - obstacleTilemap.transform.position.x + gridSize.x * 0.5f * cellSize) / (gridSize.x * cellSize);
        float percentY = (worldPos.y - obstacleTilemap.transform.position.y + gridSize.y * 0.5f * cellSize) / (gridSize.y * cellSize);
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSize.x - 1) * percentX);
        int y = Mathf.RoundToInt((gridSize.y - 1) * percentY);

        if (x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y)
            return grid[x, y];
        return null;
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbors;
    }

    private int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);

        // diagonal movement cost raiz2 ~ 14, horizontal/vertical 10
        int straightCost = 10;
        int diagonalCost = 14;

        int diag = Mathf.Min(dstX, dstY);
        int straight = Mathf.Abs(dstX - dstY);
        return diagonalCost * diag + straightCost * straight;
    }

    public class Node
    {
        public bool walkable;
        public Vector3 worldPosition;
        public int gridX, gridY;
        public int gCost, hCost;
        public Node parent;

        public Node(bool walkable, Vector3 worldPos, int x, int y)
        {
            this.walkable = walkable;
            worldPosition = worldPos;
            gridX = x;
            gridY = y;
        }

        public int fCost => gCost + hCost;
    }
}
