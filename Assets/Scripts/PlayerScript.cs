using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerScript : MonoBehaviour
{

    public GameObject GroundCheck;
    private Rigidbody rb; // CAMBIO: Rigidbody2D -> Rigidbody (3D)
    private SpriteRenderer sr;
    public Animator animator;
    private PlayerInput input;
    [SerializeField] private GameObject bulletPrefab; // Cambiado de 'bullet'
    [SerializeField] private Transform bulletSpawn;

    public float speed;

    private Vector2 direccion; // Se mantiene para input
    private Vector3 direccion3D; // CAMBIO: nueva dirección en 3D (X,Z)

    private Vector3 directionMouse; // CAMBIO: ahora Vector3

    private bool shoot;
    [SerializeField] float bulletSpeed = 5;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); // CAMBIO
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        input = GetComponent<PlayerInput>();
    }

    void FixedUpdate()
    {
        direccion = input.actions["Move"].ReadValue<Vector2>();

        // CAMBIO: Convertimos el input 2D a movimiento en XZ (Y = 0)
        direccion3D = new Vector3(direccion.x, 0f, direccion.y);

        rb.linearVelocity = direccion3D * speed; // Sigue usando linearVelocity pero en 3D

        // CAMBIO: Raycast desde la cámara hacia un plano para obtener posición del mouse en 3D
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.up, transform.position);

        float distance;

        if (plane.Raycast(ray, out distance))
        {
            Vector3 mousePos = ray.GetPoint(distance);

            directionMouse = mousePos - transform.position;
            directionMouse.y = 0f; // CAMBIO: ignoramos altura

            // CAMBIO: rotación en 3D usando Quaternion
            if (directionMouse != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(directionMouse);
                rb.MoveRotation(rot);
            }
        }
    }

    void Update()
    {
        Animations();

        // CAMBIO: ahora usamos forward en vez de vector 2D
        Debug.DrawRay(transform.position, transform.forward * 10f, Color.green);

        if (input.actions["Attack"].triggered) // Cambiado de ReadValue<bool>() para disparar solo una vez
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject newBullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity); // Cambiado

        Rigidbody bulletRb = newBullet.GetComponent<Rigidbody>(); // CAMBIO: Rigidbody 3D
        bulletRb.linearVelocity = transform.forward * bulletSpeed; // CAMBIO: dispara hacia adelante en 3D

    }

    public void Animations()
    {
        if (direccion.x != 0 || direccion.y != 0)
        {
            animator.SetBool("isRunning", true);
            //print("isRunning is true");
        }
        else
        {
            animator.SetBool("isRunning", false);
            //print("isRunning is false");
        }
    }
}