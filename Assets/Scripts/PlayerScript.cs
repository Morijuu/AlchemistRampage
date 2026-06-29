using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerScript : MonoBehaviour
{
    public GameObject GroundCheck;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    public Animator animator;
    private PlayerInput input;

    [SerializeField] private GameObject sharedBulletPrefab;
    [SerializeField] private Transform bulletSpawn;

    [Header("Raycast Shot (Regular)")]
    [SerializeField] private float raycastMaxDistance = 60f;
    private RaycastLineEffect raycastEffect;

    public float speed;
    public float runspeed;
    bool corriendo;
    public bool agotado = false;

    [HideInInspector] public bool cinematicMode = false;

    public float stamina = 100f;
    public float maxStamina = 100f;

    private Vector2 direccion;
    private Vector2 directionMouse;

    private BulletInventory inventory;
    private float lastShootTime = float.NegativeInfinity;


    private GameObject    weaponObject;
    private WeaponAnimator weaponAnim;
    private Vector3       weaponBaseLocalPos;
    private float         bobTime = 0f;

    // Body squash/stretch
    private Vector3 baseBodyScale;
    private float   bodyScaleX  = 1f;
    private float   bodyScaleY  = 1f;
    private float   breathTime  = 0f;
    private bool    wasMoving   = false;

    private Health health;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        input = GetComponent<PlayerInput>();

        health = GetComponent<Health>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Flag"))
        {
            UIManager.Instance.ShowWin();
        }
    }

    private void Start()
    {
        inventory = BulletInventory.Instance;

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            if (t.CompareTag("Weapon")) { weaponObject = t.gameObject; break; }

        if (weaponObject != null)
        {
            weaponAnim = weaponObject.GetComponent<WeaponAnimator>()
                         ?? weaponObject.AddComponent<WeaponAnimator>();
            weaponBaseLocalPos = weaponObject.transform.localPosition;
        }

        baseBodyScale = transform.localScale;
        raycastEffect = gameObject.AddComponent<RaycastLineEffect>();
    }

    void FixedUpdate()
    {
        if (cinematicMode)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (health != null && health.IsKnocked())
        {
            return;
        }

        direccion = input.actions["Move"].ReadValue<Vector2>();
        corriendo = input.actions["Run"].IsPressed();

        if (stamina <= 0)
        {
            agotado = true;
        }

        if (agotado && stamina >= 30f)
        {
            agotado = false;
        }

        if (corriendo && !agotado)
        {
            stamina -= Time.deltaTime * 10f;
            rb.linearVelocity = direccion * runspeed;
        }
        else
        {
            rb.linearVelocity = direccion * speed;

            if (stamina < maxStamina)
            {
                stamina += Time.deltaTime * (100f / 15f);
            }
        }

        stamina = Mathf.Clamp(stamina, 0, maxStamina);
    }

    void Update()
    {
        if (cinematicMode) return;

        // Bloquear disparo e inputs al pausar
        if (Time.timeScale == 0f)
            return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0f;
        directionMouse = mousePos - transform.position;

        float targetAngle = Mathf.Atan2(directionMouse.y, directionMouse.x) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.z;

        float smoothAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, 720f * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);

        Animations();
        UpdateBodyAnimation();
        Debug.DrawRay(bulletSpawn.position, directionMouse.normalized * raycastMaxDistance, Color.green);

        if (weaponObject != null)
        {
            bool hasAmmo = inventory != null && inventory.ActiveData != null
                           && inventory.ActiveData.bulletType != BulletType.None
                           && inventory.ShotCount > 0;
            weaponAnim?.SetVisible(hasAmmo);

            // Weapon bob — oscillates with footstep rhythm while moving
            float moveSpeed = direccion.magnitude;
            if (moveSpeed > 0.1f)
            {
                float freq = corriendo && !agotado ? 11f : 8f;
                bobTime += Time.deltaTime * freq;
                float bobY = Mathf.Sin(bobTime) * 0.04f;
                float bobX = Mathf.Cos(bobTime * 0.5f) * 0.015f;
                weaponObject.transform.localPosition = weaponBaseLocalPos + new Vector3(bobX, bobY, 0f);
            }
            else
            {
                bobTime = 0f;
                weaponObject.transform.localPosition = Vector3.Lerp(
                    weaponObject.transform.localPosition, weaponBaseLocalPos, Time.deltaTime * 10f);
            }
        }

        if (input.actions["Attack"].IsPressed())
            Shoot();
    }

void Shoot()
{
    if (inventory == null || inventory.ActiveData == null)
        return;

    float cooldown =
        1f /
        (inventory.ActiveData.fireRate * (XPSystem.Instance != null ? XPSystem.FireRateMultiplier : 1f));

    if (
        Time.time -
        lastShootTime <
        cooldown
    )
        return;

    if (
        !inventory
        .TryConsumeBullet()
    )
        return;

    AudioManager.Instance?.PlayShoot();
    weaponAnim?.Recoil();

    lastShootTime =
        Time.time;

    BulletData data =
        inventory.ActiveData;

    if (
        data.bulletType ==
        BulletType.Regular
    )
    {
        ShootRaycast(
            data
        );
    }
    else
    {
        GameObject newBullet =
            Instantiate(
                sharedBulletPrefab,
                bulletSpawn.position,
                Quaternion.identity
            );

        newBullet
        .GetComponent
        <BulletScript>()
        .Initialize(
            data,
            false
        );

        newBullet
        .GetComponent
        <Rigidbody2D>()
        .linearVelocity =
        directionMouse
        .normalized
        * data.speed;
    }
}

    private void ShootRaycast(BulletData data)
    {
        Vector2 origin = bulletSpawn.position;
        Vector2 direction = directionMouse.normalized;

        // --- NUEVO: Creamos una máscara que incluya todo EXCEPTO la capa "PickupLayer" ---
        int layerMask = ~LayerMask.GetMask("PickupLayer");

        // --- MODIFICADO: Le pasamos la layerMask a la función RaycastAll ---
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, raycastMaxDistance, layerMask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Vector2 endPoint = origin + direction * raycastMaxDistance;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("Player")) continue;

            endPoint = hit.point;
            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                int dmg = Mathf.RoundToInt(data.damage * (XPSystem.Instance != null ? XPSystem.DamageMultiplier : 1f));
                health.TakeDamage(dmg, transform);
            }
            break;
        }

        raycastEffect?.Fire(origin, endPoint);
    }

public void Animations()
{
    bool moving = direccion.x != 0 || direccion.y != 0;
    animator.SetBool("isRunning", moving);

    if (moving)
        AudioManager.Instance?.StartFootsteps();
    else
        AudioManager.Instance?.StopFootsteps();
}

private void UpdateBodyAnimation()
{
    bool moving    = direccion.magnitude > 0.1f;
    bool sprinting = moving && corriendo && !agotado;

    float targetX = 1f, targetY = 1f;

    if (moving)
    {
        // Footstep squash/stretch synced with weapon bob cycle
        float step     = (Mathf.Sin(bobTime) + 1f) * 0.5f;   // 0→1 per stride
        float amt      = sprinting ? 0.08f : 0.05f;
        targetX = 1f + amt * step;          // stretch in facing direction on stride
        targetY = 1f - amt * 0.55f * step; // compress perpendicular

        if (sprinting)
        {
            targetX *= 1.05f;   // overall slightly taller when at full sprint
            targetY *= 0.96f;
        }

        // Pop out when starting to move
        if (!wasMoving) { bodyScaleX = 0.82f; bodyScaleY = 1.18f; }
    }
    else
    {
        // Idle breathing — slow, subtle
        breathTime += Time.deltaTime * 1.1f;
        float breath = Mathf.Sin(breathTime) * 0.022f;
        targetX = 1f + breath;
        targetY = 1f - breath * 0.6f;

        // Landing squash when stopping
        if (wasMoving) { bodyScaleX = 1.14f; bodyScaleY = 0.86f; }
    }

    wasMoving = moving;

    bodyScaleX = Mathf.Lerp(bodyScaleX, targetX, Time.deltaTime * 15f);
    bodyScaleY = Mathf.Lerp(bodyScaleY, targetY, Time.deltaTime * 15f);

    transform.localScale = new Vector3(
        baseBodyScale.x * bodyScaleX,
        baseBodyScale.y * bodyScaleY,
        baseBodyScale.z);
}
}