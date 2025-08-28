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

    [SerializeField] private float moveThreshold = 0.01f; // mínimo delta
    [SerializeField] private float stopDelay = 0.1f;      // segundos que espera antes de poner Idle
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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic; // mantenemos Kinematic
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        lastPosition = rb.position;
    }

    void Update()
    {
        DetectNearestPlayer();

        if (target != null)
            TryShootAtTarget();
    }

    void FixedUpdate()
    {
        // --- Movimiento ---
        if (target != null)
        {
            MoveToOptimalDistance();
            RotateToFaceTarget();
        }

        // --- Comprobación de movimiento para animación ---
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

    void DetectNearestPlayer()
    {
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

    void MoveToOptimalDistance()
    {
        if (target == null) return;

        Vector2 toTarget = (Vector2)target.position - rb.position;
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
            RaycastHit2D hit = Physics2D.Raycast(rb.position, move.normalized, move.magnitude + 1f, LayerMask.GetMask("Walls"));

            if (hit.collider == null) // si no hay pared delante, movemos
            {
                rb.MovePosition(newPos);
            }
            else
            {
                // Bloqueado por pared -> no mover
                Debug.Log("[Beholder] Movimiento bloqueado por pared: " + hit.collider.name);
            }
        }
    }


    void RotateToFaceTarget()
    {
        if (target == null) return;

        Vector2 toTarget = (Vector2)target.position - rb.position;
        float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        angle += 90f; // el sprite mira hacia abajo
        rb.MoveRotation(angle);
    }

    void TryShootAtTarget()
    {
        if (rayPrefab == null || firePoint == null) return;
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
