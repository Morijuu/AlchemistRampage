using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerMovement : MonoBehaviour
{

    public GameObject GroundCheck;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    public Animator animator;
    private PlayerInput input;
    [SerializeField] private GameObject bulletPrefab; // Cambiado de 'bullet'

    public float speed;

    private Vector2 direccion;

    private Vector2 directionMouse;

    private bool shoot;
    [SerializeField] float bulletSpeed = 5;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        input = GetComponent<PlayerInput>();
    }

    void FixedUpdate()
    {
        direccion = input.actions["Move"].ReadValue<Vector2>();

        rb.linearVelocity = direccion * speed; // Cambiado de linearVelocity

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0f;

        directionMouse = mousePos - transform.position;

        float angle = Mathf.Atan2(directionMouse.y, directionMouse.x) * Mathf.Rad2Deg;

        rb.rotation = angle;

        if (input.actions["Attack"].triggered) // Cambiado de ReadValue<bool>() para disparar solo una vez
        {
            Shoot();
        }
    }

    void Update()
    {
        Animations();
        Debug.DrawRay(transform.position, directionMouse.normalized * 10f, Color.green);
    }

    void Shoot()
    {
        GameObject newBullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity); // Cambiado

        Rigidbody2D bulletRb = newBullet.GetComponent<Rigidbody2D>();
        bulletRb.linearVelocity = directionMouse.normalized * bulletSpeed; // Cambiado de linearVelocity y normalizado
    }

    public void Animations()
    {
        if (direccion.x != 0 || direccion.y != 0)
        {
            animator.SetBool("isRunning", true);
            print("isRunning is true");
        }
        else
        {
            animator.SetBool("isRunning", false);
            print("isRunning is false");
        }
    }
}