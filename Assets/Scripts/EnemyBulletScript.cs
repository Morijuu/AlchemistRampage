using UnityEngine;

public class EnemyBulletScript : MonoBehaviour
{
    [SerializeField] private int damage = 15;

    private void Start()
    {
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
            health.TakeDamage(damage);

        Destroy(gameObject);
    }
}
