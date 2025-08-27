using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(Collider2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    public GameObject rayPrefab;
    public Tilemap obstacleTilemap;

    [Header("Detection / Movement")]
    public float detectionRadius = 10f;
    public float stoppingDistance = 2f;
    public float moveSpeed = 3f;
    public LayerMask obstacleLayer;

    [Header("Attack")]
    public float rayCooldown = 2f;
    public float raySpeed = 10f;

    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D col;

    private Vector2 lastKnownPosition;
    private bool playerVisible = false;
    private float lastShotTime = -999f;

    private GameObject currentTarget;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        if (obstacleTilemap == null)
        {
            GameObject walls = GameObject.Find("Walls");
            if (walls != null) obstacleTilemap = walls.GetComponent<Tilemap>();
        }

        lastKnownPosition = transform.position;
    }

    void Update()
    {
        if (PlayerRegistry.Instance == null) return;

        currentTarget = PlayerRegistry.Instance.GetClosestPlayer(transform.position);
        if (currentTarget == null) return;

        Vector2 toPlayer = currentTarget.transform.position - transform.position;
        playerVisible = (toPlayer.magnitude <= detectionRadius) && HasLineOfSight(currentTarget.transform.position);

        if (playerVisible)
            lastKnownPosition = currentTarget.transform.position;

        // Disparo si está en rango y lo ve
        if (playerVisible && toPlayer.magnitude <= stoppingDistance)
        {
            TryShootAtTarget();
        }

        // Rotación del firePoint hacia el jugador
        if (firePoint != null && currentTarget != null)
        {
            Vector2 dir = (currentTarget.transform.position - firePoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            firePoint.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void FixedUpdate()
    {
        if (currentTarget == null) return;

        // No moverse si está disparando
        if (playerVisible && Vector2.Distance(transform.position, currentTarget.transform.position) <= stoppingDistance)
        {
            anim.SetBool("IsMoving", false);
            return;
        }

        MoveTowards(lastKnownPosition);
    }

    void MoveTowards(Vector2 targetPos)
    {
        Vector2 currentPos = rb.position;
        Vector2 delta = targetPos - currentPos;

        if (delta.magnitude < 0.1f)
        {
            anim.SetBool("IsMoving", false);
            return;
        }

        Vector2 moveDir = delta.normalized;
        Vector2 move = moveDir * moveSpeed * Time.fixedDeltaTime;

        // Comprobamos colisión antes de mover
        RaycastHit2D hit = Physics2D.BoxCast(rb.position, col.bounds.size, 0f, moveDir, move.magnitude, obstacleLayer);
        if (!hit)
        {
            rb.MovePosition(rb.position + move);
            anim.SetBool("IsMoving", true);
        }
        else
        {
            anim.SetBool("IsMoving", false);
        }

        // Actualizamos parámetros para animación (si tienes direcciones)
        anim.SetFloat("MoveX", moveDir.x);
        anim.SetFloat("MoveY", moveDir.y);
    }

    bool HasLineOfSight(Vector2 targetPos)
    {
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        float dist = Vector2.Distance(transform.position, targetPos);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, obstacleLayer);
        return hit.collider == null;
    }

    void TryShootAtTarget()
    {
        if (rayPrefab == null || firePoint == null) return;
        if (Time.time < lastShotTime + rayCooldown) return;

        lastShotTime = Time.time;

        Vector2 dir = (currentTarget.transform.position - firePoint.position).normalized;
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
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, lastKnownPosition);
    }
}
