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
    [SerializeField] private LineRenderer raycastLine;
    [SerializeField] private float raycastMaxDistance = 60f;
    [SerializeField] private float raycastLineDuration = 0.08f;

    public float speed;
    public float runspeed;
    bool corriendo;
    public bool agotado = false;

    public float stamina = 100f;
    public float maxStamina = 100f;

    private Vector2 direccion;
    private Vector2 directionMouse;

    private BulletInventory inventory;
    private float lastShootTime;

    private Vector2 raycastEndPoint;
    private float raycastLineEndTime;

    private GameObject weaponObject;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        input = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        inventory = BulletInventory.Instance;

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
            if (t.CompareTag("Weapon")) { weaponObject = t.gameObject; break; }

        if (raycastLine == null)
            raycastLine = gameObject.AddComponent<LineRenderer>();

        raycastLine.positionCount = 2;
        raycastLine.useWorldSpace = true;
        raycastLine.startWidth = 0.05f;
        raycastLine.endWidth = 0.05f;
        raycastLine.material = new Material(Shader.Find("Sprites/Default"));
        raycastLine.startColor = Color.white;
        raycastLine.endColor = Color.white;
        raycastLine.sortingOrder = 10;
        raycastLine.enabled = false;
    }

    void FixedUpdate()
    {
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
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0f;
        directionMouse = mousePos - transform.position;

        float targetAngle = Mathf.Atan2(directionMouse.y, directionMouse.x) * Mathf.Rad2Deg;
        float currentAngle = transform.eulerAngles.z;

        // MoveTowardsAngle filtra el micro-jitter del mouse sin perder responsividad
        float smoothAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, 720f * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);

        Animations();
        Debug.DrawRay(bulletSpawn.position, directionMouse.normalized * raycastMaxDistance, Color.green);

        if (weaponObject != null)
        {
            bool hasAmmo = inventory != null && inventory.ActiveData != null
                           && inventory.ActiveData.bulletType != BulletType.None
                           && inventory.ShotCount > 0;
            weaponObject.SetActive(hasAmmo);
        }

        if (raycastLine != null)
        {
            if (Time.time < raycastLineEndTime)
            {
                raycastLine.SetPosition(0, new Vector3(bulletSpawn.position.x, bulletSpawn.position.y, 0f));
                raycastLine.SetPosition(1, new Vector3(raycastEndPoint.x, raycastEndPoint.y, 0f));
                raycastLine.enabled = true;
            }
            else
            {
                raycastLine.enabled = false;
            }
        }

        if (input.actions["Attack"].IsPressed())
            Shoot();
    }

    void Shoot()
    {
        if (inventory == null || inventory.ActiveData == null) return;

        float cooldown = 1f / inventory.ActiveData.fireRate;
        if (Time.time - lastShootTime < cooldown) return;

        if (!inventory.TryConsumeBullet()) return;

        lastShootTime = Time.time;
        BulletData data = inventory.ActiveData;

        if (data.bulletType == BulletType.Regular)
        {
            ShootRaycast(data);
        }
        else
        {
            GameObject newBullet = Instantiate(sharedBulletPrefab, bulletSpawn.position, Quaternion.identity);
            newBullet.GetComponent<BulletScript>().Initialize(data, false);
            newBullet.GetComponent<Rigidbody2D>().linearVelocity = directionMouse.normalized * data.speed;
        }
    }

    private void ShootRaycast(BulletData data)
    {
        Vector2 origin = bulletSpawn.position;
        Vector2 direction = directionMouse.normalized;

        // RaycastAll para ignorar el propio collider del jugador
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, raycastMaxDistance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Vector2 endPoint = origin + direction * raycastMaxDistance;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("Player")) continue;

            endPoint = hit.point;
            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(data.damage);
            break;
        }

        raycastEndPoint = endPoint;
        raycastLineEndTime = Time.time + raycastLineDuration;
    }

    public void Animations()
    {
        if (direccion.x != 0 || direccion.y != 0)
            animator.SetBool("isRunning", true);
        else
            animator.SetBool("isRunning", false);
    }
}
