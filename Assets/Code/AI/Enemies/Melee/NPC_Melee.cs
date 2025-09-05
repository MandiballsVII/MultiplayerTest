using UnityEngine;

public class NPC_Melee : NPC_ControllerBase
{
    [Header("Objetivo")]
    public Transform target;
    public float attackRange = 1f;

    [Header("IA")]
    public Faction faction = Faction.Enemy;

    protected override void UpdateState()
    {
        Transform newTarget = FindClosestTarget();

        if (newTarget != target)
        {
            // Si detecta un nuevo objetivo, interrumpimos lo que esta haciendo
            target = newTarget;
            path.Clear(); // <- interrumpe cualquier patrol anterior
        }

        if (target == null)
        {
            currentState = StateMachine.Patrol;
            animator.SetInteger("State", 0);
            Patrol();
        }
        else
        {
            float dist = Vector2.Distance(transform.position, target.position);
            if (dist <= attackRange)
            {
                currentState = StateMachine.Engage;
                animator.SetInteger("State", 1);
                Attack();
            }
            else
            {
                currentState = StateMachine.Engage;
                animator.SetInteger("State", 0);
                ChaseTarget();
            }
        }
    }


    Transform FindClosestTarget()
    {
        Transform closest = null;
        float bestDist = Mathf.Infinity;

        int targetLayer = (faction == Faction.Enemy)
            ? LayerMask.NameToLayer("Player")
            : LayerMask.NameToLayer("Enemy");

        // Recorremos todos los objetos activos en escena
        foreach (var obj in GameObject.FindObjectsOfType<Transform>())
        {
            if (obj.gameObject.layer != targetLayer)
                continue;

            float dist = Vector2.Distance(transform.position, obj.position);
            if (dist <= detectionRadius && dist < bestDist)
            {
                // Mas adelante aqui se puede agregar raycast para linea de vision
                bestDist = dist;
                closest = obj;
            }
        }

        return closest;
    }


    void Patrol()
    {
        if (path.Count == 0)
        {
            Node randomNode = nodeManager.GetRandomNode();
            path = AStarManager.instance.GeneratePath(currentNode, randomNode);
        }
    }

    void ChaseTarget()
    {
        if (path.Count == 0 && target != null)
        {
            Node targetNode = GetClosestNode(target.position);
            path = AStarManager.instance.GeneratePath(currentNode, targetNode);
        }
    }

    void Attack()
    {
        if (target == null) return;

        // Rotar hacia el objetivo antes de atacar
        RotateTowards(target.position);

        // Logica de ataque cuerpo a cuerpo
        Debug.Log($"{name} ataca a {target.name}!");
    }

}