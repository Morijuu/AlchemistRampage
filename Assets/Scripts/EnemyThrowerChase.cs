using UnityEngine;

public class EnemyThrowerChase : MonoBehaviour
{
    public float speed = 2.5f;
    public float detectionRange = 7f;
    public float loseRange = 10f;
    public float shootRange = 5f;

    public GameObject projectilePrefab;
    public float shootCooldown = 2f;
    public float projectileSpeed = 6f;

    [Header("Wander")]
    public float wanderSpeed = 1f;
    public float wanderChangeInterval = 2f;

    private Transform player;
    private Rigidbody2D rb;

    private enum State { Wandering, Chasing }
    private State state = State.Wandering;

    private Vector2 wanderDirection;
    private float wanderTimer;
    private float shootTimer;

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

        if (state == State.Wandering && distance <= detectionRange)
            state = State.Chasing;
        else if (state == State.Chasing && distance > loseRange)
            state = State.Wandering;

        if (state == State.Chasing)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = distance > shootRange ? direction * speed : Vector2.zero;
        }
        else
        {
            Wander();
        }
    }

    void Update()
    {
        if (player == null || state != State.Chasing) return;

        shootTimer += Time.deltaTime;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= shootRange && shootTimer >= shootCooldown)
        {
            Shoot();
            shootTimer = 0f;
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

    void Shoot()
    {
        if (projectilePrefab == null) return;
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Vector2 direction = (player.position - transform.position).normalized;
        proj.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileSpeed;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loseRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}
