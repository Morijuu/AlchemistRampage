using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to any GameObject in InicioScene.
// Adds UIButtonAnimator to every Button and restykes them to match the in-game panel style.
public class MenuAutoSetup : MonoBehaviour
{
    private void Start()
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
}
