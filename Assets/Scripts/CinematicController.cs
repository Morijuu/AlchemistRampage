using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CinematicController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject introTextCanvas;
    [SerializeField] private GameObject playerCanvas;

    [Header("Audio Cinemática")]
    [SerializeField] private AudioClip pitidoClip;
    [SerializeField] private AudioClip keyboardClip;

    // UI
    private Canvas cinCanvas;
    private RectTransform blinkTop;
    private RectTransform blinkBottom;
    private Image fadeImage;
    private GameObject dialoguePanel;
    private TextMeshProUGUI dialogueText;

    // Audio
    private AudioSource audioSource;

    // Post-processing
    private Volume postVolume;
    private VolumeProfile postProfile;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private ColorAdjustments colorAdjustments;

    private PlayerScript playerScript;
    private bool canSkip;
    private bool skipped;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        canSkip = PlayerPrefs.GetInt("CinematicSeen", 0) == 1;

        if (player != null)
        {
            playerScript = player.GetComponent<PlayerScript>();
            if (playerScript != null)
                playerScript.cinematicMode = true;

            player.rotation = Quaternion.Euler(0f, 0f, 90f);
        }

        if (playerCanvas != null) playerCanvas.SetActive(false);

        if (introTextCanvas != null)
            introTextCanvas.SetActive(false);

        SetupPostProcessing();
    }

    private void Update()
    {
        if (canSkip && !skipped && Input.GetKeyDown(KeyCode.Space))
            Skip();
    }

    private void Start()
    {
        BuildUI();
        StartCoroutine(RunCinematic());
    }

    // ─────────────────────────────────────────
    // POST-PROCESSING
    // ─────────────────────────────────────────
    private void SetupPostProcessing()
    {
        // Asegurarse de que la cámara principal tiene post-processing activo
        if (Camera.main != null)
        {
            var camData = Camera.main.GetUniversalAdditionalCameraData();
            if (camData != null)
                camData.renderPostProcessing = true;
        }

        // Crear volumen global temporal solo para la cinemática
        GameObject volGO = new GameObject("CinematicPostVolume");
        postVolume = volGO.AddComponent<Volume>();
        postVolume.isGlobal = true;
        postVolume.priority = 100f;

        postProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        postVolume.profile = postProfile;

        // Aberración cromática — simula visión desenfocada/confusa
        chromaticAberration = postProfile.Add<ChromaticAberration>();
        chromaticAberration.active = true;
        chromaticAberration.intensity.Override(0.75f);

        // Distorsión de lente — efecto de aturdimiento leve
        lensDistortion = postProfile.Add<LensDistortion>();
        lensDistortion.active = true;
        lensDistortion.intensity.Override(-0.22f);
        lensDistortion.xMultiplier.Override(0.6f);
        lensDistortion.yMultiplier.Override(0.6f);
        lensDistortion.scale.Override(1.05f);

        // Ajuste de color — ligeramente desaturado al despertar
        colorAdjustments = postProfile.Add<ColorAdjustments>();
        colorAdjustments.active = true;
        colorAdjustments.saturation.Override(-25f);
    }

    private IEnumerator ClearEffects(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            float progress = t / duration;
            float e = 1f - (1f - progress) * (1f - progress); // ease-out

            if (chromaticAberration != null)
                chromaticAberration.intensity.Override(Mathf.Lerp(0.75f, 0f, e));

            if (lensDistortion != null)
                lensDistortion.intensity.Override(Mathf.Lerp(-0.22f, 0f, e));

            if (colorAdjustments != null)
                colorAdjustments.saturation.Override(Mathf.Lerp(-25f, 0f, e));

            t += Time.deltaTime;
            yield return null;
        }

        if (chromaticAberration != null) chromaticAberration.intensity.Override(0f);
        if (lensDistortion != null)      lensDistortion.intensity.Override(0f);
        if (colorAdjustments != null)    colorAdjustments.saturation.Override(0f);
    }

    // ─────────────────────────────────────────
    // UI
    // ─────────────────────────────────────────
    private void BuildUI()
    {
        GameObject canvasGO = new GameObject("CinematicCanvas");
        cinCanvas = canvasGO.AddComponent<Canvas>();
        cinCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        cinCanvas.sortingOrder = 200;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        blinkTop    = MakeEyelid(canvasGO.transform, top: true);
        blinkBottom = MakeEyelid(canvasGO.transform, top: false);
        SetEyelids(0f);

        dialoguePanel = BuildDialoguePanel(canvasGO.transform);
        dialoguePanel.SetActive(false);

        GameObject fadeGO = new GameObject("FadePanel");
        fadeGO.transform.SetParent(canvasGO.transform, false);
        RectTransform fr = fadeGO.AddComponent<RectTransform>();
        fr.anchorMin = Vector2.zero;
        fr.anchorMax = Vector2.one;
        fr.sizeDelta = Vector2.zero;
        fadeImage = fadeGO.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);

        if (canSkip) BuildSkipHint(canvasGO.transform);
    }

    private void BuildSkipHint(Transform parent)
    {
        GameObject hint = new GameObject("SkipHint");
        hint.transform.SetParent(parent, false);
        RectTransform rt = hint.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot     = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-40f, 40f);
        rt.sizeDelta = new Vector2(270f, 46f);

        CanvasGroup cg = hint.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        hint.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        // Key box
        GameObject keyBox = new GameObject("KeyBox");
        keyBox.transform.SetParent(hint.transform, false);
        RectTransform kr = keyBox.AddComponent<RectTransform>();
        kr.anchorMin = new Vector2(0f, 0.5f); kr.anchorMax = new Vector2(0f, 0.5f);
        kr.pivot = new Vector2(0f, 0.5f);
        kr.anchoredPosition = new Vector2(10f, 0f);
        kr.sizeDelta = new Vector2(112f, 30f);
        keyBox.AddComponent<Image>().color = new Color(0.88f, 0.88f, 0.88f, 1f);

        GameObject keyTxtGO = new GameObject("KeyTxt");
        keyTxtGO.transform.SetParent(keyBox.transform, false);
        RectTransform ktr = keyTxtGO.AddComponent<RectTransform>();
        ktr.anchorMin = Vector2.zero; ktr.anchorMax = Vector2.one; ktr.sizeDelta = Vector2.zero;
        TextMeshProUGUI keyTmp = keyTxtGO.AddComponent<TextMeshProUGUI>();
        keyTmp.text = "ESPACIO"; keyTmp.fontSize = 14f; keyTmp.fontStyle = FontStyles.Bold;
        keyTmp.alignment = TextAlignmentOptions.Center;
        keyTmp.color = new Color(0.1f, 0.1f, 0.1f, 1f);

        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(hint.transform, false);
        RectTransform lr = labelGO.AddComponent<RectTransform>();
        lr.anchorMin = new Vector2(0f, 0f); lr.anchorMax = new Vector2(1f, 1f);
        lr.offsetMin = new Vector2(132f, 0f); lr.offsetMax = new Vector2(-10f, 0f);
        TextMeshProUGUI lbl = labelGO.AddComponent<TextMeshProUGUI>();
        lbl.text = "para saltar"; lbl.fontSize = 16f;
        lbl.alignment = TextAlignmentOptions.Left;
        lbl.color = new Color(0.85f, 0.85f, 0.85f, 1f);

        StartCoroutine(FadeCanvasGroup(hint, 0f, 1f, 0.6f));
    }

    private RectTransform MakeEyelid(Transform parent, bool top)
    {
        GameObject go = new GameObject(top ? "BlinkTop" : "BlinkBottom");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = top ? new Vector2(0f, 1f) : new Vector2(0f, 0f);
        rt.anchorMax = top ? new Vector2(1f, 1f) : new Vector2(1f, 0f);
        rt.pivot     = top ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(0f, 540f);
        go.AddComponent<Image>().color = Color.black;
        return rt;
    }

    private GameObject BuildDialoguePanel(Transform parent)
    {
        GameObject dp = new GameObject("DialoguePanel");
        dp.transform.SetParent(parent, false);
        RectTransform dpr = dp.AddComponent<RectTransform>();
        dpr.anchorMin = new Vector2(0f, 0f);
        dpr.anchorMax = new Vector2(1f, 0f);
        dpr.pivot     = new Vector2(0.5f, 0f);
        dpr.sizeDelta = new Vector2(0f, 210f);
        dp.AddComponent<CanvasGroup>().alpha = 0f;
        dp.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.10f, 0.92f);

        GameObject acc = new GameObject("Accent");
        acc.transform.SetParent(dp.transform, false);
        RectTransform ar = acc.AddComponent<RectTransform>();
        ar.anchorMin = new Vector2(0f, 0f); ar.anchorMax = new Vector2(0f, 1f);
        ar.pivot = new Vector2(0f, 0.5f);   ar.sizeDelta = new Vector2(6f, 0f);
        acc.AddComponent<Image>().color = new Color(0.95f, 0.75f, 0.15f, 1f);

        GameObject nameGO = new GameObject("NameLabel");
        nameGO.transform.SetParent(dp.transform, false);
        RectTransform nr = nameGO.AddComponent<RectTransform>();
        nr.anchorMin = new Vector2(0f, 1f); nr.anchorMax = new Vector2(0.5f, 1f);
        nr.pivot = new Vector2(0f, 1f);
        nr.anchoredPosition = new Vector2(20f, -8f);
        nr.sizeDelta = new Vector2(0f, 34f);
        TextMeshProUGUI nameTxt = nameGO.AddComponent<TextMeshProUGUI>();
        nameTxt.text = "<color=#F5C518><b>???</b></color>";
        nameTxt.fontSize = 22f;
        nameTxt.alignment = TextAlignmentOptions.Left;

        GameObject sep = new GameObject("Sep");
        sep.transform.SetParent(dp.transform, false);
        RectTransform sr = sep.AddComponent<RectTransform>();
        sr.anchorMin = new Vector2(0f, 1f); sr.anchorMax = new Vector2(1f, 1f);
        sr.pivot = new Vector2(0.5f, 1f);
        sr.anchoredPosition = new Vector2(0f, -42f);
        sr.sizeDelta = new Vector2(-20f, 2f);
        sep.AddComponent<Image>().color = new Color(0.95f, 0.75f, 0.15f, 0.3f);

        GameObject textGO = new GameObject("DialogueText");
        textGO.transform.SetParent(dp.transform, false);
        RectTransform tr = textGO.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(22f, 14f);
        tr.offsetMax = new Vector2(-22f, -54f);
        dialogueText = textGO.AddComponent<TextMeshProUGUI>();
        dialogueText.fontSize  = 30f;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;
        dialogueText.color     = new Color(0.96f, 0.96f, 0.92f, 1f);
        dialogueText.text      = "";

        return dp;
    }

    // ─────────────────────────────────────────
    // SECUENCIA PRINCIPAL
    // ─────────────────────────────────────────
    private IEnumerator RunCinematic()
    {
        yield return null;
        AudioManager.Instance?.PauseMusic();

        if (pitidoClip != null)
        {
            audioSource.clip   = pitidoClip;
            audioSource.loop   = false;
            audioSource.volume = 0.55f;
            audioSource.Play();
        }

        // 2 segundos en negro
        yield return new WaitForSeconds(2f);

        // Arrancar limpieza de efectos en paralelo con el pestañeo (~3s)
        StartCoroutine(ClearEffects(3.2f));

        // Pestañeo lento
        yield return StartCoroutine(BlinkSequence());

        // Diálogo
        dialoguePanel.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(dialoguePanel, 0f, 1f, 0.25f));

        string[] lines = { "¿Dónde estoy?", "Tengo que salir de aquí..." };
        yield return StartCoroutine(Typewriter(lines, 3f));

        yield return new WaitForSeconds(2f);

        // Fade a negro lento
        yield return StartCoroutine(FadeImg(0f, 1f, 4f));

        if (playerScript != null)
            playerScript.cinematicMode = false;
        AudioManager.Instance?.ResumeMusic();

        yield return StartCoroutine(FadeImg(1f, 0f, 0.8f));

        PlayerPrefs.SetInt("CinematicSeen", 1);
        PlayerPrefs.Save();

        if (playerCanvas != null) playerCanvas.SetActive(true);
        Cleanup();
    }

    // ─────────────────────────────────────────
    // SKIP
    // ─────────────────────────────────────────
    private void Skip()
    {
        skipped = true;
        StopAllCoroutines();

        if (chromaticAberration != null) chromaticAberration.intensity.Override(0f);
        if (lensDistortion      != null) lensDistortion.intensity.Override(0f);
        if (colorAdjustments    != null) colorAdjustments.saturation.Override(0f);

        if (playerScript != null) playerScript.cinematicMode = false;

        PlayerPrefs.SetInt("CinematicSeen", 1);
        PlayerPrefs.Save();

        StartCoroutine(SkipFade());
    }

    private IEnumerator SkipFade()
    {
        float startAlpha = fadeImage != null ? fadeImage.color.a : 0f;
        float t = 0f, dur = 0.35f;
        while (t < dur)
        {
            if (fadeImage != null)
                fadeImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(startAlpha, 1f, t / dur));
            t += Time.deltaTime;
            yield return null;
        }

        if (playerCanvas != null) playerCanvas.SetActive(true);
        AudioManager.Instance?.ResumeMusic();

        yield return StartCoroutine(FadeImg(1f, 0f, 0.4f));
        Cleanup();
    }

    // ─────────────────────────────────────────
    // PESTAÑEO
    // ─────────────────────────────────────────
    private IEnumerator BlinkSequence()
    {
        yield return StartCoroutine(AnimateEyelids(0f, 1f, 1.8f, smooth: true));

        yield return StartCoroutine(AnimateEyelids(1f, 0f, 0.15f, smooth: false));
        yield return StartCoroutine(AnimateEyelids(0f, 1f, 0.40f, smooth: true));

        StartCoroutine(TurnPlayer());

        yield return StartCoroutine(AnimateEyelids(1f, 0f, 0.15f, smooth: false));
        yield return StartCoroutine(AnimateEyelids(0f, 1f, 0.45f, smooth: true));

        yield return new WaitForSeconds(0.15f);
    }

    private void SetEyelids(float open)
    {
        blinkTop.anchoredPosition    = new Vector2(0f, Mathf.Lerp(0f,  540f, open));
        blinkBottom.anchoredPosition = new Vector2(0f, Mathf.Lerp(0f, -540f, open));
    }

    private IEnumerator AnimateEyelids(float from, float to, float dur, bool smooth)
    {
        float t = 0f;
        while (t < dur)
        {
            float n = t / dur;
            float e = smooth ? n * (2f - n) : n;
            SetEyelids(Mathf.Lerp(from, to, e));
            t += Time.deltaTime;
            yield return null;
        }
        SetEyelids(to);
    }

    // ─────────────────────────────────────────
    // GIRO DEL PLAYER
    // ─────────────────────────────────────────
    private IEnumerator TurnPlayer()
    {
        if (player == null) yield break;
        float from = 90f, to = -90f, dur = 0.55f, t = 0f;
        while (t < dur)
        {
            float e = t / dur; e = e * e;
            player.rotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(from, to, e));
            t += Time.deltaTime;
            yield return null;
        }
        player.rotation = Quaternion.Euler(0f, 0f, to);
    }

    // ─────────────────────────────────────────
    // TYPEWRITER
    // ─────────────────────────────────────────
    private IEnumerator Typewriter(string[] lines, float totalSeconds)
    {
        if (keyboardClip != null)
            audioSource.PlayOneShot(keyboardClip, 0.7f);

        int total = 0;
        foreach (string l in lines) total += l.Length;
        float pause    = (lines.Length - 1) * 0.45f;
        float charTime = (totalSeconds - pause) / Mathf.Max(total, 1);

        string shown = "";
        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0)
            {
                yield return new WaitForSeconds(0.45f);
                shown += "\n";
            }
            foreach (char c in lines[i])
            {
                shown += c;
                dialogueText.text = shown;
                yield return new WaitForSeconds(charTime);
            }
        }
    }

    // ─────────────────────────────────────────
    // UTILIDADES
    // ─────────────────────────────────────────
    private IEnumerator FadeCanvasGroup(GameObject go, float from, float to, float dur)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        float t = 0f;
        while (t < dur)
        {
            cg.alpha = Mathf.Lerp(from, to, t / dur);
            t += Time.deltaTime;
            yield return null;
        }
        cg.alpha = to;
    }

    private IEnumerator FadeImg(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            fadeImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, t / dur));
            t += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = new Color(0f, 0f, 0f, to);
    }

    private void Cleanup()
    {
        if (postVolume != null)    Destroy(postVolume.gameObject);
        if (postProfile != null)   Destroy(postProfile);
        if (cinCanvas != null)     Destroy(cinCanvas.gameObject);
        Destroy(gameObject);
    }
}
