using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class NPC_Beholder : NPC_ControllerBase
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
    public float projectileSpeed = 15f; // Velocidad del proyectil

    protected override void UpdateState()
    {
        radiusInTiles = 2f; // Beholder es más grande
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
            if (randomNode != null && currentNode != null)
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

        // Actualizamos currentNode
        currentNode = GetClosestNode(transform.position);

        if (path.Count == 0)
        {
            int requiredClearance = Mathf.Max(0, Mathf.CeilToInt(radiusInTiles) - 1);

            // buscamos un nodo objetivo que cumpla clearance cerca de la última posición
            Node goal = nodeManager.GetClosestNodeWithClearance(lastKnownPosition.Value, requiredClearance, maxRadius: 8);

            if (goal != null && currentNode != null)
            {
                Debug.Log($"A* start.cl={currentNode.clearance} goal.cl={goal.clearance} req={requiredClearance}");

                path = AStarManager.instance.GeneratePath(currentNode, goal, radiusInTiles);
            }
            else
            {
                Debug.LogWarning($"{name}: no se encontró un nodo válido cerca de la última posición conocida (clearance req {requiredClearance}). Cancelo search.");
                lastKnownPosition = null;
                path.Clear();
                currentState = StateMachine.Patrol;
                return;
            }
        }

        if (Vector2.Distance(transform.position, lastKnownPosition.Value) < 0.8f)
        {
            Debug.Log($"{name} no encontró al jugador, vuelve a patrullar");
            lastKnownPosition = null;
            path.Clear();
            currentState = StateMachine.Idle;
            nextStateAfterIdle = StateMachine.Patrol;
        }
    }



    void Attack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer < attackCooldown) return;

        int mask = LayerMask.GetMask("Player", "Summoned");
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, mask);

        if (hits.Length == 0) return;

        List<Transform> visibleTargets = new List<Transform>();
        foreach (var h in hits)
        {
            Vector2 dir = (h.transform.position - firePoint.position).normalized;
            float dist = Vector2.Distance(firePoint.position, h.transform.position);

            // Raycast para comprobar si hay una pared en medio
            RaycastHit2D hit = Physics2D.Raycast(firePoint.position, dir, dist, LayerMask.GetMask("Walls"));

            if (hit.collider == null) // no hay muro bloqueando
            {
                visibleTargets.Add(h.transform);
            }
        }

        if (visibleTargets.Count == 0) return;

        // Ordenar por distancia
        visibleTargets.Sort((a, b) =>
            Vector2.Distance(transform.position, a.position).CompareTo(Vector2.Distance(transform.position, b.position)));

        // Elegir hasta 4 objetivos
        int count = Mathf.Min(4, visibleTargets.Count);

        // Rotar hacia el más cercano
        RotateTowards(visibleTargets[0].position);

        for (int i = 0; i < count; i++)
        {
            Vector2 dir = (visibleTargets[i].position - firePoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.AngleAxis(angle, Vector3.forward));
            projectile.GetComponent<BeholderRay>().Init(dir, projectileSpeed);
        }

        Debug.Log($"{name} dispara a {count} objetivos (con línea de visión)");
        attackTimer = 0f;
    }


}
