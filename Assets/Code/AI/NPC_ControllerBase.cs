using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

[RequireComponent(typeof(Health))]
public abstract class NPC_ControllerBase : MonoBehaviour
{
    [Header("Movimiento")]
    public Node currentNode;
    protected List<Node> path = new List<Node>();
    public float speed = 3f;

    [Header("IA")]
    public float detectionRadius = 5f;
    public float detectionHysteresis = 0.35f;
    public float occlusionLossDelay = 0.12f;

    public NodeManager nodeManager;
    public float attackRange = 1f;

    public enum StateMachine { Patrol, Engage, Evade, Search, Idle }
    public StateMachine currentState;

    [Header("Facción")]
    public Faction faction = Faction.Enemy;

    [Header("Line of Sight")]
    public LayerMask obstacleMask;

    // --- Datos internos ---
    protected Transform target;
    protected Transform lastSeenTarget;
    protected Vector3? lastKnownPosition;
    float timeLOSLostStart = -999f;
    bool isLOSLost = false;

    protected Animator animator;
    public Health health { get; private set; }

    public float radiusInTiles = 1f;

    // Invocador (si aplica)
    public Transform Owner { get; private set; }

    protected virtual void Awake()
    {
        health = GetComponent<Health>();
        health.OnDied += Die;
    }

    protected virtual void Start()
    {
        if (nodeManager == null)
            nodeManager = FindObjectOfType<NodeManager>();
        if (currentNode == null && nodeManager != null)
            currentNode = GetClosestNode(transform.position);

        animator = GetComponent<Animator>();

        if (obstacleMask.value == 0)
        {
            int wLayer = LayerMask.NameToLayer("Walls");
            if (wLayer != -1)
                obstacleMask = LayerMask.GetMask("Walls");
            else
                Debug.LogWarning($"{name}: obstacleMask no asignado y no existe la layer 'Walls'.");
        }
    }

    protected virtual void Update()
    {
        UpdateTargetInfo();
        UpdateState();
        if (currentState != StateMachine.Idle)
            MoveAlongPath();
    }

    public void Init(Transform owner, Faction faction)
    {
        Owner = owner;
        this.faction = faction;
        int ownerLayer = owner.gameObject.layer;
        SetLayerRecursively(gameObject, ownerLayer);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, newLayer);
    }

    protected abstract void UpdateState();

    // ================== VIDA ==================
    protected virtual void Die()
    {
        Debug.Log($"{name} ha muerto.");
        Destroy(gameObject);
    }

    // ================== VISIÓN ==================
    protected void UpdateTargetInfo()
    {
        float searchRadius = detectionRadius + detectionHysteresis;
        Transform candidate = FindClosestInLayerWithinRadius(faction, searchRadius);

        if (candidate != null)
        {
            bool hasLOSNow = HasLineOfSight(candidate);

            if (hasLOSNow)
            {
                target = candidate;
                lastSeenTarget = candidate;
                lastKnownPosition = candidate.position;
                isLOSLost = false;
                timeLOSLostStart = -999f;
            }
            else if (lastSeenTarget == candidate)
            {
                if (!isLOSLost)
                {
                    isLOSLost = true;
                    timeLOSLostStart = Time.time;
                    lastKnownPosition = candidate.position;
                }
                else if (Time.time - timeLOSLostStart >= occlusionLossDelay)
                {
                    target = null;
                }
            }
        }
        else
        {
            target = null;
            lastSeenTarget = null;
            if (!isLOSLost)
                lastKnownPosition = null;
        }
    }

    Transform FindClosestInLayerWithinRadius(Faction myFaction, float radius)
    {
        int targetLayer = (myFaction == Faction.Enemy)
            ? LayerMask.NameToLayer("Player")
            : LayerMask.NameToLayer("Enemy");

        if (targetLayer == -1) return null;

        Transform closest = null;
        float best = Mathf.Infinity;

        foreach (var t in GameObject.FindObjectsOfType<Transform>())
        {
            if (t.gameObject.layer != targetLayer) continue;
            float d = Vector2.Distance(transform.position, t.position);
            if (d <= radius && d < best)
            {
                best = d;
                closest = t;
            }
        }
        return closest;
    }

    protected bool HasLineOfSight(Transform t)
    {
        Vector2 dir = (t.position - transform.position);
        float dist = dir.magnitude;
        if (dist <= 0.001f) return true;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, dist, obstacleMask);
        Color rayColor = (hit.collider == null) ? Color.green : Color.red;
        Debug.DrawRay(transform.position, dir.normalized * dist, rayColor);

        return hit.collider == null;
    }

    // ================== MOVIMIENTO ==================
    protected void MoveAlongPath()
    {

        if (path.Count == 0) return;

        Node targetNode = path[0];
        RotateTowards(targetNode.transform.position);
        transform.position = Vector3.MoveTowards(transform.position, targetNode.transform.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetNode.transform.position) < 0.1f)
        {
            currentNode = targetNode;
            path.RemoveAt(0);
        }
        float step = speed * Time.deltaTime;
        float dist = Vector2.Distance(transform.position, targetNode.transform.position);
        //Debug.Log($"Dist: {dist}, Step: {step}");
        //transform.position = Vector3.MoveTowards(transform.position, targetNode.transform.position, step);
        //Debug.Log($"NPC: {transform.position}, NextNode: {targetNode.transform.position}");
        //foreach (var n in path)
        //    Debug.Log($"Path Node: {n.transform.position}");
    }

    protected Node GetClosestNode(Vector3 world) => nodeManager.GetClosestNode(world);

    protected void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        if (direction.sqrMagnitude < 0.001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if(gameObject.name.Contains("Skeleton"))
            transform.rotation = Quaternion.Euler(0, 0, angle);
        else
            transform.rotation = Quaternion.Euler(0, 0, angle + 90);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (lastKnownPosition.HasValue)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lastKnownPosition.Value, 0.08f);
        }

        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
    protected virtual void OnDrawGizmos()
    {
        if (path == null || path.Count == 0)
            return;

        // Dibujar líneas entre nodos
        Gizmos.color = Color.cyan;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i].transform.position, path[i + 1].transform.position);
        }

        // Nodos intermedios (azul)
        Gizmos.color = Color.blue;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawSphere(path[i].transform.position, 0.15f);
        }

        // Nodo final (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(path[path.Count - 1].transform.position, 0.25f);

        // Nodo actual (verde)
        if (currentNode != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentNode.transform.position, 0.2f);
        }
    }

}
