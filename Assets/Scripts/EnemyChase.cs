using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    public float speed = 2f;
    public float detectionRange = 5f;
    public float loseRange = 8f;

    // 🔽 NUEVO: Capa para detectar las paredes
    [Header("Vision")]
    public LayerMask wallLayer; 

    [Header("Wander")]
    public float wanderSpeed = 1f;
    public float wanderChangeInterval = 2f;

    private Transform player;
    private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    private enum State { Wandering, Chasing }
    private State state = State.Wandering;

    private Vector2 wanderDirection;
    private float wanderTimer;

    [SerializeField] private float attackPauseTime = 1f;
    private float attackPauseTimer = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        PickNewWanderDirection();
    }

    void FixedUpdate()
    {
        if (attackPauseTimer > 0)
        {
            attackPauseTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = Vector2.zero;
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.SetBool("isAttacking", true);
                animator.SetBool("isWalking", false);
            }
            return;
        }

        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetBool("isAttacking", false);

        if (player == null) { rb.linearVelocity = Vector2.zero; return; }

        float distance = Vector2.Distance(transform.position, player.position);

        // --- 🔽 NUEVO: Comprobar línea de visión (Raycast) ---
        bool hasLineOfSight = false;
        
        // Solo lanzamos el rayo si el jugador está dentro del rango máximo posible para no gastar recursos a lo tonto
        if (distance <= loseRange)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            // Lanzamos el rayo. Si no choca con nada de la capa "wallLayer", es que nos ve.
            RaycastHit2D hitWall = Physics2D.Raycast(transform.position, directionToPlayer, distance, wallLayer);
            hasLineOfSight = (hitWall.collider == null);
        }

        // --- 🔽 MODIFICADO: Ahora además de la distancia, exigimos que haya visión ---
        if (state == State.Wandering && distance <= detectionRange && hasLineOfSight)
        {
            state = State.Chasing;
        }
        else if (state == State.Chasing)
        {
            // Deja de perseguir si te alejas mucho o si rompes la línea de visión (te escondes detrás de una pared)
            if (distance > loseRange || !hasLineOfSight)
            {
                state = State.Wandering;
            }
        }

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

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }

        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetBool("isWalking", rb.linearVelocity.magnitude > 0f);
    }

    public void PauseAfterAttack()
    {
        attackPauseTimer = attackPauseTime;
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