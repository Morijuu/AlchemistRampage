using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to canvasOpciones. Builds volume sliders at runtime and wires them to AudioManager.
public class OptionsVolumePanel : MonoBehaviour
{
    private Slider musicSlider;
    private Slider sfxSlider;

    private void Start()
    {
        BuildSliders();
    }

    private void OnEnable()
    {
        // Sync sliders with current AudioManager values every time panel opens
        if (AudioManager.Instance == null) return;
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(AudioManager.Instance.musicVolume);
        if (sfxSlider   != null) sfxSlider.SetValueWithoutNotify(AudioManager.Instance.sfxVolume);
    }

    private void BuildSliders()
    {
        // Find or create a container inside this canvas
        RectTransform root = GetComponent<RectTransform>();
        if (root == null) return;

        // Container centered on screen
        GameObject container = new GameObject("VolumeSliders");
        container.transform.SetParent(transform, false);
        RectTransform cr = container.AddComponent<RectTransform>();
        cr.anchorMin = new Vector2(0.5f, 0.5f);
        cr.anchorMax = new Vector2(0.5f, 0.5f);
        cr.pivot     = new Vector2(0.5f, 0.5f);
        cr.anchoredPosition = new Vector2(0f, -80f);
        cr.sizeDelta = new Vector2(500f, 180f);

        musicSlider = CreateSliderRow(container.transform, "MÚSICA",   0f, 0f);
        sfxSlider   = CreateSliderRow(container.transform, "EFECTOS", -80f, 1f);

        if (AudioManager.Instance != null)
        {
            musicSlider.SetValueWithoutNotify(AudioManager.Instance.musicVolume);
            sfxSlider.SetValueWithoutNotify(AudioManager.Instance.sfxVolume);
        }

        musicSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetMusicVolume(v));
        sfxSlider.onValueChanged.AddListener  (v => AudioManager.Instance?.SetSFXVolume(v));
    }

    private Slider CreateSliderRow(Transform parent, string label, float yOffset, float defaultValue)
    {
        // Label
        GameObject labelGO = new GameObject("Label_" + label);
        labelGO.transform.SetParent(parent, false);
        RectTransform lr = labelGO.AddComponent<RectTransform>();
        lr.anchorMin = new Vector2(0f, 1f);
        lr.anchorMax = new Vector2(0f, 1f);
        lr.pivot     = new Vector2(0f, 1f);
        lr.anchoredPosition = new Vector2(0f, yOffset);
        lr.sizeDelta = new Vector2(500f, 36f);
        TextMeshProUGUI lTxt = labelGO.AddComponent<TextMeshProUGUI>();
        lTxt.text      = label;
        lTxt.fontSize  = 20f;
        lTxt.fontStyle = FontStyles.Bold;
        lTxt.alignment = TextAlignmentOptions.Left;
        lTxt.color     = new Color(0.95f, 0.85f, 0.5f, 1f);

        // Slider track background
        GameObject sliderGO = new GameObject("Slider_" + label);
        sliderGO.transform.SetParent(parent, false);
        RectTransform sr = sliderGO.AddComponent<RectTransform>();
        sr.anchorMin = new Vector2(0f, 1f);
        sr.anchorMax = new Vector2(0f, 1f);
        sr.pivot     = new Vector2(0f, 1f);
        sr.anchoredPosition = new Vector2(0f, yOffset - 38f);
        sr.sizeDelta = new Vector2(500f, 26f);

        Slider slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value    = defaultValue;

        // Background track
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderGO.transform, false);
        RectTransform bgr = bg.AddComponent<RectTransform>();
        bgr.anchorMin  = Vector2.zero;
        bgr.anchorMax  = Vector2.one;
        bgr.sizeDelta  = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color    = new Color(0.12f, 0.12f, 0.18f, 1f);

        // Fill area
        GameObject fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform far = fillArea.AddComponent<RectTransform>();
        far.anchorMin = new Vector2(0f, 0f);
        far.anchorMax = new Vector2(1f, 1f);
        far.offsetMin = new Vector2(5f, 2f);
        far.offsetMax = new Vector2(-15f, -2f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fr = fill.AddComponent<RectTransform>();
        fr.anchorMin = Vector2.zero;
        fr.anchorMax = new Vector2(1f, 1f);
        fr.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.95f, 0.75f, 0.15f, 1f);
        slider.fillRect = fr;

        // Handle area
        GameObject handleArea = new GameObject("HandleArea");
        handleArea.transform.SetParent(sliderGO.transform, false);
        RectTransform har = handleArea.AddComponent<RectTransform>();
        har.anchorMin = Vector2.zero;
        har.anchorMax = Vector2.one;
        har.offsetMin = new Vector2(10f, 0f);
        har.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform hr = handle.AddComponent<RectTransform>();
        hr.sizeDelta = new Vector2(22f, 22f);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        slider.handleRect = hr;

        slider.targetGraphic = handleImg;

        return slider;
    }
}
