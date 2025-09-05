using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Controller : MonoBehaviour
{
    public int maxHealth = 100;
    public int curHealth;
    public int panicMultiplier = 1;
    public NodeManager nodeManager;

    public Node currentNode;
    public List<Node> path = new List<Node>();

    public enum StateMachine
    {
        Patrol,
        Engage,
        Evade
    }

    public StateMachine currentState;

    public PlayerController player;

    public float speed = 3f;

    private void Start()
    {
        curHealth = maxHealth;

        // Buscar nodo más cercano a la posición inicial
        currentNode = nodeManager.GetClosestNode(transform.position);
    }

    private void Update()
    {
        switch (currentState)
        {
            case StateMachine.Patrol:
                Patrol();
                break;
            case StateMachine.Engage:
                Engage();
                break;
            case StateMachine.Evade:
                Evade();
                break;
        }

        bool playerSeen = Vector2.Distance(transform.position, player.transform.position) < 5.0f;

        if (!playerSeen && currentState != StateMachine.Patrol && curHealth > (maxHealth * 20) / 100)
        {
            currentState = StateMachine.Patrol;
            path.Clear();
        }
        else if (playerSeen && currentState != StateMachine.Engage && curHealth > (maxHealth * 20) / 100)
        {
            currentState = StateMachine.Engage;
            path.Clear();
        }
        else if (currentState != StateMachine.Evade && curHealth <= (maxHealth * 20) / 100)
        {
            panicMultiplier = 2;
            currentState = StateMachine.Evade;
            path.Clear();
        }

        CreatePath();
    }

    void Patrol()
    {
        if (path.Count == 0)
        {
            Node randomNode = nodeManager.GetRandomNode();
            path = AStarManager.instance.GeneratePath(currentNode, randomNode);
        }
    }

    void Engage()
    {
        if (path.Count == 0)
        {
            Node targetNode = nodeManager.GetClosestNode(player.transform.position);
            path = AStarManager.instance.GeneratePath(currentNode, targetNode);
        }
    }

    void Evade()
    {
        if (path.Count == 0)
        {
            Node targetNode = nodeManager.GetFurthestNode(player.transform.position);
            path = AStarManager.instance.GeneratePath(currentNode, targetNode);
        }
    }

    public void CreatePath()
    {
        if (path.Count > 0)
        {
            Node target = path[0];
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(target.transform.position.x, target.transform.position.y, -2),
                (speed * panicMultiplier) * Time.deltaTime
            );

            if (Vector2.Distance(transform.position, target.transform.position) < 0.1f)
            {
                currentNode = target;
                path.RemoveAt(0);
            }
        }
    }
}
