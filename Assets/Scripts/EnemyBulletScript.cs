using UnityEngine;

public class EnemyBulletScript : MonoBehaviour
{
    [SerializeField] private int damage = 15;

    [Header("Explosion")]
    public Sprite explosionSprite;
    public float explosionTime = 1f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D col;

    private bool exploded = false;

    private void Start()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();

        col = GetComponent<Collider2D>();

        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (exploded) return;

        if (
            other.CompareTag(
                "Enemy"
            )
        ) return;

        if (
            other.CompareTag(
                "Pickup"
            )
        ) return;

        Health health =
            other.GetComponent<Health>();

        if (health != null)
        {
            health.TakeDamage(
                damage,
                transform
            );
        }

        Explode();
    }

    private void Explode()
    {
        exploded = true;

        if (
            spriteRenderer != null &&
            explosionSprite != null
        )
        {
            spriteRenderer.sprite =
                explosionSprite;
        }

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }

        if (col != null)
        {
            col.enabled = false;
        }

        Destroy(
            gameObject,
            explosionTime
        );
    }
}