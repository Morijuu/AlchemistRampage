using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Inventory Slots")]
    [SerializeField] private Image[] slotImages = new Image[3];

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

    private bool uiInitialized = false;

    private void Start()
    {
        InitializeUI();

        if (ammoCountText != null)
            baseAmmoFontSize = ammoCountText.fontSize;

        if (healthText != null)
            baseHealthFontSize = healthText.fontSize;
    }

    private void InitializeUI()
    {
        if (uiInitialized) return;

        CreateSlots();
        CreateAmmoUI();

        uiInitialized = true;
    }

    private void CreateSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            if (slotImages[i] != null) continue;

            GameObject slotGO = new GameObject($"Slot{i + 1}");
            slotGO.transform.SetParent(transform, false);

            RectTransform slotRect = slotGO.AddComponent<RectTransform>();
            slotRect.anchoredPosition = new Vector2(50 + (i * 28), 100);
            slotRect.anchorMin = Vector2.zero;
            slotRect.anchorMax = Vector2.zero;
            slotRect.sizeDelta = new Vector2(20, 20);

            Image slotBg = slotGO.AddComponent<Image>();
            slotBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            slotBg.raycastTarget = false;

            Outline outline = slotGO.AddComponent<Outline>();
            outline.effectColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            outline.effectDistance = new Vector2(1, -1);

            slotImages[i] = slotBg;
        }
    }

    private void CreateAmmoUI()
    {
        if (weaponNameText == null)
        {
            GameObject nameGO = new GameObject("WeaponName");
            nameGO.transform.SetParent(transform, false);
            RectTransform nameRect = nameGO.AddComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(50, 30);
            nameRect.anchorMin = Vector2.zero;
            nameRect.anchorMax = Vector2.zero;
            nameRect.sizeDelta = new Vector2(180, 30);
            weaponNameText = nameGO.AddComponent<TextMeshProUGUI>();
            weaponNameText.fontSize = 16;
            weaponNameText.alignment = TextAlignmentOptions.BottomLeft;
        }

        if (ammoCountText == null)
        {
            GameObject ammoGO = new GameObject("AmmoCount");
            ammoGO.transform.SetParent(transform, false);
            RectTransform ammoRect = ammoGO.AddComponent<RectTransform>();
            ammoRect.anchoredPosition = new Vector2(50, 5);
            ammoRect.anchorMin = Vector2.zero;
            ammoRect.anchorMax = Vector2.zero;
            ammoRect.sizeDelta = new Vector2(180, 25);
            ammoCountText = ammoGO.AddComponent<TextMeshProUGUI>();
            ammoCountText.fontSize = 14;
            ammoCountText.alignment = TextAlignmentOptions.TopLeft;
        }
    }

    private void Update()
    {
        UpdateSlots();
        UpdateAmmo();
        UpdateHealth();
        UpdateStamina();
    }

    private void UpdateSlots()
    {
        BulletInventory inv = BulletInventory.Instance;
        if (inv == null) return;

        for (int i = 0; i < 3; i++)
        {
            if (slotImages[i] == null) continue;

            BulletData data = inv.SlotData[i];
            bool isActive = (i == inv.ActiveSlot);
            bool hasAmmo = data != null && inv.SlotCounts[i] > 0;

            if (data != null && data.bulletSprite != null)
                slotImages[i].sprite = data.bulletSprite;
            else
                slotImages[i].sprite = null;

            // Color logic
            if (!hasAmmo)
            {
                // Neutro: sin balas
                slotImages[i].color = new Color(0.15f, 0.15f, 0.15f, 1f);
            }
            else if (isActive)
            {
                // Amarillo: seleccionado con balas
                slotImages[i].color = new Color(0.8f, 0.8f, 0.2f, 1f);
            }
            else
            {
                // Azul: con balas pero no seleccionado
                slotImages[i].color = new Color(0.2f, 0.4f, 0.8f, 1f);
            }
        }
    }

    private void UpdateAmmo()
    {
        if (weaponNameText == null || ammoCountText == null) return;

        BulletInventory inv = BulletInventory.Instance;
        if (inv == null) return;

        bool hasAmmo = inv.ActiveData != null && inv.ActiveData.bulletType != BulletType.None && inv.ShotCount > 0;

        if (hasAmmo)
        {
            int shots = inv.ShotCount;
            int perPickup = inv.ActiveData.shotsPerPickup;

            string nameColor = GetTypeColor(inv.ActiveData.bulletType);
            weaponNameText.text = $"<color={nameColor}><b>{inv.ActiveData.displayName}</b></color>";

            string ammoColor = GetAmmoColor(shots, perPickup);
            ammoCountText.text = $"<color={ammoColor}><b>{shots}</b></color>";

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
