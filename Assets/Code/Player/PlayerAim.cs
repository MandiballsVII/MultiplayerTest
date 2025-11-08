using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    private Transform aimTransform;
    public Transform shootPoint;
    
    private PlayerController playerController;
    private PlayerManager playerManager;

    // Input
    private Vector2 _aimStick;
    private Vector2 _aimPointer;
    private const float STICK_DEADZONE = 0.15f;

    private float lastStickAngle;

    [SerializeField] private Transform body; // Asigna el objeto Body en el inspector

    void Awake()
    {
        aimTransform = transform.Find("Body/Hands");
        print($"[PlayerAim] Aim Transform: {aimTransform.name}");
        //currentAmmo = maxAmmo;
        playerController = GetComponent<PlayerController>();
        playerManager = GetComponent<PlayerManager>();

        // Pasamos la referencia al PlayerManager
        if (playerManager != null)
        {
            playerManager.aimTransform = aimTransform;
        }
    }

    void Update()
    {
        if (!GetComponent<PlayerManager>().controlsEnabled) return;
        HandleAiming();
    }

    // === INPUT (Unity Events) ===
    public void AimStick(InputAction.CallbackContext ctx)
    {
        _aimStick = ctx.ReadValue<Vector2>();
    }

    public void AimPointer(InputAction.CallbackContext ctx)
    {
        _aimPointer = ctx.ReadValue<Vector2>(); // (Mouse.position)
    }

    // === Aiming ===
    void HandleAiming()
    {
        float angle;
        Vector3 dir;

        if (_aimStick.sqrMagnitude >= STICK_DEADZONE * STICK_DEADZONE)
        {
            angle = Mathf.Atan2(_aimStick.y, _aimStick.x) * Mathf.Rad2Deg;
            lastStickAngle = angle; // guardamos la última dirección válida
        }
        else
        {
            if (_aimPointer.sqrMagnitude > 0.1f)
            {
                Camera cam = Camera.main;
                Vector3 screenPos = new Vector3(_aimPointer.x, _aimPointer.y, Mathf.Abs(cam.transform.position.z));
                Vector3 worldMouse = cam.ScreenToWorldPoint(screenPos);
                dir = (worldMouse - transform.position).normalized;
                angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                lastStickAngle = angle; // también guardamos para ratón
            }
            else
            {
                angle = lastStickAngle; // usamos la última dirección válida
            }
        }

        // Rotar Aim (manos/arma)
        if (aimTransform != null)
        {
            aimTransform.eulerAngles = new Vector3(0, 0, angle);

            Vector3 aimLocalScale = Vector3.one;
            aimLocalScale.y = (angle > 90 || angle < -90) ? -1f : 1f;
            aimTransform.localScale = aimLocalScale;
        }

        // Flipping del cuerpo
        if (body != null)
        {
            Vector3 bodyScale = body.localScale;
            bodyScale.x = (angle > 90 || angle < -90) ? -1f : 1f;
            body.localScale = bodyScale;
        }
    }

}
