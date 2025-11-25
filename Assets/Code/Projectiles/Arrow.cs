using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Arrow : MonoBehaviour
{
    Rigidbody2D rb;
    Collider2D col;

    [Header("Times")]
    public float flyLifetime = 5f;     // tiempo máximo en vuelo
    public float stuckLifetime = 3f;   // tiempo que permanece clavada

    [Header("Visual settings")]
    public float embedDepth = 0.05f;   // cuánto se mete la punta dentro del objetivo
    [Tooltip("If your sprite is pointing up, set it to 90; if it is pointing right, set it to 0.")]
    public float angleOffset = 0f;

    [Header("Damage")]
    public float damage = 10f;
    public Faction faction; // <- facción del que la lanzó
    public Transform owner; // <- referencia al atacante (jugador o NPC)

    bool stuck = false;
    private Vector2 lastVelocity;
    private Coroutine destroyCoroutine;

    private GameObject impactedObject;

    [SerializeField] private bool damageOverTime = false;

    private float damageOverTimeInterval = 1f;

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

    private void Update()
    {
        if(stuck && impactedObject.GetComponent<Health>() && damageOverTime)
        {
            damageOverTimeInterval -= Time.deltaTime;
            if(damageOverTimeInterval <= 0f)
            {
                damageOverTimeInterval = 1f;
                var status = impactedObject.GetComponent<StatusEffectHandler>();
                if (status != null)
                {
                    status.ApplyDamage(5, transform.position);
                }
                else
                {
                    var h = impactedObject.GetComponent<Health>();
                    if (h != null) h.TakeDamage(5, transform.position);
                }

            }
        }
    }

    public void Init(Transform shooter, Faction shooterFaction, float projectileDamage)
    {
        owner = shooter;
        faction = shooterFaction;
        damage = projectileDamage;

        IgnoreSameFactionCollisions();
    }
    void IgnoreSameFactionCollisions()
    {
        // Buscar todos los colliders de la misma facción
        var allies = FactionManager.Instance.GetColliders(faction);
        foreach (var allyCol in allies)
        {
            if (allyCol != null && col != null)
                Physics2D.IgnoreCollision(col, allyCol, true);
        }
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
        var targetFaction = target.GetComponent<IFactionMember>();
        var targetHealth = target.GetComponent<Health>();
        var status = target.GetComponent<StatusEffectHandler>();

        // --- 1. No golpear ni al dueño ni a la misma facción ---
        if (target == owner) return;
        if (targetFaction != null && targetFaction.Faction == faction) return;

        // --- 2. Aplicar daño respetando StatusEffectHandler ---
        if (status != null)
        {
            status.ApplyDamage(damage, transform.position);
        }
        else if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage, hitPoint);
        }

        // --- 3. Clavar flecha solo si tiene sentido ---
        // Si quieres que flechas NO se claven en paredes, añade:
        // if (targetHealth == null) return;

        stuck = true;
        impactedObject = target.gameObject;

        // --- 4. Desactivar físicas ---
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true;
        rb.simulated = false;

        if (destroyCoroutine != null) StopCoroutine(destroyCoroutine);
        destroyCoroutine = StartCoroutine(DestroyAfter(stuckLifetime));

        // --- 5. Rotación ---
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + angleOffset;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        Vector3 embedPos = (Vector3)hitPoint + (Vector3)direction * embedDepth;
        transform.position = embedPos;
        transform.rotation = rot;

        // --- 6. Sorting Layer ---
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Walls";
            sr.sortingOrder = 0;
        }

        // --- 7. Elegir mejor padre ---
        Transform parent = GetBestParent(target);
        transform.SetParent(parent, true);

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
