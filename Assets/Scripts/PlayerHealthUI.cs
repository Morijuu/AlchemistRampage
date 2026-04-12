using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    public Health playerHealth;
    public Image fill;
    public TextMeshProUGUI healthText;

    void Update()
    {
        if (playerHealth == null)
        {
            Destroy(gameObject);
            return;
        }

        float current = (float)playerHealth.currentHealth / playerHealth.maxHealth;

        fill.fillAmount = current;
        healthText.text = playerHealth.currentHealth + "/" + playerHealth.maxHealth;

        if (playerHealth.currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}