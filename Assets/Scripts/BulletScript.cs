using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private static readonly float BULLET_LIFETIME = 5f;

    private BulletData data;
    private bool isEnemyBullet;
    private bool isFragment;
    private int bounceCount;
    private int pierceCount;

    private Rigidbody2D rb;
    private Transform homingTarget;
    private float spawnTime;

    private Vector2 previousPosition;
    private Collider2D ownCollider;

    public void Initialize(BulletData bulletData, bool enemyBullet = false, bool fragment = false)
    {
        data = bulletData;
        isEnemyBullet = enemyBullet;
        isFragment = fragment;
        bounceCount = 0;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        ownCollider = col;

        Destroy(gameObject, BULLET_LIFETIME);

        spawnTime = Time.time;

        if (data.bulletType == BulletType.Target)
            homingTarget = FindNearestEnemy();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        previousPosition = rb.position;
    }

    private void FixedUpdate()
    {
        if (data == null) return;
        if (data.bulletType == BulletType.Target)
            SteerTowardTarget();
        if (data.bulletType == BulletType.Bouncy)
            CheckBouncyRaycast();
        previousPosition = rb.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (data == null) return;
        if (!isEnemyBullet && other.CompareTag("Player")) return;
        if (isEnemyBullet && other.CompareTag("Enemy")) return;

        Health health = other.GetComponent<Health>();

        if (data.bulletType == BulletType.Bouncy) return;

        if (data.bulletType == BulletType.Piercing)
        {
            if (health != null)
            {
                health.TakeDamage(data.damage);
                pierceCount++;
                if (pierceCount >= data.maxPierceCount)
                    Destroy(gameObject);
                return;
            }
            // Pared u obstáculo
            Destroy(gameObject);
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

    private void CheckBouncyRaycast()
    {
        Vector2 movement = rb.position - previousPosition;
        float distance = movement.magnitude;
        if (distance < 0.001f) return;

        RaycastHit2D[] hits = Physics2D.RaycastAll(previousPosition, movement.normalized, distance + 0.1f);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == ownCollider) continue;
            if (!isEnemyBullet && hit.collider.CompareTag("Player")) continue;
            if (isEnemyBullet && hit.collider.CompareTag("Enemy")) continue;

            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(data.damage);

            bounceCount++;
            if (bounceCount >= 3) { Destroy(gameObject); return; }

            rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, hit.normal);
            rb.position = hit.point + hit.normal * 0.05f;
            previousPosition = rb.position;
            return;
        }
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
            case BulletType.Chain:
                if (!isFragment) DoChain(hitPosition, hitCollider);
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

    private void DoChain(Vector3 origin, Collider2D ignoreCollider)
    {
        GameObject prefab = BulletInventory.Instance?.bulletPrefab;
        if (prefab == null) return;

        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        int spawned = 0;

        foreach (GameObject enemy in allEnemies)
        {
            if (spawned >= data.chainCount) break;

            Collider2D enemyCol = enemy.GetComponent<Collider2D>();
            if (ignoreCollider != null && enemyCol == ignoreCollider) continue;

            float dist = Vector2.Distance(origin, enemy.transform.position);
            if (dist > data.chainRange) continue;

            Vector2 dir = ((Vector2)enemy.transform.position - (Vector2)origin).normalized;
            GameObject chain = Instantiate(prefab, origin + (Vector3)(dir * 0.4f), Quaternion.identity);
            BulletScript chainScript = chain.GetComponent<BulletScript>();
            chainScript.Initialize(data, isEnemyBullet, fragment: true);

            Rigidbody2D chainRb = chain.GetComponent<Rigidbody2D>();
            if (chainRb != null) chainRb.linearVelocity = dir * data.speed;

            spawned++;
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
