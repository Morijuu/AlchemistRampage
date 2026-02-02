using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    public GameObject GroundCheck;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator animator;

    public float speed;

    private Vector2 direccion;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocity = direccion * speed;

        // Posición del mouse en el mundo
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0f;

        // Dirección hacia el mouse
        Vector2 direction = mousePos - transform.position;

        // Ángulo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Rotación con Rigidbody2D
        rb.rotation = angle;

    }
    void Update()
    {
        Animations();
    }

    public void OnMove (InputValue value)
    {
        direccion = value.Get<Vector2>();
        print(direccion);
    }

    public void Animations()
    {
        if (direccion.x != 0 || direccion.y != 0) animator.SetBool("isRunning", true);
        else animator.SetBool("isRunning", false);

    }
}
