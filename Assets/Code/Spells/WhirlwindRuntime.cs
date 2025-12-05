using System.Collections;
using UnityEngine;

public class WhirlwindRuntime : MonoBehaviour
{
    [Header("Orbit Movement")]
    public float orbitRadius = 1.5f;
    public float orbitSpeed = 180f;   // grados por segundo
    public bool followCasterPosition = true;
    private Vector2 orbitCenter;

    [Header("Durations")]
    public float totalDuration = 5f;  // tiempo en idle antes de finalizar
    public float endAnimDuration = 1.2f; // duración de animación End

    private Transform caster;
    private Animator animator;

    private float angle = 0f;
    private float elapsed = 0f;
    private bool ending = false;

    private AreaSpellRuntime area;
    private ProjectileFaction faction;

    public void Init(PlayerSpellBook owner, SpellData data)
    {
        caster = owner.transform;
        animator = GetComponent<Animator>();
        area = GetComponent<AreaSpellRuntime>();
        faction = GetComponent<ProjectileFaction>();
        orbitCenter = owner.transform.position;

        totalDuration = data.duration;
        // Inicializar lógica de daño (igual que cualquier área normal)
        area?.Init(owner, data);

        // Animación inicial del vfx
        animator.Play("Spawn");
    }

    private void Update()
    {
        if (ending) return;

        elapsed += Time.deltaTime;

        // Movimiento orbital
        UpdateOrbit();

        // ¿termina el hechizo?
        if (elapsed >= totalDuration - endAnimDuration)
        {
            StartEnding();
        }
    }

    private void UpdateOrbit()
    {
        if (caster == null) return;

        angle += orbitSpeed * Time.deltaTime;

        Vector2 offset = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * orbitRadius;

        if (followCasterPosition)
        {
            // orbita alrededor del caster
            transform.position = (Vector2)caster.position + offset;
        }
        else
        {
            // orbita alrededor del punto inicial
            transform.position = orbitCenter + offset;
        }
    }

    private void StartEnding()
    {
        if (ending) return;
        ending = true;

        animator.SetTrigger("End");

        // Detener área de daño para que no siga afectando
        if (area != null)
            area.enabled = false;
    }
    
    public void DestroyWhirlpool()
    {
        Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log($"[WHIRLWIND] Entró {col.name} ({LayerMask.LayerToName(col.gameObject.layer)})");
    }

}
