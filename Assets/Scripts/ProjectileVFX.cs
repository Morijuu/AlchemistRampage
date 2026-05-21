using System.Collections;
using UnityEngine;

// Add to any bullet GameObject to get a colored trail, optional spin, and impact burst.
// For BulletScript-managed bullets: call Setup() then PlayImpact() from Explode().
// For standalone enemy projectiles: call Setup(standaloneMode:true) and it self-handles.
public class ProjectileVFX : MonoBehaviour
{
    private TrailRenderer trail;
    private bool  spinEnabled;
    private float spinSpeed;
    private bool  standalone;
    private bool  played;

    private void Awake()
    {
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time           = 0.20f;
        trail.startWidth     = 0.16f;
        trail.endWidth       = 0f;
        trail.numCapVertices = 3;
        trail.material       = new Material(Shader.Find("Sprites/Default"));
        trail.sortingOrder   = -1;
        trail.startColor     = Color.white;
        trail.endColor       = new Color(1f, 1f, 1f, 0f);
    }

    public void Setup(Color color, bool spin, bool standaloneMode = false, float spinSpd = 420f)
    {
        standalone  = standaloneMode;
        spinEnabled = spin;
        spinSpeed   = spinSpd;

        trail.startColor = color;
        trail.endColor   = new Color(color.r, color.g, color.b, 0f);
    }

    private void Update()
    {
        if (spinEnabled)
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other) => HandleStandaloneHit(other.gameObject);
    private void OnCollisionEnter2D(Collision2D col)  => HandleStandaloneHit(col.gameObject);

    private void HandleStandaloneHit(GameObject other)
    {
        if (!standalone || played) return;
        if (other.CompareTag("Pickup") || other.CompareTag("Enemy")) return;
        if (other.layer == LayerMask.NameToLayer("PickupLayer")) return;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        TriggerImpact(0.40f);
        Destroy(gameObject, 0.40f);
    }

    // Called externally by BulletScript.Explode()
    public void PlayImpact(float duration)
    {
        if (played) return;
        TriggerImpact(duration);
    }

    private void TriggerImpact(float duration)
    {
        played      = true;
        spinEnabled = false;

        if (trail != null) trail.emitting = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            StartCoroutine(ImpactAnim(sr, duration));
    }

    private IEnumerator ImpactAnim(SpriteRenderer sr, float duration)
    {
        Vector3 s0 = transform.localScale;
        Color   c0 = sr.color;
        float   t  = 0f;

        while (t < duration)
        {
            float p = t / duration;
            transform.localScale = Vector3.Lerp(s0, s0 * 1.9f, p);
            sr.color = new Color(c0.r, c0.g, c0.b, 1f - p);
            t += Time.deltaTime;
            yield return null;
        }

        sr.color = new Color(c0.r, c0.g, c0.b, 0f);
    }
}
