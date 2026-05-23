using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchScript : MonoBehaviour
{
    public GameObject Verde;
    public GameObject Azul;

    void Start()
    {
       Verde.SetActive(false);
       Azul.SetActive(true);
    }

    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("El jugador tocó el Switch presionando Espacio");
            if (Verde != null) Verde.SetActive(true);
            if (Azul != null) Azul.SetActive(false);
        }
    }
}
