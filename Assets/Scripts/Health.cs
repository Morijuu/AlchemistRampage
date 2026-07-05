using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public int heartHealAmount = 20;
    
    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 0.5f;
    [SerializeField] private float playerKnockbackForce = 8f;
    [SerializeField] private float playerKnockbackTime = 0.2f;
    private bool isKnocked = false;

    [Header("Hit Flash")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float flashDuration = 0.12f;
    private Color originalColor;
    private Coroutine flashCoroutine;

    [Header("Player")]
    [SerializeField] private bool isPlayer = false;

    [Header("Drops")]
    [SerializeField] private GameObject[] bulletPickupPrefabs;
    [SerializeField] private bool dropsPickup = true;
    [SerializeField] [Range(0f, 1f)] private float dropChance = 1f;
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] [Range(0f, 1f)] private float heartDropChance = 0.3f;

    void Awake()
    {
        currentHealth = maxHealth;

        if (sr != null)
            originalColor = sr.color;
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        currentHealth -= damage;

        // Track damage taken
        if (isPlayer)
            GameStats.Instance?.AddDamageTaken(damage);

        // AUDIO
        if (isPlayer)
        {
            AudioManager.Instance?.PlayPlayerHit();
            CameraFlash.Instance?.TriggerFlash();
            CameraFlash.Instance?.SetLowHealth(currentHealth < 30);
        }
        else
        {
            if (CompareTag("Enemy"))
                AudioManager.Instance?.PlayZombieHit();
        }

        // LÓGICA ORIGINAL
        if (isPlayer)
            ApplyPlayerKnockback(attacker);
        else
            ApplyKnockback(attacker);

        if (sr != null && !isPlayer)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine =
                StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator DamageFlash()
    {
        sr.color = new Color(1f, 0.2f, 0.2f, 0.7f);

        yield return new WaitForSeconds(flashDuration);

        sr.color = originalColor;
    }

    void ApplyKnockback(Transform attacker)
    {
        if (attacker == null) return;

        Vector2 direction =
            (transform.position - attacker.position)
            .normalized;

        transform.position +=
            (Vector3)(direction * knockbackDistance);
    }

    void ApplyPlayerKnockback(Transform attacker)
    {
        Rigidbody2D rb =
            GetComponent<Rigidbody2D>();

        if (
            rb == null ||
            attacker == null
        )
            return;

        Vector2 direction =
            (transform.position - attacker.position)
            .normalized;

        StartCoroutine(
            PlayerKnockbackCoroutine(
                rb,
                direction
            )
        );
    }

    IEnumerator PlayerKnockbackCoroutine(
        Rigidbody2D rb,
        Vector2 direction
    )
    {
        isKnocked = true;

        float timer = 0f;

        while (timer < playerKnockbackTime)
        {
            rb.linearVelocity =
                direction *
                playerKnockbackForce;

            timer += Time.deltaTime;

            yield return null;
        }

        isKnocked = false;
    }

    public bool IsKnocked()
    {
        return isKnocked;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        GameStats.Instance?.AddDamageHealed(amount);

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0,
                maxHealth
            );

        // AUDIO CURACIÓN
        AudioManager.Instance?.PlayHeal();
        if (isPlayer)
            CameraFlash.Instance?.SetLowHealth(currentHealth < 30);
    }

    void Die()
    {
        // PLAYER
// PLAYER
if (isPlayer)
{
    AudioManager.Instance?.PlayPlayerDeath();

    Debug.Log(
        "Player died. UIManager.Instance: "
        + UIManager.Instance
    );

    if (UIManager.Instance != null)
        UIManager.Instance.ShowGameOver();

    else
        Debug.LogError(
            "UIManager.Instance es null"
        );

    Destroy(gameObject);

    return;
}

        // ZOMBIE
        if (CompareTag("Enemy"))
        {
            AudioManager.Instance?.PlayZombieDeath();
            GameStats.Instance?.AddZombieKilled();
        }

        // CAJA
        if (CompareTag("Caja"))
        {
            AudioManager.Instance?.PlayCrateBreak();
        }

        // DROPS ORIGINALES
        if (dropsPickup)
        {
            if (
                heartPrefab != null &&
                Random.value <= heartDropChance
            )
            {
                Instantiate(
                    heartPrefab,
                    transform.position,
                    Quaternion.identity
                );
            }

            if (
                bulletPickupPrefabs == null ||
                bulletPickupPrefabs.Length == 0
            )
            {
                Debug.LogWarning(
                    $"{gameObject.name}: bulletPickupPrefabs vacío."
                );
            }
            else if (
                Random.value <= dropChance
            )
            {
                GameObject prefab = null;

                for (
                    int i = 0;
                    i < bulletPickupPrefabs.Length;
                    i++
                )
                {
                    int index =
                        Random.Range(
                            0,
                            bulletPickupPrefabs.Length
                        );

                    if (
                        bulletPickupPrefabs[index]
                        != null
                    )
                    {
                        prefab =
                            bulletPickupPrefabs[index];

                        break;
                    }
                }

                if (prefab != null)
                {
                    Instantiate(
                        prefab,
                        transform.position,
                        Quaternion.identity
                    );
                }
                else
                {
                    Debug.LogWarning(
                        $"{gameObject.name}: todos los slots son null."
                    );
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        Debug.Log(
            $"OnTriggerEnter2D: {other.gameObject.name} tag={other.tag}, isPlayer={isPlayer}"
        );

        if (!isPlayer)
            return;

        if (other.CompareTag("Corazon"))
        {
            Debug.Log(
                $"Curando {heartHealAmount}. Vida actual: {currentHealth}/{maxHealth}"
            );

            if (currentHealth < maxHealth)
            {
                Heal(heartHealAmount);

                Destroy(
                    other.gameObject
                );
            }
        }
    }
}