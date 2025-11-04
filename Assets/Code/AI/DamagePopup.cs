using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private float lifetime = 1f;

    private Vector3 moveVector;
    private float gravity = -2f;
    private float moveSpeed = 1.5f;

    private Camera cam;

    private Canvas parentCanvas;

    private void Awake()
    {
        parentCanvas = GetComponentInChildren<Canvas>();
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        cam = Camera.main;
        parentCanvas.worldCamera = cam;

        // Movimiento inicial aleatorio ligero
        moveVector = new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(0.7f, 1.2f),
            0
        );
    }

    public void Setup(float damage, float healthPercent)
    {
        textMesh.text = Mathf.RoundToInt(damage).ToString();

        // Color: Verde -> Amarillo -> Rojo según % vida
        Color finalColor = Color.Lerp(Color.red, Color.green, healthPercent);
        textMesh.color = finalColor;

        // Asegurar visibilidad
        textMesh.alpha = 1f;
    }

    private void LateUpdate()
    {
        // Movimiento tipo salto
        transform.position += moveVector * moveSpeed * Time.deltaTime;

        moveVector.y += gravity * Time.deltaTime;

        // Siempre mirar a la cámara top-down
        if (cam != null)
            transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
            Destroy(gameObject);
    }
}
