using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SkeletonAI : SummonedBase
{
    [Header("AI Settings")]
    public float moveSpeed = 3f;
    public float detectionRadius = 6f;
    public float attackRange = 1f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;

    public LayerMask enemyLayer;

    private Rigidbody2D rb;
    private Transform currentTarget;
    private float lastAttackTime;

    [Header("Anti-Stacking")]
    public float separationRadius = 1f;
    public float separationForce = 2f;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        print("Current heath: " + currentHealth);
        if (!IsAlive || Owner == null) return;

        // Buscar enemigo más cercano
        FindNearestEnemy();

        Vector2 targetPos;

        if (currentTarget != null)
        {
            targetPos = currentTarget.position;

            // Atacar si está en rango
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack(currentTarget);
            }
        }
        else
        {
            // Volver junto al mago
            targetPos = Owner.position;
        }

        // Movimiento hacia target
        MoveTowards(targetPos);

        // Aplicar separación con otros esqueletos
        ApplySeparation();
    }

    void FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        if (hits.Length == 0) { currentTarget = null; return; }

        float bestDist = Mathf.Infinity;
        Transform best = null;
        foreach (var h in hits)
        {
            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = h.transform;
            }
        }
        currentTarget = best;
    }

    void MoveTowards(Vector2 targetPos)
    {
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
    }

    void ApplySeparation()
    {
        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, separationRadius);
        foreach (var ally in allies)
        {
            if (ally.gameObject == this.gameObject) continue;
            if (ally.GetComponent<SkeletonAI>() == null) continue;

            Vector2 away = (transform.position - ally.transform.position).normalized;
            rb.MovePosition(rb.position + away * separationForce * Time.fixedDeltaTime);
        }
    }

    void Attack(Transform enemy)
    {
        lastAttackTime = Time.time;
        var target = enemy.GetComponent<IAttackable>();
        if (target != null)
        {
            target.TakeDamage(attackDamage);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
