using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    public Health health;
    public Image fill;
    public TextMeshProUGUI healthText;

    void Update()
    {
        if (health == null) return;

        float current = (float)health.currentHealth / health.maxHealth;

        fill.fillAmount = current;

        healthText.text = health.currentHealth + "/" + health.maxHealth;
    }
}