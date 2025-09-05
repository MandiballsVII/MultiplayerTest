using System.Collections.Generic;
using UnityEngine;

public abstract class NPC_ControllerBase : MonoBehaviour
{
    [Header("Movimiento")]
    public Node currentNode;
    protected List<Node> path = new List<Node>();
    public float speed = 3f;

    [Header("IA")]
    public float detectionRadius = 5f;
    public NodeManager nodeManager;

    public enum StateMachine { Patrol, Engage, Evade }
    public StateMachine currentState;

    protected Animator animator;

    protected virtual void Start()
    {
        if (nodeManager == null)
            nodeManager = FindObjectOfType<NodeManager>();
        if (currentNode == null)
            currentNode = GetClosestNode(transform.position);
        animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        UpdateState();
        MoveAlongPath();
    }

    protected abstract void UpdateState();

    protected void MoveAlongPath()
    {
        if (path.Count == 0) return;

        Node targetNode = path[0];
        RotateTowards(targetNode.transform.position); // <-- rotacion hacia el nodo
        transform.position = Vector3.MoveTowards(transform.position, targetNode.transform.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetNode.transform.position) < 0.1f)
        {
            currentNode = targetNode;
            path.RemoveAt(0);
        }
    }


    protected Node GetClosestNode(Vector3 world) => nodeManager.GetClosestNode(world);

    protected void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        if (direction.sqrMagnitude < 0.001f) return; // Evitar errores si esta en el mismo punto

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90f);
        // El -90f depende de como este orientado tu sprite. Ajusta segun el "frente" del sprite.
    }

}