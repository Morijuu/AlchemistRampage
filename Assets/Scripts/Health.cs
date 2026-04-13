using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Player")]
    [SerializeField] private bool isPlayer = false;

    [Header("Drops")]
    [SerializeField] private GameObject[] bulletPickupPrefabs;
    [SerializeField] private bool dropsPickup = true;
    [SerializeField] [Range(0f, 1f)] private float dropChance = 1f;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
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
}
