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

    private void Start()
    {
        canvasInicio.SetActive(true);
        canvasMenuPrincipal.SetActive(false);
        canvasOpciones.SetActive(false);

        RestyleAllButtons();
        BuildVolumeSliders();
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
        SyncSliders();
    }

    private void SyncSliders()
    {
        if (AudioManager.Instance == null) return;
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(AudioManager.Instance.musicVolume);
        if (sfxSlider   != null) sfxSlider.SetValueWithoutNotify(AudioManager.Instance.sfxVolume);
    }

    // ─── Add hover/press animation to every Button (preserves existing colors) ──
    private void RestyleAllButtons()
    {
        // Include inactive so buttons inside hidden canvases are also set up
        foreach (Button b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (b.GetComponent<UIButtonAnimator>() == null)
                b.gameObject.AddComponent<UIButtonAnimator>();

            // Slime-feel: darken on hover/press without changing the base color
            ColorBlock cb = b.colors;
            cb.normalColor      = Color.white;
            cb.highlightedColor = new Color(0.82f, 1f, 0.82f, 1f);
            cb.pressedColor     = new Color(0.60f, 0.85f, 0.60f, 1f);
            cb.selectedColor    = cb.highlightedColor;
            cb.fadeDuration     = 0.05f;
            b.colors = cb;
        }
    }

    // ─── Volume sliders ───────────────────────────────────────────────────
    private void BuildVolumeSliders()
    {
        if (canvasOpciones == null) return;

        GameObject container = new GameObject("VolumeContainer");
        container.transform.SetParent(canvasOpciones.transform, false);
        RectTransform cr = container.AddComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.5f, 0.5f);
        cr.anchorMax = new Vector2(0.5f, 0.5f);
        cr.pivot     = new Vector2(0.5f, 0.5f);
        cr.anchoredPosition = new Vector2(0f, 0f);
        cr.sizeDelta = new Vector2(900f, 300f);

        musicSlider = MakeSliderRow(container.transform, "MÚSICA",  new Vector2(0f,  75f));
        sfxSlider   = MakeSliderRow(container.transform, "EFECTOS", new Vector2(0f, -75f));

        SyncSliders();

        musicSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetMusicVolume(v));
        sfxSlider.onValueChanged.AddListener  (v => AudioManager.Instance?.SetSFXVolume(v));
    }

    // label + slider unit, perfectly centered as a block
    private Slider MakeSliderRow(Transform parent, string labelText, Vector2 pos)
    {
        const float labelW = 200f, sliderW = 560f, gap = 28f;
        float leftEdge = -(labelW + gap + sliderW) / 2f;

        // Label
        GameObject lGO = new GameObject("Lbl_" + labelText);
        lGO.transform.SetParent(parent, false);
        RectTransform lr = lGO.AddComponent<RectTransform>();
        lr.anchorMin = new Vector2(0.5f, 0.5f); lr.anchorMax = new Vector2(0.5f, 0.5f);
        lr.pivot = new Vector2(0f, 0.5f);
        lr.anchoredPosition = new Vector2(leftEdge, pos.y);
        lr.sizeDelta = new Vector2(labelW, 56f);
        TextMeshProUGUI lt = lGO.AddComponent<TextMeshProUGUI>();
        lt.text = labelText; lt.fontSize = 40f; lt.fontStyle = FontStyles.Bold;
        lt.alignment = TextAlignmentOptions.Right;
        lt.color = Color.white;

        // Slider
        GameObject sGO = new GameObject("Slider_" + labelText);
        sGO.transform.SetParent(parent, false);
        RectTransform sr = sGO.AddComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.5f, 0.5f); sr.anchorMax = new Vector2(0.5f, 0.5f);
        sr.pivot = new Vector2(0f, 0.5f);
        sr.anchoredPosition = new Vector2(leftEdge + labelW + gap, pos.y);
        sr.sizeDelta = new Vector2(sliderW, 48f);

        Slider slider = sGO.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f;

        // BG track
        GameObject bg = new GameObject("BG"); bg.transform.SetParent(sGO.transform, false);
        RectTransform bgr = bg.AddComponent<RectTransform>();
        bgr.anchorMin = Vector2.zero; bgr.anchorMax = Vector2.one; bgr.sizeDelta = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.22f);

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
