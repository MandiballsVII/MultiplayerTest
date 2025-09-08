using System.Collections.Generic;
using UnityEngine;

public abstract class NPC_ControllerBase : MonoBehaviour
{
    [Header("Movimiento")]
    public Node currentNode;
    protected List<Node> path = new List<Node>();
    public float speed = 3f;

    [Header("IA")]
    public float detectionRadius = 5f;
    [Tooltip("Histeresis para detección: evita toggles rápidos cuando el objetivo está justo en el borde")]
    public float detectionHysteresis = 0.35f;
    [Tooltip("Si la visión se corta por una pared, esperar este tiempo (s) antes de considerar perdida la visión")]
    public float occlusionLossDelay = 0.12f;

    public NodeManager nodeManager;
    public float attackRange = 1f; // rango de ataque genérico

    public enum StateMachine { Patrol, Engage, Evade, Search, Idle }
    public StateMachine currentState;

    [Header("Facción")]
    public Faction faction = Faction.Enemy; // ya común para todos

    [Header("Line of Sight")]
    public LayerMask obstacleMask; // asignar la capa de 'Walls' en inspector

    // Vision/internal state
    protected Transform target;               // target VISIBLE ahora (null si no está visible)
    protected Transform lastSeenTarget;       // referencia del transform que se vio por última vez
    protected Vector3? lastKnownPosition;     // posición donde se le vio por última vez
    float timeLOSLostStart = -999f;
    bool isLOSLost = false;

    protected Animator animator;

    protected virtual void Start()
    {
        if (nodeManager == null)
            nodeManager = FindObjectOfType<NodeManager>();
        if (currentNode == null && nodeManager != null)
            currentNode = GetClosestNode(transform.position);

        animator = GetComponent<Animator>();

        // si no se ha configurado obstacleMask en inspector, intentar asignar la layer "Walls"
        if (obstacleMask.value == 0)
        {
            int wLayer = LayerMask.NameToLayer("Walls");
            if (wLayer != -1)
                obstacleMask = LayerMask.GetMask("Walls");
            else
                Debug.LogWarning($"{name}: obstacleMask no asignado y no existe la layer 'Walls'. Asigna obstacleMask en el inspector.");
        }
    }

    // IMPORTANT: actualizamos la información de objetivo antes de UpdateState()
    protected virtual void Update()
    {
        UpdateTargetInfo();
        UpdateState();
        if(currentState != StateMachine.Idle)
            MoveAlongPath();
    }

    protected abstract void UpdateState();

    // -----------------------
    // VISIÓN con histeresis
    // -----------------------
    protected void UpdateTargetInfo()
    {
        float searchRadius = detectionRadius + detectionHysteresis;
        Transform candidate = FindClosestInLayerWithinRadius(faction, searchRadius);

        if (candidate != null)
        {
            bool hasLOSNow = HasLineOfSight(candidate);

            if (hasLOSNow)
            {
                // Restablecer todo
                target = candidate;
                lastSeenTarget = candidate;
                lastKnownPosition = candidate.position;
                isLOSLost = false;
                timeLOSLostStart = -999f;
            }
            else if (lastSeenTarget == candidate)
            {
                // Iniciamos temporizador para pérdida por obstrucción
                if (!isLOSLost)
                {
                    isLOSLost = true;
                    timeLOSLostStart = Time.time;
                    lastKnownPosition = candidate.position; // <--- guardamos la última posición vista
                }
                else if (Time.time - timeLOSLostStart >= occlusionLossDelay)
                {
                    target = null; // ahora realmente perdemos al target
                                   // lastKnownPosition ya tiene la última posición conocida
                }
            }
        }
        else
        {
            target = null;
            lastSeenTarget = null;
            // No reseteamos lastKnownPosition aquí para que SearchLastKnown() pueda usarla
            if (!isLOSLost)
                lastKnownPosition = null;
        }
    }


    // Busca el transform más cercano en la layer objetivo dentro de 'radius'
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

    // Raycast que devuelve true si NO hay obstáculo entre este NPC y el target.
    protected bool HasLineOfSight(Transform t)
    {
        Vector2 dir = (t.position - transform.position);
        float dist = dir.magnitude;
        if (dist <= 0.001f) return true;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, dist, obstacleMask);

        // Debug: dibuja la línea en la escena (verde si no impacta, rojo si impacta)
        Color rayColor = (hit.collider == null) ? Color.green : Color.red;
        Debug.DrawRay(transform.position, dir.normalized * dist, rayColor);

        // hit.collider == null => no obstáculo en obstacleMask entre ambos
        return hit.collider == null;
    }

    // -----------------------
    // MOVIMIENTO base
    // -----------------------
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
    }

    protected Node GetClosestNode(Vector3 world) => nodeManager.GetClosestNode(world);

    protected void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        if (direction.sqrMagnitude < 0.001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
    }

    // Opcional: gizmos para ver detection/attack
    protected virtual void OnDrawGizmosSelected()
    {
        // detectar solo en play? permitimos verlo siempre
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // dibujar lastKnownPosition
        if (lastKnownPosition.HasValue)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lastKnownPosition.Value, 0.08f);
        }

        // dibujar target line (si target visible)
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
