using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class NPC_SpellCaster : NPC_ControllerBase
{
    [Header("SpellCaster")]
    public SpellData spell; // Hechizo asignado a este NPC
    private float spellCooldownTimer = 0f;

    [Header("Summon settings")]
    public float summonDistance = 4f; // distancia a mantener del objetivo al invocar

    // Tiempo entre recalculaciones de path (para no recalcular cada frame)
    private float pathUpdateCooldown = 0.25f;
    private float pathUpdateTimer = 0f;

    // --- Nuevo: control de pausas entre estados ---
    private float idleTimer = 0f;
    public float idleDuration = 2f; // duración del idle en segundos
    private StateMachine nextStateAfterIdle; // para saber a dónde ir tras la pausa

    // --- Para recordar el nodo objetivo en búsqueda ---
    private Node searchGoalNode = null;

    public float attackCooldown = 1.5f; // Tiempo entre ataques
    private float attackTimer = 0f;
    public Transform firePoint; // Punto desde donde se dispara el proyectil
    public float projectileSpeed = 15f; // Velocidad del proyectil

    // Para detección de aliados (si aplica)
    float searchRadius;
    HashSet<Transform> candidates = new HashSet<Transform>();

    // Guardamos aquí al objetivo fijado cuando empieza la animación
    private NPC_ControllerBase currentHealTarget;
    private bool isCastingHeal = false;
    // Añadimos una variable para controlar si ya calculamos el path de huida
    private bool evadePathSet = false;

    protected override void UpdateState()
    {
        //print(evadePathSet);
        if (spell == null) return;

        // Reducir cooldown del hechizo
        //spellCooldownTimer -= Time.deltaTime;

        switch (spell.type)
        {
            case SpellType.Projectile:
                HandleRangedBehavior();
                break;
            case SpellType.Summon:
                HandleSummonerBehavior();
                break;
            case SpellType.Self:
                HandleSelfSupportBehavior();
                break;
        }

    }

    private void HandleSelfSupportBehavior()
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

        // Si ya estoy casteando, no cambio de objetivo
        if (isCastingHeal)
            return;
        DetectAllies();

        // Si ya tengo objetivo válido que aún necesita curación, sigo con él
        if (currentHealTarget != null)
        {
            if (currentHealTarget.TryGetComponent<Health>(out var h) && h.IsAlive && h.currentHealth < h.maxHealth)
                return; // sigue siendo válido, no cambiar
            else
                currentHealTarget = null; // si murió o ya está curado
        }

        // Buscar nuevo objetivo solo si no tengo uno
        List<NPC_ControllerBase> damagedAllies = new List<NPC_ControllerBase>();
        foreach (var t in candidates)
        {
            if (t == null) continue;
            if (t.TryGetComponent<Health>(out var h))
            {
                if (!h.IsAlive) continue;
                if (h.currentHealth < h.maxHealth)
                {
                    var ally = t.GetComponent<NPC_ControllerBase>();
                    if (ally != null) damagedAllies.Add(ally);
                }
            }
        }

        if (damagedAllies.Count > 0)
        {
            // Buscar menor % de vida
            float minRatio = damagedAllies.Min(a =>
            {
                var h = a.GetComponent<Health>();
                return (float)h.currentHealth / h.maxHealth;
            });

            // Filtrar empatados
            var lowestAllies = damagedAllies
                .Where(a =>
                {
                    var h = a.GetComponent<Health>();
                    return Mathf.Approximately((float)h.currentHealth / h.maxHealth, minRatio);
                })
                .ToList();

            // Escoger aleatorio entre los de menor vida
            currentHealTarget = lowestAllies[Random.Range(0, lowestAllies.Count)];

            // Fijar estado y animación
            path.Clear();
            RotateTowards(currentHealTarget.transform.position);
            currentState = StateMachine.Engage;
            animator?.SetInteger("State", 1); // animación de curar
        }
        else
        {
            currentHealTarget = null;

            // Si hay player en rango, huir
            if (target != null && Vector2.Distance(transform.position, target.position) < detectionRadius)
            {
                // Entrar en Evade si no está ya
                if (currentState != StateMachine.Evade)
                    currentState = StateMachine.Evade;

                animator?.SetInteger("State", 0);

                // Calcular path solo una vez
                if (!evadePathSet)
                {
                    FleeFromTarget(target.position);
                    evadePathSet = true;
                }
                if(evadePathSet && path.Count <= 0)
                {
                    // No se pudo generar path, quedarse quieto
                    currentState = StateMachine.Idle;
                    nextStateAfterIdle = StateMachine.Patrol;
                    evadePathSet = false;
                }
            }
            
            else
            {
                currentState = StateMachine.Patrol;
                animator?.SetInteger("State", 0);
                Patrol();
                evadePathSet = false;
                print(evadePathSet);
            }
        }

    }

    void FleeFromTarget(Vector3 threatPos)
    {
        // Dirección opuesta al jugador
        Vector3 fleeDir = (transform.position - threatPos).normalized;

        // Nodo destino lejano en esa dirección
        Vector3 fleePos = transform.position + fleeDir * 10f; // ajusta distancia de huida
        Node fleeNode = GetClosestNode(fleePos);

        if (fleeNode != null && currentNode != null)
        {
            path = AStarManager.instance.GeneratePath(currentNode, fleeNode, radiusInTiles);
            // No tocamos currentNode ni path otra vez hasta que termine Evade
        }
        if(path.Count == 0)
        {
            // No se pudo generar path, quedarse quieto
            currentState = StateMachine.Idle;
            nextStateAfterIdle = StateMachine.Patrol;
            evadePathSet = false;
        }
        //else
        //{
        //    evadePathSet = true; // ya tenemos path de huida
        //}
    }


    private void CastHeal(NPC_ControllerBase ally)
    {
        if (spell == null) return;
        if (!ally.TryGetComponent<Health>(out var h)) return;

        // Visual
        if (spell.auraPrefab != null)
        {
            var aura = Instantiate(spell.auraPrefab, ally.transform.position, Quaternion.identity, ally.transform);
            Destroy(aura, spell.auraDuration);
        }

        // Curación
        h.Heal(spell.healAmount);
        Debug.Log($"{name} cura a {ally.name} por {spell.healAmount}");
    }

    private void DetectAllies()
    {
        candidates.Clear();

        // Usamos física en vez de escanear toda la escena
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (var col in hits)
        {
            var npc = col.GetComponent<NPC_ControllerBase>();
            if (npc != null && npc != this && npc.faction == faction)
            {
                candidates.Add(npc.transform);
            }
        }
    }


    private void HandleSummonerBehavior()
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
            currentState = StateMachine.Engage;
            animator?.SetInteger("State", 1);
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

    void HandleRangedBehavior()
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


    public void Attack()
    {
        if (spell == null) return;

        // Reducir cooldown del hechizo
        //spellCooldownTimer -= Time.deltaTime;
        if(target != null)
            RotateTowards(target.position);

        switch (spell.type)
        {
            case SpellType.Projectile:
                // Dirección hacia el objetivo
                Vector2 direction = (target.position - firePoint.position).normalized;

                // Calcular ángulo en grados
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // Instanciar proyectil con rotación hacia el objetivo
                var projectile = Instantiate(spell.prefab, firePoint.position, Quaternion.AngleAxis(angle, Vector3.forward));

                // Darle velocidad
                projectile.GetComponent<Rigidbody2D>().velocity = direction * projectileSpeed;

                Debug.Log($"{name} dispara a {target.name}");
                break;
            case SpellType.Summon:
                CastSummon();
                break;
            case SpellType.Self:
                // Aquí usamos el objetivo fijado en el momento de empezar la animación
                if (currentHealTarget != null)
                    CastHeal(currentHealTarget);
                break;
        }
         
    }

    Transform FindClosestInLayerWithinRadius(Faction myFaction, float radius)
    {
        int targetLayer = (myFaction == Faction.Enemy)
            ? LayerMask.NameToLayer("Player")
            : LayerMask.NameToLayer("Enemy");

        if (targetLayer == -1) return null;

        Transform closest = null;
        float best = Mathf.Infinity;

        foreach (var t in GameObject.FindObjectsOfType<Transform>())
        {
            if (t.gameObject.layer != targetLayer) continue;
            float d = Vector2.Distance(transform.position, t.position);
            if (d <= radius && d < best)
            {
                best = d;
                closest = t;
            }
        }
        return closest;
    }
    // ================= SPELL CAST =================
    void TryCastSpell(Vector3 targetPos)
    {
        if (spell == null || spell.prefab == null) return;

        var spellGO = Instantiate(spell.prefab, transform.position, Quaternion.identity);

        // Si es Projectile o Area, apuntar hacia target
        if (spell.type == SpellType.Projectile || spell.type == SpellType.Area)
        {
            Vector2 dir = (targetPos - transform.position).normalized;
            spellGO.transform.right = dir;

            if (spell.type == SpellType.Projectile && spellGO.TryGetComponent<Rigidbody2D>(out var rb))
                rb.velocity = dir * spell.projectileSpeed;
        }

        // Invocados y buffs se manejan según SpellData (puedes añadir Init si hace falta)
    }

    void CastSummon()
    {
        if (spell.summonPrefab == null) return;

        for (int i = 0; i < spell.summonCount; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            var summon = Instantiate(spell.summonPrefab, spawnPos, Quaternion.identity);

            // Pasar Owner
            var npc = summon.GetComponent<NPC_ControllerBase>();
            if (npc != null)
                npc.Init(transform);

            // Destruir tras duración
            if (spell.summonDuration > 0)
                Destroy(summon, spell.summonDuration);
        }
    }

    // Llamado al inicio de la animación
    public void OnHealCastStart()
    {
        isCastingHeal = true;
    }

    // Llamado justo al final de la animación
    public void OnHealCastEnd()
    {
        isCastingHeal = false;

        // resetear para que en el siguiente ciclo pueda elegir a otro
        currentHealTarget = null;
    }

}
