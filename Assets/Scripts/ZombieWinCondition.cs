using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ZombieWinCondition : MonoBehaviour
{
    private bool won = false;
    private int initialEnemyCount = 0;
    private TextMeshProUGUI counterText;
    private TextMeshProUGUI iconText;
    private int lastCount = -1;

    private void Start()
    {
        initialEnemyCount = CountEnemies();
        CreateCounterUI();
        StartCoroutine(CheckEnemies());
    }

    private int CountEnemies()
    {
        int count = FindObjectsByType<EnemyChase>(FindObjectsSortMode.None).Length;
        count += FindObjectsByType<EnemyThrowerChase>(FindObjectsSortMode.None).Length;
        return count;
    }

    private void CreateCounterUI()
    {
        GameObject canvasGO = new GameObject("ZombieCounterCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Panel de fondo
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(canvasGO.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-24f, -24f);
        panelRect.sizeDelta = new Vector2(210f, 72f);
        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.05f, 0.78f);

        // Borde izquierdo de color rojo
        GameObject border = new GameObject("Border");
        border.transform.SetParent(panel.transform, false);
        RectTransform borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0f, 0f);
        borderRect.anchorMax = new Vector2(0f, 1f);
        borderRect.pivot = new Vector2(0f, 0.5f);
        borderRect.anchoredPosition = new Vector2(0f, 0f);
        borderRect.sizeDelta = new Vector2(6f, 0f);
        Image borderImg = border.AddComponent<Image>();
        borderImg.color = new Color(0.85f, 0.15f, 0.1f, 1f);

        // Icono calavera
        GameObject iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(panel.transform, false);
        RectTransform iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0f);
        iconRect.anchorMax = new Vector2(0f, 1f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(14f, 0f);
        iconRect.sizeDelta = new Vector2(48f, 0f);
        iconText = iconGO.AddComponent<TextMeshProUGUI>();
        iconText.text = "☠";
        iconText.fontSize = 30f;
        iconText.alignment = TextAlignmentOptions.Center;
        iconText.color = new Color(0.85f, 0.15f, 0.1f, 1f);

        // Texto contador
        GameObject textGO = new GameObject("CounterText");
        textGO.transform.SetParent(panel.transform, false);
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.offsetMin = new Vector2(62f, 4f);
        textRect.offsetMax = new Vector2(-10f, -4f);
        counterText = textGO.AddComponent<TextMeshProUGUI>();
        counterText.fontSize = 22f;
        counterText.fontStyle = FontStyles.Bold;
        counterText.alignment = TextAlignmentOptions.Left;
        counterText.color = Color.white;

        SetCounterText(initialEnemyCount);
    }

    private void SetCounterText(int count)
    {
        if (counterText == null) return;
        string color = count > 0 ? "#FF4422" : "#44FF88";
        counterText.text = $"<color={color}><b>{count}</b></color> <size=16>ZOMBIES\nRESTANTES</size>";
        lastCount = count;
    }

    private IEnumerator CheckEnemies()
    {
        while (!won)
        {
            yield return new WaitForSeconds(0.5f);

            if (UIManager.Instance == null || UIManager.Instance.AnyMenuOpen()) continue;

            int remaining = CountEnemies();

            if (remaining != lastCount)
                SetCounterText(remaining);

            if (initialEnemyCount > 0 && remaining == 0)
            {
                won = true;
                UIManager.Instance.ShowWin();
            }
        }
    }
}
