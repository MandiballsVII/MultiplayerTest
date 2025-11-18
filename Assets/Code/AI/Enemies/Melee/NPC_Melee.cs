using UnityEngine;
using UnityEngine.Timeline;

[RequireComponent(typeof(Health))]
public class NPC_Melee : NPC_ControllerBase
{
    // Tiempo entre recalculaciones de path (para no recalcular cada frame)
    private float pathUpdateCooldown = 0.25f;
    private float pathUpdateTimer = 0f;

     // --- Nuevo: control de pausas entre estados ---
    private float idleTimer = 0f;
    public float idleDuration = 2f; // duración del idle en segundos
    private StateMachine nextStateAfterIdle; // para saber a dónde ir tras la pausa

    [Header("Combate")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1f, 1f);
    [SerializeField] private float attackOffset = 0.8f;
    [SerializeField] private float attackCooldown = 1f;

    private float nextAttackTime;

    protected override void UpdateState()
    {
        if(currentState == StateMachine.Idle)
        {
            animator?.SetInteger("State", 2);
            idleTimer += Time.deltaTime;
            if(idleTimer >= idleDuration)
            {
                currentState = nextStateAfterIdle;
                animator?.SetInteger("State", 0);
                idleTimer = 0f;
            }
            return; // no hacer nada más mientras está en idle
        }
        if (target != null)
        {
            // Engaging directamente
            path.Clear();
            float dist = Vector2.Distance(transform.position, target.position);
            if (dist <= attackRange)
            {
                currentState = StateMachine.Engage;
                animator?.SetInteger("State", 1);
                Attack();
            }
            else
            {
                currentState = StateMachine.Engage;
                animator?.SetInteger("State", 0);
                ChaseTarget(target.position); // se mueve directo o por path si hay obstáculos
            }
        }
        else if (lastKnownPosition.HasValue)
        {
            // Buscar última posición conocida
            currentState = StateMachine.Search;
            animator?.SetInteger("State", 0);
            SearchLastKnown();
        }
        else
        {
            // Patrullaje
            currentState = StateMachine.Patrol;
            animator?.SetInteger("State", 0);
            Patrol();
        }
    }


    void Patrol()
    {
        if (path.Count == 0)
        {
            Node randomNode = nodeManager.GetRandomNode();
            if (randomNode != null)
                path = AStarManager.instance.GeneratePath(currentNode, randomNode, radiusInTiles);
            currentState = StateMachine.Idle; // pausa antes de patrullar
            nextStateAfterIdle = StateMachine.Patrol;
        }
    }

    void ChaseTarget(Vector3 destination)
    {
        if (target == null) return;

        // Movimiento directo si hay línea de visión
        if (HasLineOfSight(target))
        {
            RotateTowards(target.position);
            if (!CanMove) return;
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            return; // no usamos pathfinding mientras la visión sea clara
        }

        // Si no hay LOS, usamos pathfinding
        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f)
        {
            Node tnode = GetClosestNode(destination);
            if (tnode != null && currentNode != null)
            {
                path = AStarManager.instance.GeneratePath(currentNode, tnode, radiusInTiles);
            }
            pathUpdateTimer = pathUpdateCooldown;
        }
    }



    void SearchLastKnown()
    {
        if (!lastKnownPosition.HasValue) return;

        // Actualizamos currentNode para reflejar la posición actual real
        currentNode = GetClosestNode(transform.position);

        if (path.Count == 0)
        {
            Node goal = GetClosestNode(lastKnownPosition.Value);
            if (goal != null && currentNode != null)
                path = AStarManager.instance.GeneratePath(currentNode, goal, radiusInTiles);
        }
        if (Vector2.Distance(transform.position, lastKnownPosition.Value) < 0.8f)
        {
            print(name + " no encontró al jugador, vuelve a patrullar");
            lastKnownPosition = null;
            path.Clear();
            currentState = StateMachine.Idle; // pausa antes de patrullar
            nextStateAfterIdle = StateMachine.Patrol;
        }
    }

    void Attack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        RotateTowards(target.position);

        // Dirección de ataque
        Vector2 dir;
        if (gameObject.name.Contains("Skeleton"))
            dir = transform.right;
        else
            dir = -transform.up; // o transform.right si tus sprites miran hacia la derecha
        Vector2 origin = (Vector2)transform.position + dir * attackOffset;

        // Realizar boxcast
        RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, attackBoxSize, 0f, dir, 0f);

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.gameObject == gameObject) continue; // no golpearse a sí mismo

            var otherHealth = hit.collider.GetComponent<Health>();
            var otherFaction = hit.collider.GetComponent<IFactionMember>();

            if (otherHealth != null && otherFaction != null && otherFaction.Faction != this.faction)
            {
                otherHealth.TakeDamage(attackDamage, gameObject.transform.position);
                Debug.Log($"{name} golpea a {hit.collider.name} ({otherFaction.Faction}) por {attackDamage} daño");
            }
        }
    }

    // ===================== GIZMOS =====================
    protected new void OnDrawGizmosSelected()
    {
        // Llamar al método base para mantener la visualización general (detección, path, etc.)
        base.OnDrawGizmosSelected();

        // Parámetros del ataque
        Vector2 dir;
        if(gameObject.name.Contains("Skeleton"))
            dir = transform.right;
        else
            dir = -transform.up; // o transform.right si tus sprites miran hacia la derecha
        Vector2 origin = (Vector2)transform.position + dir * attackOffset;

        // Dibujar la caja del área de golpe
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, attackBoxSize);

        // Línea de referencia
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, origin);
    }
#if UNITY_EDITOR
    private new void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        // Solo mostrar durante el juego (para depuración)
        if (!Application.isPlaying) return;

        // Mismo cálculo del box
        Vector2 dir;
        if (gameObject.name.Contains("Skeleton"))
            dir = transform.right;
        else
            dir = -transform.up; // o transform.right si tus sprites miran hacia la derecha
        Vector2 origin = (Vector2)transform.position + dir * attackOffset;

        // Hacer un BoxCast como en el ataque real
        RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, attackBoxSize, 0f, dir, 0f);

        bool hitSomething = false;
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.gameObject == gameObject) continue;

            var otherNPC = hit.collider.GetComponent<NPC_ControllerBase>();
            if (otherNPC != null && otherNPC.faction != this.faction)
            {
                hitSomething = true;
                // Dibuja un punto donde golpeó
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(hit.point, 0.1f);
            }
        }

        // Dibuja el box: verde si golpea algo, rojo si no
        Gizmos.color = hitSomething ? Color.green : Color.red;
        Gizmos.matrix = Matrix4x4.TRS(origin, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, attackBoxSize);
    }
#endif


}
