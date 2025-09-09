using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Arrow : MonoBehaviour
{
    Rigidbody2D rb;
    Collider2D col;

    [Header("Tiempos")]
    public float flyLifetime = 5f;     // tiempo máximo en vuelo
    public float stuckLifetime = 3f;   // tiempo que permanece clavada

    [Header("Ajustes visuales")]
    public float embedDepth = 0.05f;   // cuánto se mete la punta dentro del objetivo
    [Tooltip("Si tu sprite apunta hacia arriba pon 90, si apunta a la derecha pon 0")]
    public float angleOffset = 0f;

    bool stuck = false;
    private Vector2 lastVelocity;
    private Coroutine destroyCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        // Iniciar destrucción en vuelo
        destroyCoroutine = StartCoroutine(DestroyAfter(flyLifetime));
    }

    private void FixedUpdate()
    {
        // Guardar la última velocidad real del rigidbody (para usar en el impacto)
        lastVelocity = rb.velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (stuck) return;

        // Obtener punto de contacto (si hay varios, usamos el primero)
        ContactPoint2D contact = collision.contacts[0];
        Vector2 hitPoint = contact.point;

        // Dirección de impacto preferente: velocidad instantánea (fallback al vector between)
        Vector2 dir = lastVelocity.sqrMagnitude > 0.001f ? lastVelocity.normalized :
                      (hitPoint - (Vector2)transform.position).normalized;

        StickToTarget(collision.transform, hitPoint, dir);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (stuck) return;

        // Si quieres filtrar por tag: Uncomment la siguiente línea y ajusta
        // if (!collision.CompareTag("Player") && !collision.CompareTag("Enemy")) return;

        Vector2 hitPoint = transform.position; // no hay contacto, usamos posición actual
        Vector2 dir = lastVelocity.sqrMagnitude > 0.001f ? lastVelocity.normalized :
                      (hitPoint - (Vector2)transform.position).normalized;

        StickToTarget(collision.transform, hitPoint, dir);
    }

    void StickToTarget(Transform target, Vector2 hitPoint, Vector2 direction)
    {
        stuck = true;

        // Detener físicas
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true;
        rb.simulated = false;

        // Cancelar destrucción en vuelo y programar destrucción clavada
        if (destroyCoroutine != null) StopCoroutine(destroyCoroutine);
        destroyCoroutine = StartCoroutine(DestroyAfter(stuckLifetime));

        // Calcular rotación según la dirección de impacto
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angleOffset;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        // Colocar embebida
        Vector3 embedPos = (Vector3)hitPoint + (Vector3)direction * embedDepth;
        transform.position = embedPos;
        transform.rotation = rot;

        // Cambiar sorting layer para que quede bajo objetos
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Walls";
            sr.sortingOrder = 0;
        }

        // Buscar el mejor padre (body si existe, fallback al root)
        Transform parent = GetBestParent(target);
        transform.SetParent(parent, true);

        // Desactivar colisión para evitar interferencias
        if (col != null) col.enabled = false;
    }


    Transform GetBestParent(Transform hitTransform)
    {
        // Caso especial: si el objeto tiene un hijo llamado "Body", usarlo
        var body = hitTransform.Find("Body");
        if (body != null)
            return body;

        // En caso de que tenga Rigidbody2D en un hijo (ej. rigs), podemos buscarlo
        var childRb = hitTransform.GetComponentInChildren<Rigidbody2D>();
        if (childRb != null)
            return childRb.transform;

        // Por defecto: usar el transform del objeto golpeado
        return hitTransform;
    }


    IEnumerator DestroyAfter(float t)
    {
        yield return new WaitForSeconds(t);
        Destroy(gameObject);
    }

    // Método público para forzar destrucción inmediata (p. ej. si el objetivo muere)
    public void DestroyNow()
    {
        if (destroyCoroutine != null) StopCoroutine(destroyCoroutine);
        Destroy(gameObject);
    }
}
