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

        // Start constant movement immediately
        rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        // Keep constant X speed (NO delay, NO smoothing)
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
            groundContact++;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContact--;
        }
    }

    bool IsGrounded()
    {
        return groundContact > 0;
    }
}