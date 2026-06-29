using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class XPSystem : MonoBehaviour
{
    public static XPSystem Instance { get; private set; }

    // Static modifiers read by PlayerScript and BulletInventory
    public static float DamageMultiplier   = 1f;
    public static float FireRateMultiplier = 1f;
    public static int   FusionBonusShots   = 0;

    [Header("HUD References")]
    [SerializeField] private Image           xpFill;
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private TextMeshProUGUI xpLabel;

    private int currentXP;
    private int currentLevel;
    private int xpToNext = 100;

    private GameObject levelUpCanvas;

    struct Upgrade
    {
        public string         name;
        public string         desc;
        public System.Action  apply;
    }

    void Awake()
    {
        Instance           = this;
        DamageMultiplier   = 1f;
        FireRateMultiplier = 1f;
        FusionBonusShots   = 0;
    }

    void Start() => RefreshHUD();

    public void AddXP(int amount)
    {
        // Ignore while level-up panel is open
        if (levelUpCanvas != null) return;

        currentXP += amount;
        RefreshHUD();

        if (currentXP >= xpToNext)
        {
            currentXP -= xpToNext;
            currentLevel++;
            xpToNext = Mathf.RoundToInt(xpToNext * 1.4f);
            ShowLevelUp();
        }
    }

    void RefreshHUD()
    {
        if (xpFill    != null) xpFill.fillAmount = xpToNext > 0 ? (float)currentXP / xpToNext : 1f;
        if (levelLabel != null) levelLabel.text  = $"Nv. {currentLevel}";
        if (xpLabel   != null) xpLabel.text      = $"{currentXP}/{xpToNext}";
    }

    // ─── LEVEL-UP PANEL ────────────────────────────────────────────

    void ShowLevelUp()
    {
        Time.timeScale = 0f;

        var pool = BuildUpgradePool();
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        var pick = pool.GetRange(0, Mathf.Min(3, pool.Count));

        if (levelUpCanvas != null) Destroy(levelUpCanvas);
        levelUpCanvas = MakeCanvas("LevelUpCanvas", 400);

        // Backdrop
        var bd = new GameObject("Backdrop"); bd.transform.SetParent(levelUpCanvas.transform, false);
        var bdRt = bd.AddComponent<RectTransform>();
        bdRt.anchorMin = Vector2.zero; bdRt.anchorMax = Vector2.one; bdRt.sizeDelta = Vector2.zero;
        bd.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

        // Card
        var card = new GameObject("Card"); card.transform.SetParent(levelUpCanvas.transform, false);
        var cardRt = card.AddComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f); cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(700f, 400f); cardRt.anchoredPosition = Vector2.zero;
        card.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.1f, 0.95f);

        Lbl(card.transform, $"¡NIVEL {currentLevel}!",  new Vector2(0f, 155f), new Vector2(620f, 60f), 48f, true,  Color.white);
        Lbl(card.transform, "Elige una mejora:",         new Vector2(0f, 108f), new Vector2(500f, 28f), 18f, false, new Color(0.72f, 0.72f, 0.72f));

        var div = new GameObject("Div"); div.transform.SetParent(card.transform, false);
        var dRt = div.AddComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0.5f, 0.5f); dRt.anchorMax = new Vector2(0.5f, 0.5f);
        dRt.pivot = new Vector2(0.5f, 0.5f);
        dRt.anchoredPosition = new Vector2(0f, 82f); dRt.sizeDelta = new Vector2(620f, 1f);
        div.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.18f);

        float[] yPos = { 35f, -48f, -131f };
        for (int i = 0; i < pick.Count; i++)
            UpgradeBtn(card.transform, pick[i], yPos[i]);
    }

    void UpgradeBtn(Transform parent, Upgrade u, float y)
    {
        var go = new GameObject("UpgradeBtn");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, y); rt.sizeDelta = new Vector2(620f, 64f);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.1f, 0.25f, 0.52f, 0.9f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(0.72f, 0.88f, 1f);
        cb.pressedColor     = new Color(0.52f, 0.68f, 0.88f);
        cb.fadeDuration     = 0.06f;
        btn.colors = cb;

        Upgrade cap = u;
        btn.onClick.AddListener(() => { cap.apply(); CloseLevelUp(); });

        Lbl(go.transform, cap.name, new Vector2(-140f, 0f), new Vector2(240f, 42f), 22f, true,  Color.white);
        Lbl(go.transform, cap.desc, new Vector2(90f,   0f), new Vector2(320f, 36f), 15f, false, new Color(0.78f, 0.9f, 1f));
    }

    void CloseLevelUp()
    {
        if (levelUpCanvas != null) { Destroy(levelUpCanvas); levelUpCanvas = null; }
        Time.timeScale = 1f;
        RefreshHUD();
        // Handle accumulated XP that might already pass next threshold
        if (currentXP >= xpToNext) AddXP(0);
    }

    // ─── UPGRADE POOL ──────────────────────────────────────────────

    List<Upgrade> BuildUpgradePool() => new List<Upgrade>
    {
        new Upgrade { name = "+Daño",       desc = "Balas hacen 30% más daño",   apply = () => DamageMultiplier += 0.3f },
        new Upgrade { name = "+Cadencia",   desc = "Dispara 25% más rápido",     apply = () => FireRateMultiplier += 0.25f },
        new Upgrade { name = "+Stamina",    desc = "+30 stamina máxima",         apply = ApplyStamina },
        new Upgrade { name = "+Bala extra", desc = "Fusionar da +1 bala extra",  apply = () => FusionBonusShots++ },
        new Upgrade { name = "+Velocidad",  desc = "Muévete más rápido",         apply = ApplySpeed },
        new Upgrade { name = "+Vida máx.",  desc = "+25 de vida máxima",         apply = ApplyHealth },
    };

    static void ApplyStamina()
    {
        var ps = FindFirstObjectByType<PlayerScript>();
        if (ps == null) return;
        ps.maxStamina += 30f;
        ps.stamina     = Mathf.Min(ps.stamina + 30f, ps.maxStamina);
    }

    static void ApplySpeed()
    {
        var ps = FindFirstObjectByType<PlayerScript>();
        if (ps == null) return;
        ps.speed    += 0.8f;
        ps.runspeed += 1.2f;
    }

    static void ApplyHealth()
    {
        var ps = FindFirstObjectByType<PlayerScript>();
        var h  = ps != null ? ps.GetComponent<Health>() : null;
        if (h == null) return;
        h.maxHealth += 25;
        h.Heal(25);
    }

    // ─── UI HELPERS ────────────────────────────────────────────────

    static GameObject MakeCanvas(string name, int order)
    {
        var go = new GameObject(name);
        var c  = go.AddComponent<Canvas>();
        c.renderMode  = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = order;
        var cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode       = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920f, 1080f);
        cs.matchWidthOrHeight  = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    static void Lbl(Transform parent, string text, Vector2 pos, Vector2 size, float fs, bool bold, Color col)
    {
        var go = new GameObject("Lbl");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text        = text; t.fontSize = fs;
        t.fontStyle   = bold ? FontStyles.Bold : FontStyles.Normal;
        t.alignment   = TextAlignmentOptions.Center; t.color = col;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode     = TextOverflowModes.Ellipsis;
    }
}
