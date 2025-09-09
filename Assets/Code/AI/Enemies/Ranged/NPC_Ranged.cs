using UnityEngine;

public class NPC_Ranged : NPC_ControllerBase
{
    // Tiempo entre recalculaciones de path (para no recalcular cada frame)
    private float pathUpdateCooldown = 0.25f;
    private float pathUpdateTimer = 0f;

    // --- Nuevo: control de pausas entre estados ---
    private float idleTimer = 0f;
    public float idleDuration = 2f; // duración del idle en segundos
    private StateMachine nextStateAfterIdle; // para saber a dónde ir tras la pausa

    public GameObject projectilePrefab; // Prefab del proyectil a instanciar

    public float attackCooldown = 1.5f; // Tiempo entre ataques
    private float attackTimer = 0f;
    public Transform firePoint; // Punto desde donde se dispara el proyectil

    protected override void UpdateState()
    {
        if (currentState == StateMachine.Idle)
        {
            animator?.SetInteger("State", 2);
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDuration)
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
                path = AStarManager.instance.GeneratePath(currentNode, randomNode);
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
                path = AStarManager.instance.GeneratePath(currentNode, tnode);
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
                path = AStarManager.instance.GeneratePath(currentNode, goal);
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
        if (target == null) return;

        attackTimer += Time.deltaTime;
        RotateTowards(target.position);

        if (attackTimer >= attackCooldown)
        {
            // Dirección hacia el objetivo
            Vector2 direction = (target.position - firePoint.position).normalized;

            // Calcular ángulo en grados
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Instanciar proyectil con rotación hacia el objetivo
            var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.AngleAxis(angle, Vector3.forward));

            // Darle velocidad
            projectile.GetComponent<Rigidbody2D>().velocity = direction * 15f;

            Debug.Log($"{name} dispara a {target.name}");
            attackTimer = 0f;
        }
    }

}
