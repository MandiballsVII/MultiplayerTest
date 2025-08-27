using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class BeholderController : MonoBehaviour
{
    [Header("Detection / Movement")]
    public float detectionRadius = 10f;
    public float optimalDistance = 5f;
    public float stoppingTolerance = 0.5f;
    public float moveSpeed = 3f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [SerializeField] private float moveThreshold = 0.01f;
    [SerializeField] private float stopDelay = 0.1f;
    private float stopTimer = 0f;

    [Header("Attack")]
    public Transform firePoint;
    public GameObject rayPrefab;
    public float rayCooldown = 2f;
    public float raySpeed = 12f;

    private Animator anim;
    private Rigidbody2D rb;
    private Transform target;
    private float lastShotTime = -999f;
    private Vector2 lastPosition;

    // Pathfinding opcional
    private List<Vector3> currentPath;
    private int pathIndex = 0;
    private Vector3 lastKnownPlayerPos;
    private bool hasLOS = false; // line of sight

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        anim = GetComponent<Animator>();

        if (firePoint == null)
            firePoint = transform; // fallback
    }

    void Start()
    {
        lastPosition = rb.position;
    }

    void Update()
    {
        DetectNearestPlayer();
        UpdateLineOfSight();

        if (hasLOS && target != null)
            lastKnownPlayerPos = target.position;

        if (target != null && hasLOS)
            TryShootAtTarget();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
        HandleAnimation();
    }

    void DetectNearestPlayer()
    {
        if (playerLayer == 0)
            playerLayer = LayerMask.GetMask("Player");

        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, detectionRadius, playerLayer);
        if (cols.Length == 0) { target = null; return; }

        float bestDist = float.MaxValue;
        Transform best = null;
        foreach (var c in cols)
        {
            float d = Vector2.Distance(transform.position, c.transform.position);
            if (d < bestDist) { bestDist = d; best = c.transform; }
        }
        target = best;
    }

    void UpdateLineOfSight()
    {
        if (target == null) { hasLOS = false; return; }

        Vector2 dir = (target.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, detectionRadius, obstacleLayer | playerLayer);
        hasLOS = hit && hit.collider != null && hit.collider.transform == target;
    }

    void HandleMovement()
    {
        if (target == null && lastKnownPlayerPos == Vector3.zero)
            return;

        Vector3 destination = (hasLOS && target != null) ? target.position : lastKnownPlayerPos;

        Vector2 toTarget = (Vector2)destination - rb.position;
        float dist = toTarget.magnitude;

        Vector2 desiredMove = Vector2.zero;

        if (dist > optimalDistance + stoppingTolerance)
            desiredMove = toTarget.normalized * moveSpeed * Time.fixedDeltaTime;
        else if (dist < optimalDistance - stoppingTolerance)
            desiredMove = -toTarget.normalized * moveSpeed * Time.fixedDeltaTime;

        if (desiredMove != Vector2.zero)
        {
            // Separar el movimiento en X y Y
            Vector2 moveX = new Vector2(desiredMove.x, 0f);
            Vector2 moveY = new Vector2(0f, desiredMove.y);

            // Primero eje X
            if (moveX != Vector2.zero)
            {
                RaycastHit2D hitX = Physics2D.BoxCast(
                    rb.position,
                    rb.GetComponent<Collider2D>().bounds.size,
                    0f,
                    moveX.normalized,
                    Mathf.Abs(moveX.x),
                    obstacleLayer
                );
                if (!hitX)
                    rb.MovePosition(rb.position + moveX);
            }

            // Luego eje Y
            if (moveY != Vector2.zero)
            {
                RaycastHit2D hitY = Physics2D.BoxCast(
                    rb.position,
                    rb.GetComponent<Collider2D>().bounds.size,
                    0f,
                    moveY.normalized,
                    Mathf.Abs(moveY.y),
                    obstacleLayer
                );
                if (!hitY)
                    rb.MovePosition(rb.position + moveY);
            }
        }
    }


    void HandleRotation()
    {
        if (target == null && !hasLOS) return;

        Vector2 lookTarget = hasLOS ? (Vector2)target.position : (Vector2)lastKnownPlayerPos;
        Vector2 toTarget = lookTarget - rb.position;
        float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg + 90f;
        rb.MoveRotation(angle);
    }

    void HandleAnimation()
    {
        Vector2 moveDelta = rb.position - lastPosition;
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

        lastPosition = rb.position;
    }

    void TryShootAtTarget()
    {
        if (rayPrefab == null || firePoint == null || !hasLOS) return;
        if (Time.time < lastShotTime + rayCooldown) return;

        lastShotTime = Time.time;

        Vector2 dir = ((Vector2)target.position - (Vector2)firePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        GameObject go = Instantiate(rayPrefab, firePoint.position, rotation);
        var ray = go.GetComponent<BeholderRay>();
        if (ray != null) ray.Init(dir, raySpeed);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, optimalDistance);
    }
}
