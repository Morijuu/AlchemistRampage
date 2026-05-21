using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    private bool isPaused   = false;
    private bool isWin      = false;
    private bool isGameOver = false;

    private GameObject winCanvas;
    private GameObject gameOverCanvas;
    private GameObject optionsCanvas;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        pausePanel.SetActive(false);
        if (gameOverPanel != null) HidePanel(gameOverPanel);
        if (winPanel      != null) HidePanel(winPanel);

        BuildWinCanvas();
        BuildGameOverCanvas();
        BuildOptionsCanvas();
        StylePausePanel();

        AudioManager.Instance?.PlayGameMusic();
    }

    private void HidePanel(GameObject panel)
    {
        panel.SetActive(false);
        foreach (Transform child in panel.transform)
            child.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsGameOverShowing())
        {
            if (optionsCanvas != null && optionsCanvas.activeSelf)
                HideOptions();
            else if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    // ─── STATE ─────────────────────────────────────────────────────────────
    public bool AnyMenuOpen() => isPaused || isWin || isGameOver;
    private bool IsGameOverShowing() => isWin || isGameOver;

    // ─── PAUSE ─────────────────────────────────────────────────────────────
    public void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        AudioManager.Instance?.PauseMusic();
        isPaused = true;
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        HideOptions();
        Time.timeScale = 1f;
        AudioManager.Instance?.ResumeMusic();
        isPaused = false;
    }

    public void ShowGameOver()
    {
        isGameOver = true;
        if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
        AudioManager.Instance?.PlayLose();
        Time.timeScale = 0f;
    }

    public void ShowWin()
    {
        isWin = true;
        if (winCanvas != null) winCanvas.SetActive(true);
        AudioManager.Instance?.PlayWin();
        Time.timeScale = 0f;
    }

    // ─── NAVIGATION ────────────────────────────────────────────────────────
    public void Retry()
    {
        Time.timeScale = 1f;
        AudioManager.Instance?.PlayGameMusic();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("InicioScene");
    }

    public void BonusLevel()
    {
        AudioManager.Instance?.PlayGameMusic();
        SceneManager.LoadScene("Level2");
    }

    public void Quit() { Time.timeScale = 1f; Application.Quit(); }

    // ─── OPTIONS ───────────────────────────────────────────────────────────
    public void ShowOptions()
    {
        if (optionsCanvas == null) return;
        SyncSliders();
        optionsCanvas.SetActive(true);
    }

    public void HideOptions()
    {
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
    }

    private void SyncSliders()
    {
        if (AudioManager.Instance == null || optionsCanvas == null) return;
        Slider[] sliders = optionsCanvas.GetComponentsInChildren<Slider>(true);
        if (sliders.Length >= 2)
        {
            sliders[0].SetValueWithoutNotify(AudioManager.Instance.musicVolume);
            sliders[1].SetValueWithoutNotify(AudioManager.Instance.sfxVolume);
        }
    }

    // ─── WIN CANVAS ────────────────────────────────────────────────────────
    private void BuildWinCanvas()
    {
        bool isLevel2 = SceneManager.GetActiveScene().name == "Level2";

        winCanvas = MakeOverlay("WinOverlay", 250);
        MakeFullRect(winCanvas.transform, new Color(0f, 0f, 0f, 0.82f));

        GameObject card = MakeCard(winCanvas.transform, new Vector2(580f, 380f));
        MakeTitle(card.transform,    "VICTORIA",               new Vector2(0f, 95f),  new Vector2(540f, 80f));
        MakeSubtitle(card.transform, "Has completado el nivel",new Vector2(0f, 38f),  new Vector2(500f, 32f));
        MakeDivider(card.transform, new Vector2(0f, 5f));

        if (isLevel2)
            MakeBtn(card.transform, "JUGAR DE NUEVO", new Vector2(0f, -62f),  new Vector2(300f, 52f), Retry);
        else
            MakeBtn(card.transform, "BONUS LEVEL",    new Vector2(0f, -62f),  new Vector2(300f, 52f), BonusLevel);

        MakeBtn(card.transform, "MENÚ PRINCIPAL", new Vector2(0f, -128f), new Vector2(300f, 52f), GoToMenu);
        winCanvas.SetActive(false);
    }

    // ─── GAMEOVER CANVAS ───────────────────────────────────────────────────
    private void BuildGameOverCanvas()
    {
        gameOverCanvas = MakeOverlay("GameOverOverlay", 250);
        MakeFullRect(gameOverCanvas.transform, new Color(0f, 0f, 0f, 0.82f));

        GameObject card = MakeCard(gameOverCanvas.transform, new Vector2(580f, 380f));
        MakeTitle(card.transform,    "HAS MUERTO",                    new Vector2(0f, 95f), new Vector2(540f, 80f));
        MakeSubtitle(card.transform, "No lo has conseguido esta vez", new Vector2(0f, 38f), new Vector2(500f, 32f));
        MakeDivider(card.transform, new Vector2(0f, 5f));
        MakeBtn(card.transform, "REINTENTAR",     new Vector2(0f, -62f),  new Vector2(300f, 52f), Retry);
        MakeBtn(card.transform, "MENÚ PRINCIPAL", new Vector2(0f, -128f), new Vector2(300f, 52f), GoToMenu);
        gameOverCanvas.SetActive(false);
    }

    // ─── OPTIONS CANVAS ────────────────────────────────────────────────────
    private void BuildOptionsCanvas()
    {
        optionsCanvas = MakeOverlay("OptionsOverlay", 300);

        // Full-screen background: copy pause panel's image so it looks like the pause screen
        GameObject bgGO = new GameObject("PauseBg");
        bgGO.transform.SetParent(optionsCanvas.transform, false);
        RectTransform bgRt = bgGO.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgGO.AddComponent<Image>();

        Image pauseImg = pausePanel != null ? pausePanel.GetComponent<Image>() : null;
        if (pauseImg != null && pauseImg.sprite != null)
        {
            bgImg.sprite = pauseImg.sprite;
            bgImg.color  = Color.white;
            bgImg.type   = Image.Type.Simple;
        }
        else
        {
            bgImg.color = new Color(0.15f, 0.05f, 0.05f, 1f);
        }

        // Dark tint so text is readable
        MakeFullRect(optionsCanvas.transform, new Color(0f, 0f, 0f, 0.45f));

        // Card
        GameObject card = MakeCard(optionsCanvas.transform, new Vector2(600f, 320f));
        MakeTitle(card.transform, "OPCIONES", new Vector2(0f, 100f), new Vector2(500f, 70f));
        MakeDivider(card.transform, new Vector2(0f, 58f));

        MakeSliderRow(card.transform, "MÚSICA",  new Vector2(0f, 15f),
            v => AudioManager.Instance?.SetMusicVolume(v),
            () => AudioManager.Instance != null ? AudioManager.Instance.musicVolume : 0.4f);
        MakeSliderRow(card.transform, "EFECTOS", new Vector2(0f, -55f),
            v => AudioManager.Instance?.SetSFXVolume(v),
            () => AudioManager.Instance != null ? AudioManager.Instance.sfxVolume : 0.8f);

        MakeBtn(card.transform, "CERRAR", new Vector2(0f, -128f), new Vector2(220f, 50f), HideOptions);

        optionsCanvas.SetActive(false);
    }

    // ─── PAUSE PANEL ───────────────────────────────────────────────────────
    private void StylePausePanel()
    {
        if (pausePanel == null) return;

        // Hide original buttons (don't delete — they stay inactive)
        foreach (Button b in pausePanel.GetComponentsInChildren<Button>(true))
            b.gameObject.SetActive(false);

        // The pause panel has localScale 0.76, so we compensate button sizes:
        // desired visual size ~290x50 → local size = desired / 0.76
        const float btnW = 382f, btnH = 66f, spacing = 88f;

        // 4 buttons stacked, centered vertically
        float startY = spacing * 1.5f;
        MakeBtn(pausePanel.transform, "CONTINUAR",      new Vector2(0f, startY),             new Vector2(btnW, btnH), Resume);
        MakeBtn(pausePanel.transform, "REINTENTAR",     new Vector2(0f, startY - spacing),   new Vector2(btnW, btnH), Retry);
        MakeBtn(pausePanel.transform, "MENÚ PRINCIPAL", new Vector2(0f, startY - spacing*2), new Vector2(btnW, btnH), GoToMenu);
        MakeBtn(pausePanel.transform, "OPCIONES",       new Vector2(0f, startY - spacing*3), new Vector2(btnW, btnH), ShowOptions);
    }

    // ─── UI PRIMITIVES ─────────────────────────────────────────────────────
    private GameObject MakeOverlay(string name, int order)
    {
        GameObject go = new GameObject(name);
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = order;
        CanvasScaler cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920f, 1080f);
        cs.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    private void MakeFullRect(Transform parent, Color color)
    {
        GameObject go = new GameObject("Overlay");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
        go.AddComponent<Image>().color = color;
    }

    private GameObject MakeCard(Transform parent, Vector2 size)
    {
        GameObject go = new GameObject("Card");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size; rt.anchoredPosition = Vector2.zero;
        go.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
        return go;
    }

    private void MakeTitle(Transform parent, string text, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject("Title");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = 58f; t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center; t.color = Color.white;
    }

    private void MakeSubtitle(Transform parent, string text, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject("Sub");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = 21f;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    private void MakeDivider(Transform parent, Vector2 pos)
    {
        GameObject go = new GameObject("Divider");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(420f, 1f);
        go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.3f);
    }

    private void MakeBtn(Transform parent, string text, Vector2 pos, Vector2 size, System.Action onClick)
    {
        GameObject go = new GameObject("Btn_" + text);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.40f, 0.78f, 0.40f, 1f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(0.85f, 1f, 0.85f, 1f);
        cb.pressedColor     = new Color(0.65f, 0.82f, 0.65f, 1f);
        cb.selectedColor    = cb.highlightedColor;
        cb.fadeDuration     = 0.05f;
        btn.colors = cb;

        System.Action captured = onClick;
        btn.onClick.AddListener(() => captured?.Invoke());
        go.AddComponent<UIButtonAnimator>();

        GameObject lbl = new GameObject("Lbl");
        lbl.transform.SetParent(go.transform, false);
        RectTransform lr = lbl.AddComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.sizeDelta = Vector2.zero;
        TextMeshProUGUI t = lbl.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = 22f; t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(0.08f, 0.08f, 0.12f, 1f);
    }

    // Slider row centered: total unit width = 160 (label) + 20 (gap) + 340 (slider) = 520px → center offset = -260
    private void MakeSliderRow(Transform parent, string labelText, Vector2 pos,
        System.Action<float> onChange, System.Func<float> getValue)
    {
        const float labelW = 160f, sliderW = 320f, gap = 20f;
        float totalW = labelW + gap + sliderW;
        float leftEdge = -totalW / 2f;

        // Label
        GameObject lGO = new GameObject("Lbl_" + labelText);
        lGO.transform.SetParent(parent, false);
        RectTransform lr = lGO.AddComponent<RectTransform>();
        lr.anchorMin = new Vector2(0.5f, 0.5f); lr.anchorMax = new Vector2(0.5f, 0.5f);
        lr.pivot = new Vector2(0f, 0.5f);
        lr.anchoredPosition = new Vector2(leftEdge, pos.y); lr.sizeDelta = new Vector2(labelW, 36f);
        TextMeshProUGUI lt = lGO.AddComponent<TextMeshProUGUI>();
        lt.text = labelText; lt.fontSize = 21f; lt.fontStyle = FontStyles.Bold;
        lt.alignment = TextAlignmentOptions.Right;
        lt.color = Color.white;

        // Slider
        GameObject sGO = new GameObject("Slider_" + labelText);
        sGO.transform.SetParent(parent, false);
        RectTransform sr = sGO.AddComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.5f, 0.5f); sr.anchorMax = new Vector2(0.5f, 0.5f);
        sr.pivot = new Vector2(0f, 0.5f);
        sr.anchoredPosition = new Vector2(leftEdge + labelW + gap, pos.y); sr.sizeDelta = new Vector2(sliderW, 28f);
        Slider slider = sGO.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f;

        // BG
        GameObject bg = new GameObject("BG"); bg.transform.SetParent(sGO.transform, false);
        RectTransform bgr = bg.AddComponent<RectTransform>();
        bgr.anchorMin = Vector2.zero; bgr.anchorMax = Vector2.one; bgr.sizeDelta = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);

        // Fill
        GameObject fa = new GameObject("FA"); fa.transform.SetParent(sGO.transform, false);
        RectTransform far = fa.AddComponent<RectTransform>();
        far.anchorMin = Vector2.zero; far.anchorMax = Vector2.one;
        far.offsetMin = new Vector2(4f, 3f); far.offsetMax = new Vector2(-14f, -3f);
        GameObject fill = new GameObject("Fill"); fill.transform.SetParent(fa.transform, false);
        RectTransform fr = fill.AddComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = new Vector2(1f, 1f); fr.sizeDelta = Vector2.zero;
        fill.AddComponent<Image>().color = Color.white;
        slider.fillRect = fr;

        // Handle
        GameObject ha = new GameObject("HA"); ha.transform.SetParent(sGO.transform, false);
        RectTransform har = ha.AddComponent<RectTransform>();
        har.anchorMin = Vector2.zero; har.anchorMax = Vector2.one;
        har.offsetMin = new Vector2(10f, 0f); har.offsetMax = new Vector2(-10f, 0f);
        GameObject handle = new GameObject("Handle"); handle.transform.SetParent(ha.transform, false);
        RectTransform hr = handle.AddComponent<RectTransform>(); hr.sizeDelta = new Vector2(26f, 26f);
        Image handleImg = handle.AddComponent<Image>(); handleImg.color = Color.white;
        slider.handleRect = hr; slider.targetGraphic = handleImg;

        slider.SetValueWithoutNotify(getValue());
        slider.onValueChanged.AddListener(v => onChange(v));
    }
}
