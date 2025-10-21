using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    public Health health;                // Asignado automáticamente o manualmente
    public Image fillImage;              // La imagen que representa la barra
    public Canvas canvas;                // El Canvas de la barra
    public Transform target;             // El enemigo al que seguirá (normalmente el root del prefab)

    [Header("Settings")]
    public float visibleDuration = 2f;   // Segundos que permanece visible tras recibir daño
    public Vector3 localOffset = new Vector3(0, 1.2f, 0); // Offset respecto al enemigo
    [Tooltip("Ajusta esto si el sprite base del enemigo está rotado (por ejemplo -90 para sprites que miran hacia abajo)")]
    public float rotationOffset = 0f;

    private Camera mainCam;
    private Coroutine hideRoutine;

    private void Awake()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        if (health == null)
            health = GetComponentInParent<Health>();
        mainCam = Camera.main;
    }

    private void Start()
    {
        if (canvas != null && canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;

        if (health != null)
            health.OnHealthChanged += UpdateBar;

        // Si no se ha asignado el target manualmente, usar el padre del Health
        if (target == null && health != null)
            target = health.transform;

        // Mantener el comportamiento anterior: oculta por defecto
        if (canvas != null)
            canvas.enabled = false;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnHealthChanged -= UpdateBar;
    }

    private void LateUpdate()
    {
        if (target == null || canvas == null)
            return;

        // Posición
        // La barra se coloca en un offset local respecto al enemigo (rotado con él)
        Vector3 worldPos = target.position + target.TransformDirection(localOffset);
        canvas.transform.position = worldPos;

        // Rotación
        // La barra sigue la rotación del enemigo (más un offset por prefab)
        Quaternion baseRot = target.rotation * Quaternion.Euler(0, 0, rotationOffset);
        canvas.transform.rotation = baseRot;
    }

    private void UpdateBar(float current, float max)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = current / max;

        // Mostrar barra
        if (canvas != null)
            canvas.enabled = true;

        // Reiniciar temporizador de ocultamiento
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(visibleDuration);
        if (canvas != null)
            canvas.enabled = false;
    }
}
