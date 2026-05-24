using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 30f;

    private Rigidbody2D rb;
    private int groundContact = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundContact = 0;
        rb.linearVelocity = Vector2.zero;
        rb.WakeUp();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
    }

    public void OnJump(InputValue value)
    {
        // Se presionó el botón
        if (value.isPressed)
        {
            // Solo salta si está tocando el suelo
            if (IsGrounded())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }
        // Al soltar botón
        else
        {
            // Aún va subiendo
            if (rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    groundContact++;
                    break;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContact = Mathf.Max(0, groundContact - 1);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        Debug.Log("Trigger hit: " + other.gameObject.name + " tag: " + other.tag);

        if (other.CompareTag("Deadly"))
        {
            Debug.Log("El jugador ha muerto al tocar un objeto mortal.");
            GameManager.instance.RestartLevel();
        }

        if (other.CompareTag("Finish"))
        {
            GameManager.instance.WinLevel();
        }
    }

    bool IsGrounded()
    {
        return groundContact > 0;
    }

    void OnEnable()
    {
        groundContact = 0;
    }
}