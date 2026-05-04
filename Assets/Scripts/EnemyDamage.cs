using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 20;
    public float cooldown = 1f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && timer >= cooldown)
        {
            Health health = collision.gameObject.GetComponent<Health>();

            if (health != null)
            {
                health.TakeDamage(damage, transform);

                // 🔽 AÑADIDO
                EnemyChase chase = GetComponent<EnemyChase>();
                if (chase != null)
                    chase.PauseAfterAttack();

                timer = 0f;
            }
        }
    }
}