using System.Collections;
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

    [SerializeField] private Image staminaFill;
    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private PlayerScript playerMovement;

    [Header("Stamina Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color agotadoColor = Color.red;

    private int lastShotCount = -1;
    private Coroutine pulseCoroutine;
    private float baseAmmoFontSize;

    private int lastHealth = -1;
    private Coroutine healthPulseCoroutine;
    private float baseHealthFontSize;

    private void Start()
    {
        if (ammoCountText != null)
            baseAmmoFontSize = ammoCountText.fontSize;

        if (healthText != null)
            baseHealthFontSize = healthText.fontSize;
    }

    private void Update()
    {
        UpdateAmmo();
        UpdateHealth();
        UpdateStamina();
    }

    private void UpdateAmmo()
    {
        BulletInventory inv = BulletInventory.Instance;
        if (inv == null) return;

        bool hasAmmo = inv.ActiveData != null && inv.ActiveData.bulletType != BulletType.None && inv.ShotCount > 0;

        if (hasAmmo)
        {
            int shots = inv.ShotCount;
            int perPickup = inv.ActiveData.shotsPerPickup;

            // Nombre con color según tipo
            string nameColor = GetTypeColor(inv.ActiveData.bulletType);
            weaponNameText.text = $"<color={nameColor}><b>{inv.ActiveData.displayName}</b></color>";

            // Color del contador según % restante
            string ammoColor = GetAmmoColor(shots, perPickup);
            ammoCountText.text = $"<color={ammoColor}><b>{shots}</b></color>";

            // Pulso al gastar bala
            if (shots != lastShotCount && lastShotCount != -1)
            {
                if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
                pulseCoroutine = StartCoroutine(PulseAmmo());
            }

            lastShotCount = shots;
        }
        else
        {
            weaponNameText.text = "<color=#888888><i>Sin munición</i></color>";
            ammoCountText.text = "<color=#FF3333><b>—</b></color>";
            lastShotCount = 0;
        }
    }

    private IEnumerator PulseAmmo()
    {
        float elapsed = 0f;
        float duration = 0.12f;
        float peakSize = baseAmmoFontSize * 1.35f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            ammoCountText.fontSize = Mathf.Lerp(peakSize, baseAmmoFontSize, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        ammoCountText.fontSize = baseAmmoFontSize;
    }

    private string GetAmmoColor(int shots, int perPickup)
    {
        if (shots <= 0)          return "#FF3333";
        float ratio = (float)shots / Mathf.Max(perPickup, 1);
        if (ratio > 0.6f)        return "#66FF66";
        if (ratio > 0.25f)       return "#FFB800";
        return                          "#FF4422";
    }

    private string GetTypeColor(BulletType type)
    {
        return type switch
        {
            BulletType.Regular   => "#DDDDDD",
            BulletType.Heavy     => "#FF8800",
            BulletType.Bouncy    => "#00DDFF",
            BulletType.Area      => "#88FF44",
            BulletType.Frag      => "#FFEE00",
            BulletType.Target    => "#CC88FF",
            BulletType.Chain     => "#44AAFF",
            BulletType.Piercing  => "#FF4444",
            _                    => "#FFFFFF",
        };
    }

    private void UpdateHealth()
    {
        if (playerHealth == null) return;

        int hp = playerHealth.currentHealth;
        float ratio = (float)hp / playerHealth.maxHealth;

        if (healthFill != null) healthFill.fillAmount = ratio;

        if (healthText != null)
        {
            string hpColor = GetHealthColor(ratio);
            healthText.text = $"<color={hpColor}><b>{hp}</b></color><size=70%><color=#AAAAAA>/{playerHealth.maxHealth}</color></size>";

            if (hp != lastHealth && lastHealth != -1)
            {
                bool healed = hp > lastHealth;
                if (healthPulseCoroutine != null) StopCoroutine(healthPulseCoroutine);
                healthPulseCoroutine = StartCoroutine(PulseHealth(healed));
            }

            lastHealth = hp;
        }
    }

    private IEnumerator PulseHealth(bool healed)
    {
        float elapsed = 0f;
        float duration = 0.15f;
        float peakSize = baseHealthFontSize * (healed ? 1.4f : 1.3f);
        Color flashColor = healed ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.2f, 0.2f);
        Color originalColor = healthText.color;

        healthText.color = flashColor;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            healthText.fontSize = Mathf.Lerp(peakSize, baseHealthFontSize, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        healthText.fontSize = baseHealthFontSize;
        healthText.color = originalColor;
    }

    private string GetHealthColor(float ratio)
    {
        if (ratio > 0.6f) return "#66FF66";
        if (ratio > 0.25f) return "#FFB800";
        return "#FF3333";
    }

    private void UpdateStamina()
    {
        if (playerMovement == null) return;

        float ratio = playerMovement.stamina / playerMovement.maxStamina;

        if (staminaFill != null)
            staminaFill.fillAmount = ratio;

        if (staminaText != null)
            staminaText.text = Mathf.RoundToInt(playerMovement.stamina) + "/" + playerMovement.maxStamina;

        if (staminaFill != null)
        {
            if (playerMovement.agotado)
                staminaFill.color = agotadoColor;
            else
                staminaFill.color = normalColor;
        }
    }
}
