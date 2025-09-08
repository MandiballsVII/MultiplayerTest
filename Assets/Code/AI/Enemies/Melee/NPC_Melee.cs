using UnityEngine;

public class NPC_Melee : NPC_ControllerBase
{
    // Tiempo entre recalculaciones de path (para no recalcular cada frame)
    private float pathUpdateCooldown = 0.25f;
    private float pathUpdateTimer = 0f;

    protected override void UpdateState()
    {
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
        print("Posicion del enemigo: " + transform.position + " -- " + " Punto último conocido: " + lastKnownPosition.Value + " -- " + " Distancia entre los puntos: " + Vector2.Distance(transform.position, lastKnownPosition.Value));
        if (Vector2.Distance(transform.position, lastKnownPosition.Value) < 0.8f)
        {
            print(name + " no encontró al jugador, vuelve a patrullar");
            lastKnownPosition = null;
            path.Clear();

        }
    }

    void Attack()
    {
        if (target == null) return;

        RotateTowards(target.position);
        Debug.Log($"{name} ataca a {target.name}");
    }
}
