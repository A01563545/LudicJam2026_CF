using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    // Arrastra a tu jugador a este campo desde el Inspector de Unity
    public Transform playerTransform; 
    
    // Ajusta este factor para controlar la velocidad del fondo (útil para efecto Parallax)
    public float scrollSpeedFactor = 0.05f; 

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (playerTransform != null)
        {
            // El offset ahora es directamente proporcional a la posición del jugador
            float offset = playerTransform.position.x * scrollSpeedFactor;
            rend.material.mainTextureOffset = new Vector2(offset, 0);
        }
    }
}