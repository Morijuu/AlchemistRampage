using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    public float speed = 2f;
    public float detectionRange = 5f;
    public float loseRange = 8f;

    [Header("Wander")]
    public float wanderSpeed = 1f;
    public float wanderChangeInterval = 2f;

    private Transform player;
    private Rigidbody2D rb;

    private enum State { Wandering, Chasing }
    private State state = State.Wandering;

    private Vector2 wanderDirection;
    private float wanderTimer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        PickNewWanderDirection();
    }

    void FixedUpdate()
    {
        if (player == null) { rb.linearVelocity = Vector2.zero; return; }

        float distance = Vector2.Distance(transform.position, player.position);

        // Transiciones de estado
        if (state == State.Wandering && distance <= detectionRange)
            state = State.Chasing;
        else if (state == State.Chasing && distance > loseRange)
            state = State.Wandering;

        Vector2 moveDirection;

        if (state == State.Chasing)
        {
            moveDirection = (player.position - transform.position).normalized;
            rb.linearVelocity = moveDirection * speed;
        }
        else
        {
            Wander();
            moveDirection = rb.linearVelocity.normalized;
        }

        // Rotar hacia la dirección de movimiento si se está moviendo
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }
    }

    void Wander()
    {
        wanderTimer += Time.fixedDeltaTime;
        if (wanderTimer >= wanderChangeInterval)
        {
            PickNewWanderDirection();
            wanderTimer = 0f;
        }
        rb.linearVelocity = wanderDirection * wanderSpeed;
    }

    void PickNewWanderDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        wanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}
