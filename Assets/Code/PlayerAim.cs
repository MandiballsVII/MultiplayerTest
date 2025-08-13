using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    private Transform aimTransform;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletForce = 20f;

    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private int maxAmmo = 20;

    public int currentAmmo;
    private bool recharging;

    // Input
    private Vector2 _aimStick;
    private Vector2 _aimPointer;
    private const float STICK_DEADZONE = 0.15f;

    void Awake()
    {
        aimTransform = transform.Find("Aim");
        currentAmmo = maxAmmo;
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
        _aimPointer = ctx.ReadValue<Vector2>(); // posición de pantalla (Mouse.position)
    }

    public void Fire(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        HandleShooting();
    }

    public void Reload(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (!recharging && currentAmmo < maxAmmo) StartCoroutine(Recharge());
    }

    // === Aiming ===
    void HandleAiming()
    {
        float angle;
        Vector3 dir;

        // 1) Prioriza stick si está por encima del deadzone
        if (_aimStick.sqrMagnitude >= STICK_DEADZONE * STICK_DEADZONE)
        {
            angle = Mathf.Atan2(_aimStick.y, _aimStick.x) * Mathf.Rad2Deg;
        }
        else
        {
            Camera cam = Camera.main;
            Vector3 screenPos = new Vector3(_aimPointer.x, _aimPointer.y, Mathf.Abs(cam.transform.position.z));
            Vector3 worldMouse = cam.ScreenToWorldPoint(screenPos);
            dir = (worldMouse - transform.position).normalized;
            angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }

        if (aimTransform != null)
        {
            aimTransform.eulerAngles = new Vector3(0, 0, angle);

            // Flip del sprite del arma
            Vector3 aimLocalScale = Vector3.one;
            aimLocalScale.y = (angle > 90 || angle < -90) ? -1f : 1f;
            aimTransform.localScale = aimLocalScale;
        }
    }

    // === Shooting ===
    void HandleShooting()
    {
        if (recharging) return;

        if (currentAmmo <= 0)
        {
            recharging = true;
            StartCoroutine(Recharge());
            return;
        }

        StartCoroutine(ShowFlash());

        if (bulletPrefab != null && shootPoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
            if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.AddForce(shootPoint.right * bulletForce, ForceMode2D.Impulse);
            }
        }

        currentAmmo--;
        if (currentAmmo <= 0)
        {
            recharging = true;
            StartCoroutine(Recharge());
        }
    }

    IEnumerator ShowFlash()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
            yield return new WaitForSeconds(.1f);
            muzzleFlash.SetActive(false);
        }
    }

    IEnumerator Recharge()
    {
        yield return new WaitForSeconds(1f);
        currentAmmo = maxAmmo;
        recharging = false;
    }
}
