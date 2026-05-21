using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to gameOverPanel. Clears its children and rebuilds with a polished defeat UI.
public class GameOverPanelBuilder : MonoBehaviour
{
    private void Awake()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        Build();
    }

    private void Build()
    {
        Image bg = gameObject.AddComponent<Image>();
        bg.color = new Color(0.04f, 0f, 0f, 0.93f);

        // Center panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(transform, false);
        RectTransform pr = panel.AddComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.5f, 0.5f);
        pr.anchorMax = new Vector2(0.5f, 0.5f);
        pr.pivot     = new Vector2(0.5f, 0.5f);
        pr.sizeDelta = new Vector2(620f, 420f);
        panel.AddComponent<Image>().color = new Color(0.06f, 0.03f, 0.03f, 0.97f);

        // Red top border
        GameObject topBorder = MakeRect("TopBorder", panel.transform,
            new Vector2(0f,1f), new Vector2(1f,1f), new Vector2(0.5f,1f),
            new Vector2(0f, 6f), new Vector2(0f, 0f));
        topBorder.AddComponent<Image>().color = new Color(0.85f, 0.1f, 0.1f, 1f);

        GameObject botBorder = MakeRect("BotBorder", panel.transform,
            new Vector2(0f,0f), new Vector2(1f,0f), new Vector2(0.5f,0f),
            new Vector2(0f, 6f), new Vector2(0f, 0f));
        botBorder.AddComponent<Image>().color = new Color(0.85f, 0.1f, 0.1f, 1f);

        // "HAS MUERTO" title
        AddText(panel.transform, "HAS MUERTO", 68f, FontStyles.Bold,
            new Color(0.9f, 0.15f, 0.15f, 1f),
            new Vector2(0f, 70f), new Vector2(580f, 90f));

        AddText(panel.transform, "No lo has conseguido esta vez", 24f, FontStyles.Normal,
            new Color(0.65f, 0.65f, 0.65f, 1f),
            new Vector2(0f, 10f), new Vector2(500f, 36f));

        // Separator
        GameObject sep = MakeRect("Sep", panel.transform,
            new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f),
            new Vector2(400f, 2f), new Vector2(0f, -30f));
        sep.AddComponent<Image>().color = new Color(0.85f, 0.1f, 0.1f, 0.4f);

        MakeButton(panel.transform, "REINTENTAR", new Vector2(0f, -100f), new Vector2(260f, 58f),
            new Color(0.75f, 0.1f, 0.1f, 1f), Color.white,
            () => FindFirstObjectByType<UIManager>()?.Retry());

        MakeButton(panel.transform, "MENÚ PRINCIPAL", new Vector2(0f, -172f), new Vector2(260f, 58f),
            new Color(0.14f, 0.08f, 0.08f, 1f), new Color(0.85f, 0.85f, 0.85f, 1f),
            () => FindFirstObjectByType<UIManager>()?.Retry());
    }

    private GameObject MakeRect(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 anchoredPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        return go;
    }

    private void AddText(Transform parent, string text, float fontSize, FontStyles style, Color color,
        Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject("Text_" + text.Substring(0, Mathf.Min(6, text.Length)));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = sizeDelta;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = color;
    }

    private void MakeButton(Transform parent, string label, Vector2 anchoredPos, Vector2 size,
        Color bgColor, Color textColor, System.Action onClick)
    {
        GameObject go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => onClick?.Invoke());
        go.AddComponent<UIButtonAnimator>();

        AddText(go.transform, label, 22f, FontStyles.Bold, textColor, Vector2.zero, size);
    }
}
