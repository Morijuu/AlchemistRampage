using UnityEngine;

public class EnemyThrowerChase : MonoBehaviour
{
    public float speed = 2.5f;
    public float detectionRange = 7f;
    public float loseRange = 10f;
    public float shootRange = 5f;

    // 🔽 NUEVO: Capa para detectar las paredes
    [Header("Vision")]
    public LayerMask wallLayer;

    public GameObject projectilePrefab;
    public float shootCooldown = 2f;
    public float projectileSpeed = 6f;

    [Header("Wander")]
    public float wanderSpeed = 1f;
    public float wanderChangeInterval = 2f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;

    private enum State { Wandering, Chasing }
    private State state = State.Wandering;

    private Vector2 wanderDirection;
    private float wanderTimer;
    private float shootTimer;
    private float attackAnimTimer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        PickNewWanderDirection();
    }

    void FixedUpdate()
    {
        if (player == null) { rb.linearVelocity = Vector2.zero; return; }

        float distance = Vector2.Distance(transform.position, player.position);

        // --- 🔽 NUEVO: Comprobar línea de visión ---
        bool hasLineOfSight = false;
        if (distance <= loseRange)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            RaycastHit2D hitWall = Physics2D.Raycast(transform.position, directionToPlayer, distance, wallLayer);
            hasLineOfSight = (hitWall.collider == null);
        }

        // --- 🔽 MODIFICADO: Pasa a Wandering si te alejas O si te escondes detrás de un muro ---
        if (state == State.Wandering && distance <= detectionRange && hasLineOfSight)
            state = State.Chasing;
        else if (state == State.Chasing && (distance > loseRange || !hasLineOfSight))
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

        if (attackAnimTimer > 0) attackAnimTimer -= Time.fixedDeltaTime;
        
        if (animator != null)
        {
            animator.SetBool("isWalking", rb.linearVelocity.magnitude > 0f);
            animator.SetBool("isAttacking", attackAnimTimer > 0);
        }

        Vector2 lookDir = state == State.Chasing
            ? (Vector2)(player.position - transform.position)
            : wanderDirection;

        if (lookDir.sqrMagnitude > 0.01f)
            rb.rotation = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
    }

    void Update()
    {
        if (player == null || state != State.Chasing) return;

        shootTimer += Time.deltaTime;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= shootRange && shootTimer >= shootCooldown)
        {
            // --- 🔽 NUEVO: Asegurarnos de que no hay pared antes de disparar ---
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            RaycastHit2D hitWall = Physics2D.Raycast(transform.position, directionToPlayer, distance, wallLayer);

            // Si el rayo NO choca con la pared, entonces disparamos
            if (hitWall.collider == null)
            {
                Shoot();
                attackAnimTimer = shootCooldown;
                shootTimer = 0f;
            }
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