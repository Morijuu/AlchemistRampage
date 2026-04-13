using System.Collections.Generic;
using UnityEngine;

public class AreaEffect : MonoBehaviour
{
    private int tickDamage;
    private float tickRate;
    private float duration;
    private bool isEnemySource;

    private float tickTimer;
    private float lifeTimer;
    private SpriteRenderer sr;

    private readonly HashSet<Health> inZone = new HashSet<Health>();

    public void Initialize(int tickDamage, float tickRate, float duration, bool isEnemySource)
    {
        this.tickDamage    = tickDamage;
        this.tickRate      = tickRate;
        this.duration      = duration;
        this.isEnemySource = isEnemySource;
    }

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        tickTimer += Time.deltaTime;

        if (tickTimer >= tickRate)
        {
            tickTimer = 0f;
            inZone.RemoveWhere(h => h == null);
            foreach (Health h in new List<Health>(inZone))
                h.TakeDamage(tickDamage);
        }

        // Fade out en los últimos 1.5 segundos
        if (sr != null && lifeTimer >= duration - 1.5f)
        {
            float alpha = Mathf.Lerp(1f, 0f, (lifeTimer - (duration - 1.5f)) / 1.5f);
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        if (lifeTimer >= duration)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isEnemySource && other.CompareTag("Enemy")) return;
        if (!isEnemySource && other.CompareTag("Player")) return;

        Health h = other.GetComponent<Health>();
        if (h != null) inZone.Add(h);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Health h = other.GetComponent<Health>();
        if (h != null) inZone.Remove(h);
    }
}
