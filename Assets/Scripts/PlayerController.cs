using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 30f;

    private Rigidbody2D rb;
    private int groundContact = 0;
    private Collider2D playerCollider;
    private Collider2D pendingDeadlyCollider = null;
    private Coroutine pendingDeadlyRoutine = null;
    
    // Variable para saber si estamos parados sobre territorio con permiso para saltar
    private bool inJumpZone = false;
    private Collider2D currentJumpZoneCollider = null;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
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
            // EL JUGADOR SOLO PUEDE SALTAR SI ESTÁ:
            // 1. Tocando el piso (IsGrounded)
            // 2. Y ADEMÁS físicamente metido dentro de una zona "JumpZone"
            if (IsGrounded() && inJumpZone)
            {
                print("Saltando desde JumpZone permitida");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);                
                // Efecto visual: hacemos crecer el objeto de la zona de salto
                if (currentJumpZoneCollider != null)
                {
                    StartCoroutine(PulseRoutine(currentJumpZoneCollider.transform));
                }            }
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
            // Solo aplicamos la excepción si este Deadly pertenece a un obstáculo que también tiene safe zone.
            // Un Deadly suelto debe matar siempre.
            Collider2D safeZoneCollider = FindSiblingGroundCollider(other);
            if (safeZoneCollider == null)
            {
                Debug.Log("El jugador ha muerto al tocar un objeto mortal.");
                GameManager.instance.RestartLevel();
            }
            else
            {
                pendingDeadlyCollider = other;

                if (pendingDeadlyRoutine != null)
                {
                    StopCoroutine(pendingDeadlyRoutine);
                }

                pendingDeadlyRoutine = StartCoroutine(ResolveSafeObstacleDeadly(safeZoneCollider));
            }
        }

        if (other.CompareTag("Finish"))
        {
            GameManager.instance.WinLevel();
        }

        // Detectar entrada a la zona donde SÍ ES LEGAL saltar
        if (other.CompareTag("JumpZone"))
        {
            inJumpZone = true;
            currentJumpZoneCollider = other;
        }
    }

    // Usamos OnTriggerExit2D para saber cuándo sales de la zona legal de salto
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("JumpZone"))
        {
            inJumpZone = false;
            if (currentJumpZoneCollider == other)
            {
                currentJumpZoneCollider = null;
            }
        }
    }

    bool IsGrounded()
    {
        return groundContact > 0;
    }

    private bool IsAboveSafeZoneTop(Collider2D safeZoneCollider)
    {
        if (playerCollider == null || safeZoneCollider == null)
        {
            return false;
        }

        Bounds playerBounds = playerCollider.bounds;
        Bounds safeBounds = safeZoneCollider.bounds;

        // Margen pequeño para tolerar diferencias entre colliders y la simulación de física.
        float margin = 0.05f;

        return playerBounds.min.y >= safeBounds.max.y - margin;
    }

    private System.Collections.IEnumerator ResolveSafeObstacleDeadly(Collider2D safeZoneCollider)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        if (pendingDeadlyCollider == null)
        {
            pendingDeadlyRoutine = null;
            yield break;
        }

        // Si tras un par de pasos de física ya estamos apoyados sobre la safe zone,
        // cancelamos la muerte. Si no, es una colisión lateral real con el deadly.
        bool touchingSafe = false;
        if (playerCollider != null && safeZoneCollider != null)
        {
            touchingSafe = playerCollider.IsTouching(safeZoneCollider);
        }

        if (!touchingSafe)
        {
            Debug.Log("El jugador ha muerto al tocar un objeto mortal.");
            GameManager.instance.RestartLevel();
        }

        pendingDeadlyCollider = null;
        pendingDeadlyRoutine = null;
    }

    private Collider2D FindSiblingGroundCollider(Collider2D deadlyCollider)
    {
        if (deadlyCollider == null)
        {
            return null;
        }

        Transform parent = deadlyCollider.transform.parent;
        if (parent == null)
        {
            return null;
        }

        foreach (Transform child in parent)
        {
            if (child != deadlyCollider.transform && child.CompareTag("Ground"))
            {
                return child.GetComponent<Collider2D>();
            }
        }

        return null;
    }

    void OnEnable()
    {
        groundContact = 0;
        pendingDeadlyCollider = null;
        pendingDeadlyRoutine = null;
    }

    private System.Collections.IEnumerator PulseRoutine(Transform t)
    {
        // Buscamos si el collider tiene un hijo que sea el Sprite a escalar (igual que en los switches)
        // Si no tiene hijo, escalará el propio Trigger
        Transform targetToScale = t;
        SpriteRenderer sr = t.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) 
        {
            targetToScale = sr.transform;
        }

        // Si prefieres guardar el tamaño inicial de las zonas, pero por ahora tomamos su tamaño de inicio.
        Vector3 originalScale = targetToScale.localScale;
        Vector3 targetScale = originalScale * 1.7f; // Reducido también al 130%

        float duration = 0.15f;
        float elapsed = 0f;

        // Inflarse
        while (elapsed < duration)
        {
            if (targetToScale == null) yield break; // Seguridad por si la zona se destruye
            elapsed += Time.deltaTime;
            targetToScale.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }

        // Desinflarse
        elapsed = 0f;
        while (elapsed < duration)
        {
            if (targetToScale == null) yield break;
            elapsed += Time.deltaTime;
            targetToScale.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }

        if (targetToScale != null)
        {
            targetToScale.localScale = originalScale;
        }
    }
}