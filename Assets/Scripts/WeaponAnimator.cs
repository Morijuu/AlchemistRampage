using System.Collections;
using UnityEngine;

public class WeaponAnimator : MonoBehaviour
{
    private Vector3   baseScale;
    private Coroutine current;
    private bool      visible = false;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    public void SetVisible(bool show)
    {
        if (show == visible) return;
        visible = show;

        if (current != null) StopCoroutine(current);

        if (show)
        {
            gameObject.SetActive(true);   // must activate before StartCoroutine
            current = StartCoroutine(AnimateIn());
        }
        else
        {
            current = StartCoroutine(AnimateOut());
        }
    }

    // Bounce-in: 0 → 1.3 → 1
    private IEnumerator AnimateIn()
    {
        float t = 0f, dur = 0.22f;
        while (t < dur)
        {
            float n = t / dur;
            float s = n < 0.65f
                ? Mathf.Lerp(0f, 1.30f, n / 0.65f)
                : Mathf.Lerp(1.30f, 1f,  (n - 0.65f) / 0.35f);
            transform.localScale = baseScale * s;
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = baseScale;
    }

    // Quick shrink-out with slight overshoot before disappearing
    private IEnumerator AnimateOut()
    {
        float t = 0f, dur = 0.14f;
        while (t < dur)
        {
            float n  = t / dur;
            float s  = 1f - n * n;   // ease-in shrink
            transform.localScale = baseScale * s;
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = baseScale;
        gameObject.SetActive(false);
    }

    // Short punch scale for shoot recoil — called from PlayerScript
    public void Recoil()
    {
        if (!visible) return;
        if (current != null) StopCoroutine(current);
        current = StartCoroutine(RecoilAnim());
    }

    private IEnumerator RecoilAnim()
    {
        float t = 0f, dur = 0.12f;
        while (t < dur)
        {
            float n = t / dur;
            // Squash on X (kick back), stretch back
            float sx = n < 0.4f ? Mathf.Lerp(1f, 0.6f, n / 0.4f)
                                : Mathf.Lerp(0.6f, 1f, (n - 0.4f) / 0.6f);
            float sy = n < 0.4f ? Mathf.Lerp(1f, 1.3f, n / 0.4f)
                                : Mathf.Lerp(1.3f, 1f, (n - 0.4f) / 0.6f);
            transform.localScale = new Vector3(baseScale.x * sx, baseScale.y * sy, baseScale.z);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = baseScale;
    }
}
