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
    private GameObject optMainPanel;
    private GameObject optVolumePanel;
    private GameObject optControlsPanel;
    private GameObject recipeCanvas;
    private GameObject recipeHint;
    private bool isRecipeOpen = false;

    private System.Collections.Generic.Dictionary<string, TextMeshProUGUI> gameOverStatTexts = new();
    private System.Collections.Generic.Dictionary<string, TextMeshProUGUI> winStatTexts = new();

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

        // Ensure GameStats exists
        if (FindFirstObjectByType<GameStats>() == null)
            gameObject.AddComponent<GameStats>();

        BuildWinCanvas();
        BuildGameOverCanvas();
        BuildOptionsCanvas();
        BuildRecipeCanvas();
        StylePausePanel();

        if (GetComponent<CameraFlash>() == null)
            gameObject.AddComponent<CameraFlash>();

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
        if (Input.GetKeyDown(KeyCode.Tab) && !IsGameOverShowing())
        {
            if (isRecipeOpen)
                HideRecipe();
            else
                ShowRecipe();
        }

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
        UpdateStats(gameOverStatTexts);
        if (gameOverCanvas != null) gameOverCanvas.SetActive(true);
        AudioManager.Instance?.PlayLose();
        Time.timeScale = 0f;
    }

    public void ShowWin()
    {
        isWin = true;
        UpdateStats(winStatTexts);
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
        ShowOptionsMain();
        optionsCanvas.SetActive(true);
    }

    public void HideOptions()
    {
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
    }

    // ─── RECIPE MENU ───────────────────────────────────────────────────────
    public void ShowRecipe()
    {
        if (recipeCanvas != null)
        {
            recipeCanvas.SetActive(true);
            if (recipeHint != null) recipeHint.SetActive(false);
            isRecipeOpen = true;
        }
    }

    public void HideRecipe()
    {
        if (recipeCanvas != null)
        {
            recipeCanvas.SetActive(false);
            if (recipeHint != null) recipeHint.SetActive(true);
            isRecipeOpen = false;
        }
    }

    private void ShowOptionsMain()
    {
        if (optMainPanel     != null) optMainPanel.SetActive(true);
        if (optVolumePanel   != null) optVolumePanel.SetActive(false);
        if (optControlsPanel != null) optControlsPanel.SetActive(false);
    }

    private void ShowVolumePanel()
    {
        SyncSliders();
        if (optMainPanel     != null) optMainPanel.SetActive(false);
        if (optVolumePanel   != null) optVolumePanel.SetActive(true);
        if (optControlsPanel != null) optControlsPanel.SetActive(false);
    }

    private void ShowControlsPanel()
    {
        if (optMainPanel     != null) optMainPanel.SetActive(false);
        if (optVolumePanel   != null) optVolumePanel.SetActive(false);
        if (optControlsPanel != null) optControlsPanel.SetActive(true);
    }

    private void SyncSliders()
    {
        if (AudioManager.Instance == null || optVolumePanel == null) return;
        Slider[] sliders = optVolumePanel.GetComponentsInChildren<Slider>(true);
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

        GameObject card = MakeCard(winCanvas.transform, new Vector2(650f, 550f));
        MakeTitle(card.transform,    "VICTORIA",               new Vector2(0f, 230f),  new Vector2(540f, 80f));
        MakeSubtitle(card.transform, "Has completado el nivel",new Vector2(0f, 168f),  new Vector2(500f, 32f));
        MakeDivider(card.transform, new Vector2(0f, 135f));

        // Stats
        AddStatLine(card.transform, "Daño realizado:", "damageDealt", new Vector2(-150f, 80f), winStatTexts);
        AddStatLine(card.transform, "Daño recibido:", "damageTaken", new Vector2(-150f, 40f), winStatTexts);
        AddStatLine(card.transform, "Daño curado:", "damageHealed", new Vector2(-150f, 0f), winStatTexts);
        AddStatLine(card.transform, "Zombies matados:", "zombiesKilled", new Vector2(-150f, -40f), winStatTexts);
        AddStatLine(card.transform, "Tiempo:", "time", new Vector2(-150f, -80f), winStatTexts);

        if (isLevel2)
            MakeBtn(card.transform, "JUGAR DE NUEVO", new Vector2(0f, -160f),  new Vector2(300f, 52f), Retry);
        else
            MakeBtn(card.transform, "BONUS LEVEL",    new Vector2(0f, -160f),  new Vector2(300f, 52f), BonusLevel);

        MakeBtn(card.transform, "MENÚ PRINCIPAL", new Vector2(0f, -226f), new Vector2(300f, 52f), GoToMenu);
        winCanvas.SetActive(false);
    }

    // ─── GAMEOVER CANVAS ───────────────────────────────────────────────────
    private void BuildGameOverCanvas()
    {
        gameOverCanvas = MakeOverlay("GameOverOverlay", 250);
        MakeFullRect(gameOverCanvas.transform, new Color(0f, 0f, 0f, 0.82f));

        GameObject card = MakeCard(gameOverCanvas.transform, new Vector2(650f, 550f));
        MakeTitle(card.transform,    "HAS MUERTO",                    new Vector2(0f, 230f), new Vector2(540f, 80f));
        MakeSubtitle(card.transform, "No lo has conseguido esta vez", new Vector2(0f, 168f), new Vector2(500f, 32f));
        MakeDivider(card.transform, new Vector2(0f, 135f));

        // Stats
        AddStatLine(card.transform, "Daño realizado:", "damageDealt", new Vector2(-150f, 80f), gameOverStatTexts);
        AddStatLine(card.transform, "Daño recibido:", "damageTaken", new Vector2(-150f, 40f), gameOverStatTexts);
        AddStatLine(card.transform, "Daño curado:", "damageHealed", new Vector2(-150f, 0f), gameOverStatTexts);
        AddStatLine(card.transform, "Zombies matados:", "zombiesKilled", new Vector2(-150f, -40f), gameOverStatTexts);
        AddStatLine(card.transform, "Tiempo:", "time", new Vector2(-150f, -80f), gameOverStatTexts);

        MakeBtn(card.transform, "REINTENTAR",     new Vector2(0f, -160f),  new Vector2(300f, 52f), Retry);
        MakeBtn(card.transform, "MENÚ PRINCIPAL", new Vector2(0f, -226f), new Vector2(300f, 52f), GoToMenu);
        gameOverCanvas.SetActive(false);
    }

    // ─── OPTIONS CANVAS ────────────────────────────────────────────────────
    private void BuildOptionsCanvas()
    {
        optionsCanvas = MakeOverlay("OptionsOverlay", 300);

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

        MakeFullRect(optionsCanvas.transform, new Color(0f, 0f, 0f, 0.45f));

        // ── Panel principal de opciones ─────────────────────────────────────
        optMainPanel = MakeCard(optionsCanvas.transform, new Vector2(700f, 420f));
        MakeTitle(optMainPanel.transform, "OPCIONES", new Vector2(0f, 140f), new Vector2(600f, 72f));
        MakeDivider(optMainPanel.transform, new Vector2(0f, 92f));
        MakeBtn(optMainPanel.transform, "VOLUMEN",    new Vector2(0f,  26f), new Vector2(260f, 58f), ShowVolumePanel);
        MakeBtn(optMainPanel.transform, "CONTROLES",  new Vector2(0f, -58f), new Vector2(260f, 58f), ShowControlsPanel);
        MakeBtn(optMainPanel.transform, "CERRAR",     new Vector2(0f,-148f), new Vector2(260f, 58f), HideOptions);

        // ── Panel de volumen ────────────────────────────────────────────────
        optVolumePanel = MakeCard(optionsCanvas.transform, new Vector2(700f, 380f));
        MakeTitle(optVolumePanel.transform, "VOLUMEN", new Vector2(0f, 120f), new Vector2(600f, 72f));
        MakeDivider(optVolumePanel.transform, new Vector2(0f, 72f));
        MakeSliderRow(optVolumePanel.transform, "MÚSICA",   new Vector2(0f,  18f),
            v => AudioManager.Instance?.SetMusicVolume(v),
            () => AudioManager.Instance != null ? AudioManager.Instance.musicVolume : 0.4f);
        MakeSliderRow(optVolumePanel.transform, "EFECTOS", new Vector2(0f, -65f),
            v => AudioManager.Instance?.SetSFXVolume(v),
            () => AudioManager.Instance != null ? AudioManager.Instance.sfxVolume : 0.8f);
        MakeBtn(optVolumePanel.transform, "VOLVER", new Vector2(0f,-152f), new Vector2(260f, 58f), ShowOptionsMain);
        optVolumePanel.SetActive(false);

        // ── Panel de controles ──────────────────────────────────────────────
        optControlsPanel = MakeCard(optionsCanvas.transform, new Vector2(780f, 530f));
        MakeTitle(optControlsPanel.transform, "CONTROLES", new Vector2(0f, 220f), new Vector2(700f, 72f));
        MakeDivider(optControlsPanel.transform, new Vector2(0f, 170f));
        BuildControlsGrid(optControlsPanel.transform);
        MakeBtn(optControlsPanel.transform, "VOLVER", new Vector2(0f,-220f), new Vector2(260f, 58f), ShowOptionsMain);
        optControlsPanel.SetActive(false);

        optionsCanvas.SetActive(false);
    }

    private void BuildControlsGrid(Transform parent)
    {
        var controls = new (string key, string action)[]
        {
            ("WASD",      "Mover"),
            ("MOUSE",     "Apuntar"),
            ("L. CLICK",  "Disparar"),
            ("L. SHIFT",  "Correr"),
            ("E",         "Interactuar"),
            ("F",         "Combinar"),
            ("ESC",       "Pausa"),
        };

        float startY  = 120f;
        float rowH    = 46f;
        float colKeyL = -310f;
        float colKeyR =   18f;
        int   half    = (controls.Length + 1) / 2;

        for (int i = 0; i < controls.Length; i++)
        {
            float x = i < half ? colKeyL : colKeyR;
            float y = startY - (i < half ? i : i - half) * rowH;
            MakeControlRow(parent, controls[i].key, controls[i].action, new Vector2(x, y));
        }
    }

    private void MakeControlRow(Transform parent, string key, string action, Vector2 pos)
    {
        const float badgeW = 110f, labelW = 140f, gap = 8f;

        // Tecla (badge)
        GameObject keyGO = new GameObject("Key");
        keyGO.transform.SetParent(parent, false);
        RectTransform kr = keyGO.AddComponent<RectTransform>();
        kr.anchorMin = new Vector2(0.5f, 0.5f); kr.anchorMax = new Vector2(0.5f, 0.5f);
        kr.pivot = new Vector2(0f, 0.5f);
        kr.anchoredPosition = pos; kr.sizeDelta = new Vector2(badgeW, 30f);
        keyGO.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.25f, 1f);

        GameObject keyLbl = new GameObject("Lbl");
        keyLbl.transform.SetParent(keyGO.transform, false);
        RectTransform klr = keyLbl.AddComponent<RectTransform>();
        klr.anchorMin = Vector2.zero; klr.anchorMax = Vector2.one; klr.sizeDelta = Vector2.zero;
        TextMeshProUGUI kt = keyLbl.AddComponent<TextMeshProUGUI>();
        kt.text = key; kt.fontSize = 13f; kt.fontStyle = FontStyles.Bold;
        kt.alignment = TextAlignmentOptions.Center;
        kt.color = new Color(0.95f, 0.82f, 0.4f, 1f);

        // Acción
        GameObject actGO = new GameObject("Act");
        actGO.transform.SetParent(parent, false);
        RectTransform ar = actGO.AddComponent<RectTransform>();
        ar.anchorMin = new Vector2(0.5f, 0.5f); ar.anchorMax = new Vector2(0.5f, 0.5f);
        ar.pivot = new Vector2(0f, 0.5f);
        ar.anchoredPosition = new Vector2(pos.x + badgeW + gap, pos.y);
        ar.sizeDelta = new Vector2(labelW, 30f);
        TextMeshProUGUI at = actGO.AddComponent<TextMeshProUGUI>();
        at.text = action; at.fontSize = 14f;
        at.alignment = TextAlignmentOptions.Left;
        at.color = new Color(0.9f, 0.9f, 0.9f, 1f);
    }

    // ─── PAUSE PANEL ───────────────────────────────────────────────────────
    private void StylePausePanel()
    {
        if (pausePanel == null) return;

        foreach (Button b in pausePanel.GetComponentsInChildren<Button>(true))
            b.gameObject.SetActive(false);

        const float btnW = 382f, btnH = 66f, spacing = 88f;

        float startY = spacing * 1.5f;
        MakeBtn(pausePanel.transform, "CONTINUAR",      new Vector2(0f, startY),             new Vector2(btnW, btnH), Resume);
        MakeBtn(pausePanel.transform, "REINTENTAR",     new Vector2(0f, startY - spacing),   new Vector2(btnW, btnH), Retry);
        MakeBtn(pausePanel.transform, "MENÚ PRINCIPAL", new Vector2(0f, startY - spacing*2), new Vector2(btnW, btnH), GoToMenu);
        MakeBtn(pausePanel.transform, "OPCIONES",       new Vector2(0f, startY - spacing*3), new Vector2(btnW, btnH), ShowOptions);
    }

    // ─── UI PRIMITIVOS ─────────────────────────────────────────────────────
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
        rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(500f, 1f);
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

    // Fila de slider: ancho total = 160 (label) + 20 (gap) + 320 (slider) = 500px
    private void MakeSliderRow(Transform parent, string labelText, Vector2 pos,
        System.Action<float> onChange, System.Func<float> getValue)
    {
        const float labelW = 160f, sliderW = 320f, gap = 20f;
        float totalW   = labelW + gap + sliderW;
        float leftEdge = -totalW / 2f;

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

        GameObject sGO = new GameObject("Slider_" + labelText);
        sGO.transform.SetParent(parent, false);
        RectTransform sr = sGO.AddComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.5f, 0.5f); sr.anchorMax = new Vector2(0.5f, 0.5f);
        sr.pivot = new Vector2(0f, 0.5f);
        sr.anchoredPosition = new Vector2(leftEdge + labelW + gap, pos.y); sr.sizeDelta = new Vector2(sliderW, 28f);
        Slider slider = sGO.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f;

        GameObject bg = new GameObject("BG"); bg.transform.SetParent(sGO.transform, false);
        RectTransform bgr = bg.AddComponent<RectTransform>();
        bgr.anchorMin = Vector2.zero; bgr.anchorMax = Vector2.one; bgr.sizeDelta = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);

        GameObject fa = new GameObject("FA"); fa.transform.SetParent(sGO.transform, false);
        RectTransform far = fa.AddComponent<RectTransform>();
        far.anchorMin = Vector2.zero; far.anchorMax = Vector2.one;
        far.offsetMin = new Vector2(4f, 3f); far.offsetMax = new Vector2(-14f, -3f);
        GameObject fill = new GameObject("Fill"); fill.transform.SetParent(fa.transform, false);
        RectTransform fr = fill.AddComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = new Vector2(1f, 1f); fr.sizeDelta = Vector2.zero;
        fill.AddComponent<Image>().color = Color.white;
        slider.fillRect = fr;

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

    // ─── RECIPE CANVAS ─────────────────────────────────────────────────────
    private void BuildRecipeCanvas()
    {
        recipeCanvas = MakeOverlay("RecipeOverlay", 240);

        GameObject container = new GameObject("Container");
        container.transform.SetParent(recipeCanvas.transform, false);
        RectTransform containerRT = container.AddComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(1f, 1f);
        containerRT.anchorMax = new Vector2(1f, 1f);
        containerRT.pivot = new Vector2(1f, 1f);
        containerRT.anchoredPosition = new Vector2(-30f, -30f);
        containerRT.sizeDelta = new Vector2(350f, 700f);

        // Title
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(container.transform, false);
        RectTransform titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(1f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot = new Vector2(1f, 1f);
        titleRT.anchoredPosition = Vector2.zero;
        titleRT.sizeDelta = new Vector2(300f, 50f);

        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "RECETAS";
        titleText.fontSize = 24f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.TopRight;
        titleText.color = Color.white;

        // Recipes list
        PopulateRecipes(container.transform);

        recipeCanvas.SetActive(false);

        // Create hint canvas
        BuildRecipeHint();
    }

    private void BuildRecipeHint()
    {
        recipeHint = MakeOverlay("RecipeHintOverlay", 235);

        GameObject hintContainer = new GameObject("HintContainer");
        hintContainer.transform.SetParent(recipeHint.transform, false);
        RectTransform hintRT = hintContainer.AddComponent<RectTransform>();
        hintRT.anchorMin = new Vector2(1f, 1f);
        hintRT.anchorMax = new Vector2(1f, 1f);
        hintRT.pivot = new Vector2(1f, 1f);
        hintRT.anchoredPosition = new Vector2(-30f, -30f);
        hintRT.sizeDelta = new Vector2(350f, 100f);

        // Tab key badge
        GameObject tabKeyGO = new GameObject("TabKey");
        tabKeyGO.transform.SetParent(hintContainer.transform, false);
        RectTransform tabKeyRT = tabKeyGO.AddComponent<RectTransform>();
        tabKeyRT.anchorMin = new Vector2(1f, 1f);
        tabKeyRT.anchorMax = new Vector2(1f, 1f);
        tabKeyRT.pivot = new Vector2(1f, 0.5f);
        tabKeyRT.anchoredPosition = new Vector2(-10f, -25f);
        tabKeyRT.sizeDelta = new Vector2(50f, 28f);

        Image tabKeyImg = tabKeyGO.AddComponent<Image>();
        tabKeyImg.color = new Color(0.18f, 0.18f, 0.25f, 1f);

        GameObject tabKeyLbl = new GameObject("Lbl");
        tabKeyLbl.transform.SetParent(tabKeyGO.transform, false);
        RectTransform tabKeyLblRT = tabKeyLbl.AddComponent<RectTransform>();
        tabKeyLblRT.anchorMin = Vector2.zero;
        tabKeyLblRT.anchorMax = Vector2.one;
        tabKeyLblRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tabKeyText = tabKeyLbl.AddComponent<TextMeshProUGUI>();
        tabKeyText.text = "TAB";
        tabKeyText.fontSize = 12f;
        tabKeyText.fontStyle = FontStyles.Bold;
        tabKeyText.alignment = TextAlignmentOptions.Center;
        tabKeyText.color = new Color(0.95f, 0.82f, 0.4f, 1f);

        // Text "PARA VER RECETAS"
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(hintContainer.transform, false);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = new Vector2(1f, 1f);
        textRT.anchorMax = new Vector2(1f, 1f);
        textRT.pivot = new Vector2(1f, 0.5f);
        textRT.anchoredPosition = new Vector2(-70f, -25f);
        textRT.sizeDelta = new Vector2(200f, 50f);

        TextMeshProUGUI hintText = textGO.AddComponent<TextMeshProUGUI>();
        hintText.text = "PARA VER\nRECETAS";
        hintText.fontSize = 14f;
        hintText.fontStyle = FontStyles.Bold;
        hintText.alignment = TextAlignmentOptions.BottomRight;
        hintText.color = new Color(0.8f, 0.8f, 0.8f, 1f);

        recipeHint.SetActive(true);
    }

    private void PopulateRecipes(Transform parent)
    {
        System.Collections.Generic.List<(BulletType a, BulletType b, BulletType result)> recipes = BulletInventory.GetAllRecipes();

        float startY = -60f;
        float rowHeight = 35f;
        int index = 0;

        foreach (var recipe in recipes)
        {
            float yPos = startY - (index * rowHeight);
            string aColor = GetRecipeColor(recipe.a);
            string bColor = GetRecipeColor(recipe.b);
            string resultColor = GetRecipeColor(recipe.result);
            string recipeText = $"<color={aColor}>{recipe.a}</color> + <color={bColor}>{recipe.b}</color>\n= <color={resultColor}>{recipe.result}</color>";

            MakeRecipeRow(parent, recipeText, new Vector2(0f, yPos));
            index++;
        }
    }

    private void MakeRecipeRow(Transform parent, string text, Vector2 pos)
    {
        GameObject go = new GameObject("RecipeRow");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(320f, 60f);

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = 13f;
        t.alignment = TextAlignmentOptions.TopRight;
        t.color = Color.white;
    }

    private string GetRecipeColor(BulletType type)
    {
        return type switch
        {
            BulletType.Regular => "#DDDDDD",
            BulletType.Heavy => "#FF8800",
            BulletType.Bouncy => "#00DDFF",
            BulletType.Area => "#88FF44",
            BulletType.Frag => "#FFEE00",
            BulletType.Target => "#CC88FF",
            BulletType.Chain => "#44AAFF",
            BulletType.Piercing => "#FF4444",
            _ => "#FFFFFF"
        };
    }

    // ─── STATS DISPLAY ────────────────────────────────────────────────────
    private void AddStatLine(Transform parent, string label, string statKey, Vector2 pos, System.Collections.Generic.Dictionary<string, TextMeshProUGUI> statDict)
    {
        GameObject statGO = new GameObject("Stat_" + statKey);
        statGO.transform.SetParent(parent, false);
        RectTransform statRT = statGO.AddComponent<RectTransform>();
        statRT.anchorMin = new Vector2(0.5f, 0.5f); statRT.anchorMax = new Vector2(0.5f, 0.5f);
        statRT.pivot = new Vector2(0.5f, 0.5f); statRT.anchoredPosition = pos; statRT.sizeDelta = new Vector2(500f, 30f);

        TextMeshProUGUI statText = statGO.AddComponent<TextMeshProUGUI>();
        statText.text = $"{label} <color=#FFFF00>--</color>";
        statText.fontSize = 16f;
        statText.alignment = TextAlignmentOptions.Left;
        statText.color = Color.white;

        statDict[statKey] = statText;
    }

    private void UpdateStats(System.Collections.Generic.Dictionary<string, TextMeshProUGUI> statDict)
    {
        GameStats stats = FindFirstObjectByType<GameStats>();
        if (stats == null) return;

        if (statDict.ContainsKey("damageDealt"))
            statDict["damageDealt"].text = $"Daño realizado: <color=#FFFF00>{((int)stats.damageDealt)}</color>";
        if (statDict.ContainsKey("damageTaken"))
            statDict["damageTaken"].text = $"Daño recibido: <color=#FFFF00>{((int)stats.damageTaken)}</color>";
        if (statDict.ContainsKey("damageHealed"))
            statDict["damageHealed"].text = $"Daño curado: <color=#FFFF00>{((int)stats.damageHealed)}</color>";
        if (statDict.ContainsKey("zombiesKilled"))
            statDict["zombiesKilled"].text = $"Zombies matados: <color=#FFFF00>{stats.zombiesKilled}</color>";
        if (statDict.ContainsKey("time"))
            statDict["time"].text = $"Tiempo: <color=#FFFF00>{stats.GetFormattedTime(stats.GetGameDuration())}</color>";
    }
}
