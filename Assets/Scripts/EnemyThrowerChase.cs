using UnityEngine;

public class EnemyThrowerChase : MonoBehaviour
{
    public float speed = 2.5f;
    public float detectionRange = 7f;
    public float shootRange = 5f;

    public GameObject projectilePrefab;
    public float shootCooldown = 2f;
    public float projectileSpeed = 6f;

    private Transform player;
    private Rigidbody2D rb;

    private bool isChasing = false;
    private float shootTimer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (!isChasing && distance <= detectionRange)
        {
            isChasing = true;
        }

        if (!isChasing)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;

        if (distance > shootRange)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Update()
    {
        if (player == null) return;

        if (!isChasing) return;

        shootTimer += Time.deltaTime;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= shootRange && shootTimer >= shootCooldown)
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    void Shoot()
    {
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        Vector2 direction = (player.position - transform.position).normalized;

        Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
        projRb.linearVelocity = direction * projectileSpeed;
    }

    //Si seleccionas el enemigo en la escena ves el rango de detección, esto se puede borrar luego.
    void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de disparo
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}