using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class BeholderController : MonoBehaviour
{
    [Header("Detection / Movement")]
    public float detectionRadius = 10f;
    public float optimalDistance = 5f;
    public float stoppingTolerance = 0.5f;
    public float moveSpeed = 3f;
    public LayerMask playerLayer;

    [SerializeField] private float moveThreshold = 0.0001f; // mínimo delta
    [SerializeField] private float stopDelay = 0.1f;      // segundos que espera antes de poner Idle
    private float stopTimer = 0f;

    [Header("Attack")]
    public Transform firePoint;
    public GameObject rayPrefab;
    public float rayCooldown = 2f;
    public float raySpeed = 12f;
    public float shotDelayBetweenPlayers = 0.3f;
    LayerMask detectionLayers;

    private Animator anim;
    private Rigidbody2D rb;

    private List<Transform> detectedPlayers = new List<Transform>();
    private Transform closestTarget;
    private float lastShotTime = -999f;
    CircleCollider2D circleCollider2D;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic; // mantenemos Kinematic
        anim = GetComponent<Animator>();
        circleCollider2D = GetComponent<CircleCollider2D>();
        detectionLayers = LayerMask.GetMask("Player", "Summoned");
    }

    void Update()
    {
        DetectPlayers();

        if (detectedPlayers.Count > 0)
            TryShootAtPlayers();
    }

    void FixedUpdate()
    {
        // --- Movimiento ---
        if (closestTarget != null)
        {
            MoveToOptimalDistance();
            RotateToFaceTarget();
        }

        // --- Comprobación de movimiento para animación ---
        Vector2 moveDelta = rb.position - (Vector2)rb.position;
        bool currentlyMoving = moveDelta.sqrMagnitude > moveThreshold;

        if (currentlyMoving)
        {
            anim.SetBool("IsMoving", true);
            stopTimer = 0f;
        }
        else
        {
            stopTimer += Time.fixedDeltaTime;
            if (stopTimer >= stopDelay)
                anim.SetBool("IsMoving", false);
        }
    }

    void DetectPlayers()
    {
        detectedPlayers.Clear();
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayer);

        if (cols.Length == 0)
        {
            closestTarget = null;
            return;
        }

        float bestDist = float.MaxValue;
        Transform best = null;

        foreach (var c in cols)
        {
            detectedPlayers.Add(c.transform);
            float d = Vector2.Distance(transform.position, c.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = c.transform;
            }
        }

        closestTarget = best;
    }

    void MoveToOptimalDistance()
    {
        if (closestTarget == null) return;

        Vector2 toTarget = (Vector2)closestTarget.position - rb.position;
        float dist = toTarget.magnitude;
        Vector2 move = Vector2.zero;

        if (dist > optimalDistance + stoppingTolerance)
            move = toTarget.normalized * moveSpeed * Time.fixedDeltaTime;
        else if (dist < optimalDistance - stoppingTolerance)
            move = -toTarget.normalized * moveSpeed * Time.fixedDeltaTime;

        if (move != Vector2.zero)
        {
            // Verificar colisión con paredes
            Vector2 newPos = rb.position + move;
            RaycastHit2D hit = Physics2D.Raycast(rb.position, move.normalized, move.magnitude + circleCollider2D.radius, LayerMask.GetMask("Walls"));

            if (hit.collider == null) // si no hay pared delante, movemos
            {
                rb.MovePosition(newPos);
            }
            
        }
    }


    void RotateToFaceTarget()
    {
        if (closestTarget == null) return;

        Vector2 toTarget = (Vector2)closestTarget.position - rb.position;
        float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        angle += 90f; // el sprite mira hacia abajo
        rb.MoveRotation(angle);
    }

    void TryShootAtPlayers()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionLayers);
        if (rayPrefab == null || firePoint == null) return;
        if (Time.time < lastShotTime + rayCooldown) return;

        lastShotTime = Time.time;

        foreach (var entity in cols)
        {
            if (entity == null) continue;

            Vector2 dir = ((Vector2)entity.transform.position - (Vector2)firePoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            GameObject go = Instantiate(rayPrefab, firePoint.position, rotation);
            var ray = go.GetComponent<BeholderRay>();
            if (ray != null) ray.Init(dir, raySpeed);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, optimalDistance);
    }
}
