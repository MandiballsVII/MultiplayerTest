using UnityEngine;

public class NPC_Ranged : NPC_ControllerBase
{
    public GameObject projectilePrefab;
    public Transform firePoint; // punto desde donde dispara
    public float fireCooldown = 1.5f;
    private float fireTimer = 0f;

    public float safeDistance = 2f; // si el player entra dentro, el enemigo retrocede

    private float pathUpdateCooldown = 0.25f;
    private float pathUpdateTimer = 0f;

    // Idle
    private float idleTimer = 0f;
    public float idleDuration = 2f;
    private StateMachine nextStateAfterIdle;

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

        if (target != null)
        {
            float dist = Vector2.Distance(transform.position, target.position);

            if (dist <= attackRange && dist > safeDistance)
            {
                currentState = StateMachine.Engage;
                animator?.SetInteger("State", 1);
                Attack();
            }
            else if (dist <= safeDistance)
            {
                currentState = StateMachine.Evade;
                animator?.SetInteger("State", 0);
                RetreatFromTarget(target.position);
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
            currentState = StateMachine.Idle;
            nextStateAfterIdle = StateMachine.Patrol;
        }
    }

    void ChaseTarget(Vector3 destination)
    {
        if (target == null) return;

        if (HasLineOfSight(target))
        {
            RotateTowards(target.position);
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            return;
        }

        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer <= 0f)
        {
            Node tnode = GetClosestNode(destination);
            if (tnode != null && currentNode != null)
                path = AStarManager.instance.GeneratePath(currentNode, tnode);

            pathUpdateTimer = pathUpdateCooldown;
        }
    }

    void RetreatFromTarget(Vector3 targetPos)
    {
        RotateTowards(targetPos);

        Vector3 dirAway = (transform.position - targetPos).normalized;
        transform.position += dirAway * speed * Time.deltaTime;
    }

    void SearchLastKnown()
    {
        if (!lastKnownPosition.HasValue) return;

        currentNode = GetClosestNode(transform.position);

        if (path.Count == 0)
        {
            Node goal = GetClosestNode(lastKnownPosition.Value);
            if (goal != null && currentNode != null)
                path = AStarManager.instance.GeneratePath(currentNode, goal);
        }

        if (Vector2.Distance(transform.position, lastKnownPosition.Value) < 0.8f)
        {
            lastKnownPosition = null;
            path.Clear();
            currentState = StateMachine.Idle;
            nextStateAfterIdle = StateMachine.Patrol;
        }
    }

    void Attack()
    {
        if (target == null) return;

        RotateTowards(target.position);

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            // aquí puedes añadir lógica al proyectil: velocidad, daño, etc.
            fireTimer = fireCooldown;
            Debug.Log($"{name} dispara a {target.name}");
        }
    }
}
