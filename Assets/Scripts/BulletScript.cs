using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    [SerializeField] private bool isEnemyBullet = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Health health = collision.GetComponent<Health>();

        if (health != null)
        {
            // Bala del player → NO daña player
            if (!isEnemyBullet && collision.CompareTag("Player")) return;

            // Bala enemiga → NO daña enemigos
            if (isEnemyBullet && collision.CompareTag("Enemy")) return;

            health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}