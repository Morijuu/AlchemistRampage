using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasController : MonoBehaviour
{
    public GameObject canvasInicio;
    public GameObject canvasMenuPrincipal;
    public GameObject canvasOpciones;

    private Slider musicSlider;
    private Slider sfxSlider;

    private GameObject optMain;
    private GameObject optVolume;
    private GameObject optControls;

    // Estilo robado de los botones de escena
    private Sprite       _btnSprite;
    private Image.Type   _btnImageType = Image.Type.Sliced;
    private TMP_FontAsset _btnFont;
    private float        _btnFontSize  = 25.56f;
    private Color        _btnLblColor  = new Color(0.45f, 0.35f, 0.12f, 1f);

    // Dimensiones exactas de los botones de escena
    private const float BTN_W     = 186.1233f;
    private const float BTN_H     = 30f;
    private const float BTN_SCALE = 3.73f;

    private void Start()
    {
        canvasInicio.SetActive(true);
        canvasMenuPrincipal.SetActive(false);
        canvasOpciones.SetActive(false);

        RestyleAllButtons();
        GrabSceneButtonStyle();
        BuildOptionsSubPanels();
    }

    public void MostrarMenuPrincipal()
    {
        canvasInicio.SetActive(false);
        canvasMenuPrincipal.SetActive(true);
        canvasOpciones.SetActive(false);
    }

    public void Opciones()
    {
        canvasInicio.SetActive(false);
        canvasMenuPrincipal.SetActive(false);
        canvasOpciones.SetActive(true);
        ShowOptionsMain();
    }

    private void ShowOptionsMain()
    {
        if (optMain     != null) optMain.SetActive(true);
        if (optVolume   != null) optVolume.SetActive(false);
        if (optControls != null) optControls.SetActive(false);
    }

    private void ShowVolumeSection()
    {
        SyncSliders();
        if (optMain     != null) optMain.SetActive(false);
        if (optVolume   != null) optVolume.SetActive(true);
        if (optControls != null) optControls.SetActive(false);
    }

    private void ShowControlsSection()
    {
        if (optMain     != null) optMain.SetActive(false);
        if (optVolume   != null) optVolume.SetActive(false);
        if (optControls != null) optControls.SetActive(true);
    }

    private void SyncSliders()
    {
        if (AudioManager.Instance == null) return;
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(AudioManager.Instance.musicVolume);
        if (sfxSlider   != null) sfxSlider.SetValueWithoutNotify(AudioManager.Instance.sfxVolume);
    }

    // ─── Copia animación a todos los botones existentes ──────────────────────
    private void RestyleAllButtons()
    {
        foreach (Button b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (b.GetComponent<UIButtonAnimator>() == null)
                b.gameObject.AddComponent<UIButtonAnimator>();

            Image img = b.GetComponent<Image>();
            if (img != null) img.color = new Color(0.40f, 0.78f, 0.40f, 1f);

            ColorBlock cb = b.colors;
            cb.normalColor      = Color.white;
            cb.highlightedColor = new Color(0.85f, 1f, 0.85f, 1f);
            cb.pressedColor     = new Color(0.65f, 0.82f, 0.65f, 1f);
            cb.selectedColor    = cb.highlightedColor;
            cb.fadeDuration     = 0.05f;
            b.colors = cb;
        }
    }

    // ─── Roba sprite y fuente de los botones de escena para igualarlo ─────────
    private void GrabSceneButtonStyle()
    {
        if (canvasMenuPrincipal == null) return;
        Button[] btns = canvasMenuPrincipal.GetComponentsInChildren<Button>(true);
        if (btns.Length == 0) return;

        Image img = btns[0].GetComponent<Image>();
        if (img != null)
        {
            _btnSprite    = img.sprite;
            _btnImageType = img.type;
        }

        TextMeshProUGUI lbl = btns[0].GetComponentInChildren<TextMeshProUGUI>(true);
        if (lbl != null)
        {
            _btnFont     = lbl.font;
            _btnFontSize = lbl.fontSize;
            _btnLblColor = lbl.color;
        }
    }

    // ─── Construye los tres sub-paneles de opciones ───────────────────────────
    private void BuildOptionsSubPanels()
    {
        if (canvasOpciones == null) return;

        // Bloqueador transparente: impide clics en botones de escena pre-existentes
        GameObject blocker = new GameObject("Blocker");
        blocker.transform.SetParent(canvasOpciones.transform, false);
        RectTransform bRt = blocker.AddComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero; bRt.anchorMax = Vector2.one; bRt.sizeDelta = Vector2.zero;
        Image bImg = blocker.AddComponent<Image>();
        bImg.color = new Color(0f, 0f, 0f, 0f); // transparente pero bloquea raycasts
        bImg.raycastTarget = true;

        // ── Panel principal: VOLUMEN + CONTROLES + VOLVER ───────────────────
        optMain = MakeContainer(canvasOpciones.transform);
        MakeSceneBtn(optMain.transform, "VOLUMEN",   new Vector2(0f,  105f), ShowVolumeSection);
        MakeSceneBtn(optMain.transform, "CONTROLES", new Vector2(0f,  -75f), ShowControlsSection);
        MakeSceneBtn(optMain.transform, "VOLVER",    new Vector2(0f, -265f), MostrarMenuPrincipal);

        // ── Panel de volumen: sliders + VOLVER ─────────────────────────────
        optVolume = MakeContainer(canvasOpciones.transform);
        MakeOptTitle(optVolume.transform, "VOLUMEN", new Vector2(0f, 185f));
        musicSlider = MakeOptSliderRow(optVolume.transform, "MÚSICA",   new Vector2(0f, 80f));
        sfxSlider   = MakeOptSliderRow(optVolume.transform, "EFECTOS",  new Vector2(0f, -20f));
        musicSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetMusicVolume(v));
        sfxSlider.onValueChanged.AddListener  (v => AudioManager.Instance?.SetSFXVolume(v));
        MakeSceneBtn(optVolume.transform, "VOLVER", new Vector2(0f, -265f), ShowOptionsMain);
        optVolume.SetActive(false);

        // ── Panel de controles: grid + VOLVER ──────────────────────────────
        optControls = MakeContainer(canvasOpciones.transform);
        MakeOptTitle(optControls.transform, "CONTROLES", new Vector2(0f, 185f));
        BuildOptControlsGrid(optControls.transform);
        MakeSceneBtn(optControls.transform, "VOLVER", new Vector2(0f, -265f), ShowOptionsMain);
        optControls.SetActive(false);
    }

    // Contenedor invisible centrado que sirve de padre de los sub-elementos
    private GameObject MakeContainer(Transform parent)
    {
        GameObject go = new GameObject("Container");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(0f, 0f);
        rt.anchoredPosition = Vector2.zero;
        return go;
    }

    // Título flotante sobre el fondo de escena
    private void MakeOptTitle(Transform parent, string text, Vector2 pos)
    {
        GameObject go = new GameObject("Title");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f); rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(700f, 80f);
        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = _btnFontSize * 1.5f;
        t.fontStyle = FontStyles.Bold;
        if (_btnFont != null) t.font = _btnFont;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
    }

    // Botón con exactamente el mismo aspecto que los botones de escena
    private void MakeSceneBtn(Transform parent, string text, Vector2 pos, System.Action onClick)
    {
        GameObject go = new GameObject("Btn_" + text);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(BTN_W, BTN_H);
        go.transform.localScale = new Vector3(BTN_SCALE, BTN_SCALE, BTN_SCALE);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.40f, 0.78f, 0.40f, 1f);
        if (_btnSprite != null) { img.sprite = _btnSprite; img.type = _btnImageType; }

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

        // Etiqueta: inversa del scale para que el texto quede al tamaño correcto
        GameObject lbl = new GameObject("Lbl");
        lbl.transform.SetParent(go.transform, false);
        RectTransform lr = lbl.AddComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.sizeDelta = Vector2.zero;
        TextMeshProUGUI t = lbl.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = _btnFontSize;
        t.fontStyle = FontStyles.Bold;
        if (_btnFont != null) t.font = _btnFont;
        t.alignment = TextAlignmentOptions.Center;
        t.color = _btnLblColor;
    }

    // ─── Grid de controles ────────────────────────────────────────────────────
    private void BuildOptControlsGrid(Transform parent)
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

        float startY  = 110f;
        float rowH    = 50f;
        float colKeyL = -260f;
        float colKeyR =  30f;
        int   half    = (controls.Length + 1) / 2;

        for (int i = 0; i < controls.Length; i++)
        {
            float x = i < half ? colKeyL : colKeyR;
            float y = startY - (i < half ? i : i - half) * rowH;
            MakeOptControlRow(parent, controls[i].key, controls[i].action, new Vector2(x, y));
        }
    }

    private void MakeOptControlRow(Transform parent, string key, string action, Vector2 pos)
    {
        const float badgeW = 120f, labelW = 145f, gap = 10f;

        // Badge de tecla
        GameObject keyGO = new GameObject("Key");
        keyGO.transform.SetParent(parent, false);
        RectTransform kr = keyGO.AddComponent<RectTransform>();
        kr.anchorMin = new Vector2(0.5f, 0.5f); kr.anchorMax = new Vector2(0.5f, 0.5f);
        kr.pivot = new Vector2(0f, 0.5f);
        kr.anchoredPosition = pos; kr.sizeDelta = new Vector2(badgeW, 34f);
        keyGO.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.22f, 0.85f);

        GameObject keyLbl = new GameObject("Lbl");
        keyLbl.transform.SetParent(keyGO.transform, false);
        RectTransform klr = keyLbl.AddComponent<RectTransform>();
        klr.anchorMin = Vector2.zero; klr.anchorMax = Vector2.one; klr.sizeDelta = Vector2.zero;
        TextMeshProUGUI kt = keyLbl.AddComponent<TextMeshProUGUI>();
        kt.text = key; kt.fontSize = 16f; kt.fontStyle = FontStyles.Bold;
        kt.alignment = TextAlignmentOptions.Center;
        kt.color = new Color(0.95f, 0.82f, 0.4f, 1f);

        // Acción
        GameObject actGO = new GameObject("Act");
        actGO.transform.SetParent(parent, false);
        RectTransform ar = actGO.AddComponent<RectTransform>();
        ar.anchorMin = new Vector2(0.5f, 0.5f); ar.anchorMax = new Vector2(0.5f, 0.5f);
        ar.pivot = new Vector2(0f, 0.5f);
        ar.anchoredPosition = new Vector2(pos.x + badgeW + gap, pos.y);
        ar.sizeDelta = new Vector2(labelW, 34f);
        TextMeshProUGUI at = actGO.AddComponent<TextMeshProUGUI>();
        at.text = action; at.fontSize = 18f;
        at.alignment = TextAlignmentOptions.Left;
        at.color = Color.white;
    }

    // ─── Slider (sin fondo de tarjeta) ────────────────────────────────────────
    private Slider MakeOptSliderRow(Transform parent, string labelText, Vector2 pos)
    {
        const float labelW = 180f, sliderW = 380f, gap = 24f;
        float totalW   = labelW + gap + sliderW;
        float leftEdge = -totalW / 2f;

        // Etiqueta
        GameObject lGO = new GameObject("Lbl_" + labelText);
        lGO.transform.SetParent(parent, false);
        RectTransform lr = lGO.AddComponent<RectTransform>();
        lr.anchorMin = new Vector2(0.5f, 0.5f); lr.anchorMax = new Vector2(0.5f, 0.5f);
        lr.pivot = new Vector2(0f, 0.5f);
        lr.anchoredPosition = new Vector2(leftEdge, pos.y); lr.sizeDelta = new Vector2(labelW, 56f);
        TextMeshProUGUI lt = lGO.AddComponent<TextMeshProUGUI>();
        lt.text = labelText; lt.fontSize = 34f; lt.fontStyle = FontStyles.Bold;
        if (_btnFont != null) lt.font = _btnFont;
        lt.alignment = TextAlignmentOptions.Right;
        lt.color = Color.white;

        // Slider
        GameObject sGO = new GameObject("Slider_" + labelText);
        sGO.transform.SetParent(parent, false);
        RectTransform sr = sGO.AddComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.5f, 0.5f); sr.anchorMax = new Vector2(0.5f, 0.5f);
        sr.pivot = new Vector2(0f, 0.5f);
        sr.anchoredPosition = new Vector2(leftEdge + labelW + gap, pos.y); sr.sizeDelta = new Vector2(sliderW, 48f);
        Slider slider = sGO.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f;

        // BG
        GameObject bg = new GameObject("BG"); bg.transform.SetParent(sGO.transform, false);
        RectTransform bgr = bg.AddComponent<RectTransform>();
        bgr.anchorMin = Vector2.zero; bgr.anchorMax = Vector2.one; bgr.sizeDelta = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);

        // Fill
        GameObject fa = new GameObject("FA"); fa.transform.SetParent(sGO.transform, false);
        RectTransform far = fa.AddComponent<RectTransform>();
        far.anchorMin = Vector2.zero; far.anchorMax = Vector2.one;
        far.offsetMin = new Vector2(5f, 5f); far.offsetMax = new Vector2(-20f, -5f);
        GameObject fill = new GameObject("Fill"); fill.transform.SetParent(fa.transform, false);
        RectTransform fr = fill.AddComponent<RectTransform>();
        fr.anchorMin = Vector2.zero; fr.anchorMax = new Vector2(1f, 1f); fr.sizeDelta = Vector2.zero;
        fill.AddComponent<Image>().color = Color.white;
        slider.fillRect = fr;

        // Handle
        GameObject ha = new GameObject("HA"); ha.transform.SetParent(sGO.transform, false);
        RectTransform har = ha.AddComponent<RectTransform>();
        har.anchorMin = Vector2.zero; har.anchorMax = Vector2.one;
        har.offsetMin = new Vector2(12f, 0f); har.offsetMax = new Vector2(-12f, 0f);
        GameObject handle = new GameObject("Handle"); handle.transform.SetParent(ha.transform, false);
        RectTransform hr = handle.AddComponent<RectTransform>(); hr.sizeDelta = new Vector2(48f, 48f);
        Image handleImg = handle.AddComponent<Image>(); handleImg.color = Color.white;
        slider.handleRect = hr; slider.targetGraphic = handleImg;

        return slider;
    }
}
