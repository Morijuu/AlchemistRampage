using System.Collections;
using UnityEngine;

// Tracer effect for the Regular bullet raycast:
//   - thin white core line  (tapered, fades in 0.15s)
//   - wide soft-blue glow   (tapered, fades faster)
//   - warm muzzle flash     (expands + fades at origin)
//   - white impact flash    (expands + fades at hit point)
public class RaycastLineEffect : MonoBehaviour
{
    private LineRenderer    core;
    private LineRenderer    glow;
    private SpriteRenderer  muzzleSR;
    private SpriteRenderer  impactSR;
    private Coroutine       anim;

    private void Awake()
    {
        Sprite circle = MakeSoftCircle(64);

        // Core — crisp white tracer
        core = gameObject.AddComponent<LineRenderer>();
        core.positionCount  = 2;
        core.useWorldSpace  = true;
        core.startWidth     = 0.07f;
        core.endWidth       = 0.01f;
        core.material       = new Material(Shader.Find("Sprites/Default"));
        core.startColor     = Color.white;
        core.endColor       = new Color(0.85f, 0.93f, 1f, 0f);
        core.sortingOrder   = 11;
        core.enabled        = false;

        // Glow — wide soft blue-white halo
        GameObject glowGO = new GameObject("RayGlow");
        glowGO.transform.SetParent(transform, false);
        glow = glowGO.AddComponent<LineRenderer>();
        glow.positionCount  = 2;
        glow.useWorldSpace  = true;
        glow.startWidth     = 0.26f;
        glow.endWidth       = 0.05f;
        glow.material       = new Material(Shader.Find("Sprites/Default"));
        glow.startColor     = new Color(0.72f, 0.87f, 1f, 0.30f);
        glow.endColor       = new Color(0.72f, 0.87f, 1f, 0f);
        glow.sortingOrder   = 10;
        glow.enabled        = false;

        // Muzzle flash
        GameObject mGO = new GameObject("MuzzleFlash");
        mGO.transform.SetParent(transform, false);
        muzzleSR = mGO.AddComponent<SpriteRenderer>();
        muzzleSR.sprite       = circle;
        muzzleSR.color        = new Color(1f, 0.94f, 0.78f, 1f);
        muzzleSR.sortingOrder = 12;
        mGO.SetActive(false);

        // Impact flash
        GameObject iGO = new GameObject("ImpactFlash");
        iGO.transform.SetParent(transform, false);
        impactSR = iGO.AddComponent<SpriteRenderer>();
        impactSR.sprite       = circle;
        impactSR.color        = Color.white;
        impactSR.sortingOrder = 12;
        iGO.SetActive(false);
    }

    public void Fire(Vector2 from, Vector2 to)
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(Animate(from, to));
    }

    private IEnumerator Animate(Vector2 from, Vector2 to)
    {
        Vector3 f3 = new Vector3(from.x, from.y, 0f);
        Vector3 t3 = new Vector3(to.x,   to.y,   0f);

        core.SetPosition(0, f3); core.SetPosition(1, t3);
        glow.SetPosition(0, f3); glow.SetPosition(1, t3);
        core.enabled = true;
        glow.enabled = true;

        muzzleSR.transform.position = f3;
        muzzleSR.gameObject.SetActive(true);

        impactSR.transform.position = t3;
        impactSR.gameObject.SetActive(true);

        float dur = 0.16f, t = 0f;
        while (t < dur)
        {
            float p      = t / dur;
            float eased  = 1f - p * p;   // fast fade at start, slow tail

            // Lines fade out
            core.startColor = new Color(1f,    1f,    1f,    eased);
            core.endColor   = new Color(0.85f, 0.93f, 1f,    0f);
            glow.startColor = new Color(0.72f, 0.87f, 1f,    0.30f * eased);
            glow.endColor   = new Color(0.72f, 0.87f, 1f,    0f);

            // Muzzle: quick burst, gone in first 40 % of duration
            float mp = Mathf.Clamp01(p / 0.40f);
            muzzleSR.transform.localScale = Vector3.one * Mathf.Lerp(0.20f, 0.55f, mp);
            muzzleSR.color = new Color(1f, 0.94f, 0.78f, 1f - mp);

            // Impact: slower expand and fade
            impactSR.transform.localScale = Vector3.one * Mathf.Lerp(0.12f, 0.42f, p);
            impactSR.color = new Color(1f, 1f, 1f, (1f - p) * 0.85f);

            t += Time.deltaTime;
            yield return null;
        }

        core.enabled = false;
        glow.enabled = false;
        muzzleSR.gameObject.SetActive(false);
        impactSR.gameObject.SetActive(false);
    }

    // Soft radial gradient circle texture
    private static Sprite MakeSoftCircle(int size)
    {
        Texture2D tex    = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[]   pixels = new Color[size * size];
        float     c      = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                float a = Mathf.Clamp01(1f - d);
                pixels[y * size + x] = new Color(1f, 1f, 1f, a * a);
            }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
    }
}
