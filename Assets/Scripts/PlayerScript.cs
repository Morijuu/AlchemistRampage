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

    public float speed;

    private Vector2 direccion;
    private Vector2 directionMouse;

    private BulletInventory inventory;
    private float lastShootTime;

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
    }

    void FixedUpdate()
    {
        direccion = input.actions["Move"].ReadValue<Vector2>();
        rb.linearVelocity = direccion * speed;
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
        Debug.DrawRay(transform.position, directionMouse.normalized * 10f, Color.green);

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
        GameObject newBullet = Instantiate(sharedBulletPrefab, bulletSpawn.position, Quaternion.identity);
        newBullet.GetComponent<BulletScript>().Initialize(data, false);
        newBullet.GetComponent<Rigidbody2D>().linearVelocity = directionMouse.normalized * data.speed + rb.linearVelocity;
    }

    public void Animations()
    {
        if (direccion.x != 0 || direccion.y != 0)
            animator.SetBool("isRunning", true);
        else
            animator.SetBool("isRunning", false);
    }
}
