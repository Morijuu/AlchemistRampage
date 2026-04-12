using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    public float speed = 2f;
    public float detectionRange = 5f;

    private Transform player;
    private Rigidbody2D rb;

    private bool isChasing = false;

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

        if (isChasing)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    //Si seleccionas el enemigo en la escena ves el rango de detección, esto se puede borrar luego.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}