using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CapsuleCollider2D))]
public class PlayerManager : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private CapsuleCollider2D capsuleCollider;

    private Vector2 moveInput;
    public bool controlsEnabled = true;

    [Header("Movement")]
    public int movementSpeed = 6;
    public int healthPoints = 3;

    [Header("Dash Settings")]
    public float dashDistance = 5f;       // Distancia del dash
    public float dashDuration = 0.2f;     // Tiempo que dura el dash
    public float dashCooldown = 1f;       // Tiempo entre dashes
    public bool dashInvulnerability = true;

    private bool isDashing = false;
    public bool IsDashing => isDashing; // Propiedad pública de solo lectura
    private bool canDash = true;

    [HideInInspector] public bool invulnerable;
    [HideInInspector] public bool isDead;

    [SerializeField] GameObject deathMenu;
    [SerializeField] private TrailRenderer dashTrail;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    void OnEnable() { MultiTargetCamera.Instance?.Register(transform); }
    void OnDisable() { MultiTargetCamera.Instance?.Unregister(transform); }

    void FixedUpdate()
    {
        if (!controlsEnabled || isDead || isDashing)
        {
            if (!isDashing) rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = moveInput.normalized * movementSpeed;

        bool moving = rb.velocity.sqrMagnitude > 0.0001f;
        animator.SetBool("Moving", moving);

        if (moving)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // === Stay into camera space ===
        var camBounds = MultiTargetCamera.Instance.GetCameraBounds();
        Vector3 pos = transform.position;

        float halfWidth = capsuleCollider.bounds.extents.x;
        float halfHeight = capsuleCollider.bounds.extents.y;

        pos.x = Mathf.Clamp(pos.x, camBounds.min.x + halfWidth, camBounds.max.x - halfWidth);
        pos.y = Mathf.Clamp(pos.y, camBounds.min.y + halfHeight, camBounds.max.y - halfHeight);

        transform.position = pos;
    }

    // === INPUT (Unity Events) ===
    public void Move(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        print(ctx);
        if (!ctx.performed || !canDash || isDead || moveInput == Vector2.zero) return;
        StartCoroutine(PerformDash());
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;
        if (dashInvulnerability) invulnerable = true;

        // Activar trail
        if (dashTrail != null) dashTrail.emitting = true;

        Vector2 dashDirection = moveInput.normalized;
        float dashSpeed = dashDistance / dashDuration;
        rb.velocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector2.zero;
        if (dashInvulnerability) invulnerable = false;
        isDashing = false;

        // Desactivar trail
        if (dashTrail != null) dashTrail.emitting = false;
        dashTrail.Clear();

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
