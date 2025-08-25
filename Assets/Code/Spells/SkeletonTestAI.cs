using UnityEngine;

public class SkeletonTestAI : MonoBehaviour
{
    public float speed = 2f;
    public float moveTime = 2f; // tiempo en cada dirección

    private Vector2[] directions = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
    private int currentDirection = 0;
    private float timer;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= moveTime)
        {
            timer = 0f;
            currentDirection = (currentDirection + 1) % directions.Length;
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = directions[currentDirection] * speed;
    }
}
