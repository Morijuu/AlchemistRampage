using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private static readonly float BULLET_LIFETIME = 5f;

    private BulletData data;
    private bool isEnemyBullet;
    private bool isFragment;
    private int bounceCount;

    private Rigidbody2D rb;
    private Transform homingTarget;
    private float spawnTime;

    public void Initialize(BulletData bulletData, bool enemyBullet = false, bool fragment = false)
    {
        data = bulletData;
        isEnemyBullet = enemyBullet;
        isFragment = fragment;
        bounceCount = 0;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        Destroy(gameObject, BULLET_LIFETIME);

        spawnTime = Time.time;

        if (data.bulletType == BulletType.Target)
            homingTarget = FindNearestEnemy();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (data == null) return;
        if (data.bulletType == BulletType.Target)
            SteerTowardTarget();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (data == null) return;
        if (!isEnemyBullet && other.CompareTag("Player")) return;
        if (isEnemyBullet && other.CompareTag("Enemy")) return;

        Health health = other.GetComponent<Health>();

        if (data.bulletType == BulletType.Bouncy)
        {
            if (health != null)
                health.TakeDamage(data.damage);

            bounceCount++;
            if (bounceCount >= 3)
            {
                Destroy(gameObject);
                return;
            }

            Vector2 closestPoint = other.ClosestPoint(transform.position);
            Vector2 normal = ((Vector2)transform.position - closestPoint).normalized;
            if (normal == Vector2.zero) normal = -rb.linearVelocity.normalized;
            rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, normal);
            return;
        }

        if (health != null)
        {
            health.TakeDamage(data.damage);
            HandleHitEffect(transform.position, other);
            Destroy(gameObject);
            return;
        }

        // Pared u obstáculo sin Health
        HandleHitEffect(transform.position, null);
        Destroy(gameObject);
    }

    private void HandleHitEffect(Vector3 hitPosition, Collider2D hitCollider)
    {
        if (data == null) return;
        switch (data.bulletType)
        {
            case BulletType.Area:
                DoSplash(hitPosition);
                break;
            case BulletType.Frag:
                if (!isFragment) DoFrag(hitPosition, hitCollider);
                break;
        }
    }

    private void DoSplash(Vector3 center)
    {
        if (data.areaEffectPrefab == null)
        {
            Debug.LogWarning("Area bullet: areaEffectPrefab no asignado en BulletData.");
            return;
        }

        GameObject zone = Instantiate(data.areaEffectPrefab, center, Quaternion.identity);
        AreaEffect effect = zone.GetComponent<AreaEffect>();
        if (effect != null)
            effect.Initialize(data.splashDamage, data.areaTickRate, data.areaDuration, isEnemyBullet);
    }

    private void DoFrag(Vector3 origin, Collider2D ignoreCollider)
    {
        GameObject prefab = BulletInventory.Instance?.bulletPrefab;
        if (prefab == null) return;

        float angleStep = 360f / data.fragCount;
        for (int i = 0; i < data.fragCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            // Spawn desplazado para no colisionar inmediatamente con el enemigo golpeado
            Vector3 spawnPos = origin + (Vector3)(dir * 0.4f);

            GameObject frag = Instantiate(prefab, spawnPos, Quaternion.identity);
            BulletScript fragScript = frag.GetComponent<BulletScript>();
            fragScript.Initialize(data, isEnemyBullet, fragment: true);

            // Ignorar el collider golpeado para que los fragmentos no impacten inmediatamente
            if (ignoreCollider != null)
            {
                Collider2D fragCol = frag.GetComponent<Collider2D>();
                if (fragCol != null)
                    Physics2D.IgnoreCollision(fragCol, ignoreCollider);
            }

            Rigidbody2D fragRb = frag.GetComponent<Rigidbody2D>();
            if (fragRb != null) fragRb.linearVelocity = dir * data.fragSpeed;
        }
    }

    // Target: curva hacia el enemigo pero con margen de error (no tracking perfecto)
    private void SteerTowardTarget()
    {
        if (homingTarget == null) homingTarget = FindNearestEnemy();
        if (homingTarget == null) return;

        // Vuela recto durante el delay inicial
        if (Time.time - spawnTime < data.homingDelay) return;

        float dist = Vector2.Distance(rb.position, homingTarget.position);

        // Deja de corregir cuando está cerca — vuela recto y puede fallar
        if (dist <= data.homingStopDistance) return;

        Vector2 toTarget = ((Vector2)homingTarget.position - rb.position).normalized;

        float currentAngle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        float targetAngle  = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        float newAngle     = Mathf.MoveTowardsAngle(currentAngle, targetAngle,
                                                     data.homingTurnSpeed * Time.fixedDeltaTime)
                             * Mathf.Deg2Rad;

        rb.linearVelocity = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle)) * data.speed;
    }

    private Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float minDist = float.MaxValue;
        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist) { minDist = dist; nearest = enemy.transform; }
        }
        return nearest;
    }
}
