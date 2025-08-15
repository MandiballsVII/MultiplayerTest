using UnityEngine;
using UnityEngine.InputSystem;

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

    [HideInInspector] public bool invulnerable;
    [HideInInspector] public bool isDead;

    [SerializeField] GameObject deathMenu;

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
        if (!controlsEnabled || isDead)
        {
            rb.velocity = Vector2.zero;
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
}
