using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CapsuleCollider2D))]
public class PlayerManager : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private CapsuleCollider2D capsuleCollider;
    private GameObject playerBody;
    private GameObject playerHands;
    private Animator bodyAnimator;
    private Animator handsAnimator;

    private Vector2 moveInput;
    public bool controlsEnabled = true;

    [Header("Movement")]
    public int movementSpeed = 6;
    public int healthPoints = 3;

    [Header("Dash Settings")]
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public bool dashInvulnerability = true;

    private bool isDashing = false;
    public bool IsDashing => isDashing;
    private bool canDash = true;

    [HideInInspector] public bool invulnerable;
    [HideInInspector] public bool isDead;

    [SerializeField] GameObject deathMenu;
    [SerializeField] private TrailRenderer dashTrail;

    [Header("References")]
    public Transform bodyTransform; // Nuevo: el sprite del cuerpo
    public Transform aimTransform;  // Nuevo: referencia al Aim (lo setea PlayerAim en Awake)

    public enum PlayerAnimState
    {
        Idle,
        MoveForward,
        MoveBack,
        MoveLeft,
        MoveRight
    }

    private PlayerAnimState currentState = PlayerAnimState.Idle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        playerBody = transform.Find("Body").gameObject;
        playerHands = playerBody.transform.Find("Hands").gameObject;
        print($"[PlayerManager] Body: {playerBody.name}, Hands: {playerHands.name}");
        bodyAnimator = playerBody.GetComponent<Animator>();
        handsAnimator = playerHands.GetComponent<Animator>();
    }

    void OnEnable() { MultiTargetCamera.Instance?.Register(transform); }
    void OnDisable() { MultiTargetCamera.Instance?.Unregister(transform); }

    void FixedUpdate()
    {
        Vector2 moveDir = moveInput; // Vector2 del Input System (x, y)
        if (!controlsEnabled || isDead || isDashing)
        {
            if (!isDashing) rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = moveInput.normalized * movementSpeed;

        bool moving = rb.velocity.sqrMagnitude > 0.0001f;
        
        // Ya NO rotamos el transform según el movimiento
        // Ahora el cuerpo mira donde apuntan las manos (Aim)
        if (aimTransform != null && bodyTransform != null)
        {
            float aimAngle = aimTransform.eulerAngles.z;
            bodyTransform.rotation = Quaternion.Euler(0, 0, aimAngle);
        }

        // === Stay into camera space ===
        var camBounds = MultiTargetCamera.Instance.GetCameraBounds();
        Vector3 pos = transform.position;

        float halfWidth = capsuleCollider.bounds.extents.x;
        float halfHeight = capsuleCollider.bounds.extents.y;

        pos.x = Mathf.Clamp(pos.x, camBounds.min.x + halfWidth, camBounds.max.x - halfWidth);
        pos.y = Mathf.Clamp(pos.y, camBounds.min.y + halfHeight, camBounds.max.y - halfHeight);

        transform.position = pos;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Vector2 aimDir = aimTransform.right;
            float angle = Vector2.SignedAngle(aimDir, moveDir);

            if (angle > -45f && angle <= 45f)
                ChangeAnimation(PlayerAnimState.MoveForward);
            else if (angle > 45f && angle <= 135f)
                ChangeAnimation(PlayerAnimState.MoveLeft);
            else if (angle <= -45f && angle > -135f)
                ChangeAnimation(PlayerAnimState.MoveRight);
            else
                ChangeAnimation(PlayerAnimState.MoveBack);
        }
        else
        {
            ChangeAnimation(PlayerAnimState.Idle);
        }
    }

    private void ChangeAnimation(PlayerAnimState newState)
    {
        if (currentState == newState) return; // Ya estamos en esa animación

        currentState = newState;

        switch (newState)
        {
            case PlayerAnimState.Idle:
                bodyAnimator.Play("BodyIdle");
                handsAnimator.Play("HandsIdle");
                break;
            case PlayerAnimState.MoveForward:
                bodyAnimator.Play("BodyMoveForward");
                handsAnimator.Play("HandsMovingForward");
                break;
            case PlayerAnimState.MoveBack:
                bodyAnimator.Play("BodyMoveBack");
                handsAnimator.Play("HandsMovingBack");
                break;
            case PlayerAnimState.MoveLeft:
                bodyAnimator.Play("BodyMoveLeft");
                handsAnimator.Play("HandsMovingLeft");
                break;
            case PlayerAnimState.MoveRight:
                bodyAnimator.Play("BodyMoveRight");
                handsAnimator.Play("HandsMovingRight");
                break;
        }
    }


    // === INPUT (Unity Events) ===
    public void Move(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !canDash || isDead || moveInput == Vector2.zero) return;
        StartCoroutine(PerformDash());
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;
        if (dashInvulnerability) invulnerable = true;

        if (dashTrail != null) dashTrail.emitting = true;

        Vector2 dashDirection = moveInput.normalized;
        float dashSpeed = dashDistance / dashDuration;
        rb.velocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector2.zero;
        if (dashInvulnerability) invulnerable = false;
        isDashing = false;

        if (dashTrail != null) dashTrail.emitting = false;
        dashTrail.Clear();

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
