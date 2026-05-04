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
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        currentHealth -= damage;

        if (isPlayer)
            ApplyPlayerKnockback(attacker); 
        else
            ApplyKnockback(attacker); 

        if (currentHealth <= 0)
            Die();
    }


    void ApplyKnockback(Transform attacker)
    {
        if (attacker == null) return;

        Vector2 direction = (transform.position - attacker.position).normalized;

        transform.position += (Vector3)(direction * knockbackDistance);
    }

    void ApplyPlayerKnockback(Transform attacker)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null || attacker == null) return;

        Vector2 direction = (transform.position - attacker.position).normalized;

        StartCoroutine(PlayerKnockbackCoroutine(rb, direction));
    }

    IEnumerator PlayerKnockbackCoroutine(Rigidbody2D rb, Vector2 direction)
    {
        isKnocked = true;

        rb.linearVelocity = direction * playerKnockbackForce;

        yield return new WaitForSeconds(playerKnockbackTime);

        isKnocked = false;
    }
        public bool IsKnocked()
    {
        return isKnocked;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
    void Die()
    {
        if (isPlayer)
        {
            Debug.Log("Player died. UIManager.Instance: " + UIManager.Instance);
            if (UIManager.Instance != null)
                UIManager.Instance.ShowGameOver();
            else
                Debug.LogError("UIManager.Instance es null — GameManager no está en la escena o UIManager no tiene Awake ejecutado.");
            Destroy(gameObject);
            return;
        }

        if (dropsPickup)
        {
            if (heartPrefab != null && Random.value <= heartDropChance)
            {
            Instantiate(heartPrefab, transform.position, Quaternion.identity);
            }
            
            if (bulletPickupPrefabs == null || bulletPickupPrefabs.Length == 0)
            {
                Debug.LogWarning($"{gameObject.name}: bulletPickupPrefabs vacío.");
            }
            else if (Random.value <= dropChance)
            {
                // Buscar un prefab válido (no null) en el array
                GameObject prefab = null;
                for (int i = 0; i < bulletPickupPrefabs.Length; i++)
                {
                    int index = Random.Range(0, bulletPickupPrefabs.Length);
                    if (bulletPickupPrefabs[index] != null) { prefab = bulletPickupPrefabs[index]; break; }
                }

                if (prefab != null)
                    Instantiate(prefab, transform.position, Quaternion.identity);
                else
                    Debug.LogWarning($"{gameObject.name}: todos los slots de bulletPickupPrefabs son null.");
            }
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
{
    if (!isPlayer) return;

    if (other.CompareTag("Corazon"))
    {
        if (currentHealth < maxHealth)
        {
            Heal(heartHealAmount);
            Destroy(other.gameObject);
        }
    }


}
}
