using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchScript : MonoBehaviour
{
    public GameObject Verde;
    public GameObject Azul;
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("El jugador tocó el Switch presionando Espacio");
            if (Verde != null) Verde.SetActive(false);
            if (Azul != null) Azul.SetActive(true);
        }
    }
}
