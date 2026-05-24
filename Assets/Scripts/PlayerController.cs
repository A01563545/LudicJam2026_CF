using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

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
        if (!value.isPressed)
            return;

        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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