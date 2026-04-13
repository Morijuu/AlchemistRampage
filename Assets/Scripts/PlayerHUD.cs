using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Ammo")]
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text ammoCountText;

    [Header("Health")]
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Health playerHealth;

    private void Update()
    {
        UpdateAmmo();
        UpdateHealth();
    }

    private void UpdateAmmo()
    {
        BulletInventory inv = BulletInventory.Instance;
        if (inv == null) return;

        if (inv.ActiveData != null && inv.ActiveData.bulletType != BulletType.None)
        {
            weaponNameText.text = inv.ActiveData.displayName;
            ammoCountText.text = inv.ShotCount.ToString();
        }
        else
        {
            weaponNameText.text = "No ammo";
            ammoCountText.text = "0";
        }
    }

    private void UpdateHealth()
    {
        if (playerHealth == null) return;

        float ratio = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        if (healthFill != null) healthFill.fillAmount = ratio;
        if (healthText != null) healthText.text = playerHealth.currentHealth + "/" + playerHealth.maxHealth;
    }
}
