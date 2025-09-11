using UnityEngine;
using System.Collections.Generic;

public class NPC_SpellCaster : NPC_ControllerBase
{
    public enum SpellType { Projectile, Area, Self, Summon }

    [Header("Spell Config")]
    public SpellData assignedSpell;

    private float attackTimer = 0f;
    private float pathUpdateCooldown = 0.25f;
    private float pathUpdateTimer = 0f;

    [SerializeField] Transform firePoint;

    // Control de idle
    private float idleTimer = 0f;
    public float idleDuration = 2f;
    private StateMachine nextStateAfterIdle;

    // Summoner
    public float desiredDistanceFromTarget = 6f;
    private List<GameObject> activeSummons = new List<GameObject>();

    // --- Para recordar el nodo objetivo en búsqueda ---
    private Node searchGoalNode = null;

    // Healer
    private IAttackable allyTarget;

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
            return;
        }

        if (assignedSpell == null)
        {
            Debug.LogWarning($"{name} no tiene SpellData asignado.");
            return;
        }

        switch (assignedSpell.type)
        {
            case SpellType.Projectile:
            case SpellType.Area:
                HandleRangedStyle();
                break;

            case SpellType.Summon:
                HandleSummonerStyle();
                break;

            case SpellType.Self:
                HandleHealerStyle();
                break;
        }
    }

    // ========== PROJECTILE / AREA ==========
    private void HandleRangedStyle()
    {
        if (target != null)
        {
            float dist = Vector2.Distance(transform.position, target.position);
            if (dist <= attackRange)
            {
                currentState = StateMachine.Engage;
                animator?.SetInteger("State", 1);
                AttackProjectile();
            }
            else
            {
                currentState = StateMachine.Engage;
                animator?.SetInteger("State", 0);
                ChaseTarget(target.position);
            }
        }
        else if (lastKnownPosition.HasValue)
        {
            currentState = StateMachine.Search;
            animator?.SetInteger("State", 0);
            SearchLastKnown();
        }
        else
        {
            Patrol();
        }
    }

    private void AttackProjectile()
    {
        if (target == null) return;

        attackTimer += Time.deltaTime;
        RotateTowards(target.position);

        if (attackTimer >= assignedSpell.cooldown)
        {
            if (assignedSpell.prefab != null)
            {
                Vector2 dir = (target.position - firePoint.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                var proj = Instantiate(assignedSpell.prefab, firePoint.position, Quaternion.AngleAxis(angle, Vector3.forward));
                var rb = proj.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.velocity = dir * assignedSpell.projectileSpeed;

                Debug.Log($"{name} lanza {assignedSpell.displayName} a {target.name}");
            }
            attackTimer = 0f;
        }
    }

    // ========== SUMMONER ==========
    private void HandleSummonerStyle()
    {
        if (target != null)
        {
            float dist = Vector2.Distance(transform.position, target.position);

            // mantener distancia
            if (dist < desiredDistanceFromTarget)
            {
                Vector2 dir = (transform.position - target.position).normalized;
                transform.position += (Vector3)(dir * speed * Time.deltaTime);
            }

            attackTimer += Time.deltaTime;
            if (attackTimer >= assignedSpell.cooldown && activeSummons.Count < assignedSpell.summonCount)
            {
                SummonCreature();
                attackTimer = 0f;
            }
        }
        else if (lastKnownPosition.HasValue)
        {
            currentState = StateMachine.Search;
            animator?.SetInteger("State", 0);
            SearchLastKnown();
        }
        else
        {
            Patrol();
        }
    }

    private void SummonCreature()
    {
        if (assignedSpell.summonPrefab == null) return;

        var summon = Instantiate(assignedSpell.summonPrefab, transform.position + Vector3.right, Quaternion.identity);
        activeSummons.Add(summon);
        Destroy(summon, assignedSpell.summonDuration);

        Debug.Log($"{name} invoca {assignedSpell.summonPrefab.name}");
    }

    // ========== HEALER ==========
    private void HandleHealerStyle()
    {
        Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, allyLayer);
        IAttackable weakest = null;
        float lowestHpRatio = 1f;

        foreach (var a in allies)
        {
            var att = a.GetComponent<IAttackable>();
            if (att != null && att.IsAlive && att != this)
            {
                float ratio = (att as NPC_ControllerBase)?.GetHealthRatio() ?? 1f;
                if (ratio < lowestHpRatio)
                {
                    lowestHpRatio = ratio;
                    weakest = att;
                }
            }
        }

        if (weakest != null)
        {
            allyTarget = weakest;
            float dist = Vector2.Distance(transform.position, ((MonoBehaviour)weakest).transform.position);
            if (dist > attackRange)
            {
                ChaseTarget(((MonoBehaviour)weakest).transform.position);
            }
            else
            {
                AttackHeal(weakest);
            }
        }
        else
        {
            // huir si hay jugador
            if (target != null)
            {
                Vector2 dir = (transform.position - target.position).normalized;
                transform.position += (Vector3)(dir * speed * Time.deltaTime);
            }
            else
            {
                Patrol();
            }
        }
    }

    private void AttackHeal(IAttackable ally)
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= assignedSpell.cooldown)
        {
            ally.Heal(assignedSpell.healAmount);
            Debug.Log($"{name} cura a {((MonoBehaviour)ally).name} con {assignedSpell.displayName}");
            attackTimer = 0f;
        }
    }

    // ========== PATHFINDING ==========

    private void Patrol()
    {
        if (path.Count == 0)
        {
            Node randomNode = nodeManager.GetRandomNode();
            if (randomNode != null)
                path = AStarManager.instance.GeneratePath(currentNode, randomNode, radiusInTiles);

            currentState = StateMachine.Idle;
            nextStateAfterIdle = StateMachine.Patrol;
        }
    }

    private void ChaseTarget(Transform destination)
    {
        if (HasLineOfSight(destination))
        {
            RotateTowards(destination.position);
            transform.position = Vector3.MoveTowards(transform.position, destination.position, speed * Time.deltaTime);
            return;
        }

        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f)
        {
            Node tnode = GetClosestNode(destination.position);
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

        // Actualizamos el nodo actual
        currentNode = GetClosestNode(transform.position);

        // Si no tenemos path, lo calculamos
        if (path.Count == 0)
        {
            int requiredClearance = Mathf.Max(0, Mathf.CeilToInt(radiusInTiles) - 1);
            Node goal = nodeManager.GetClosestNodeWithClearance(lastKnownPosition.Value, requiredClearance, maxRadius: 8);

            if (goal != null && currentNode != null)
            {
                Debug.Log($"A* start.cl={currentNode.clearance} goal.cl={goal.clearance} req={requiredClearance}");
                path = AStarManager.instance.GeneratePath(currentNode, goal, radiusInTiles);

                // Guardamos el nodo de destino real para comparar luego
                searchGoalNode = goal;
            }
            else
            {
                Debug.LogWarning($"{name}: no se encontró nodo válido cerca de la última posición conocida. Cancelando búsqueda.");
                lastKnownPosition = null;
                path.Clear();
                currentState = StateMachine.Patrol;
                return;
            }
        }

        // Si hemos llegado suficientemente cerca al nodo de destino
        if (searchGoalNode != null && Vector2.Distance(transform.position, searchGoalNode.transform.position) < 0.2f)
        {
            Debug.Log($"{name} terminó búsqueda y no encontró al jugador. Vuelve a patrullar.");
            lastKnownPosition = null;
            path.Clear();
            currentState = StateMachine.Idle; // pausa breve antes de retomar patrulla
            nextStateAfterIdle = StateMachine.Patrol;
            searchGoalNode = null; // limpiamos referencia
        }
    }
}
