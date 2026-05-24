using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SwitchScript : MonoBehaviour
{
    // Listas para guardar todos los obstáculos (estáticas para compartirlas)
    private static List<GameObject> verdeObjects = new List<GameObject>();
    private static List<GameObject> azulObjects = new List<GameObject>();
    private static float lastToggleTime = 0f; 
    
    // Este será el indicador PROPIO de este switch
    public SpriteRenderer indicadorSprite;
    
    [Header("Configuración de Colores")]
    [Tooltip("Si es True, el color cambiará automáticamente según el estado del bloque verde/azul. Si es False, mantendrá sus colores originales.")]
    public bool cambiarColorAutomaticamente = true;
    
    [Tooltip("Color que toma cuando los bloques AZULES están activos")]
    public Color colorIndicadorAzul = Color.blue; 
    
    [Tooltip("Color que toma cuando los bloques VERDES están activos")]
    public Color colorIndicadorVerde = Color.green; 

    private bool playerInZone = false;
    private bool hasVisitedThisZone = false; // Permite un solo uso hasta que salgas y entres    
    // Corrutina para el efecto de palpito
    private Coroutine pulseCoroutine;
    private Vector3 baseScale;

    void Awake()
    {
        // Al reiniciar, limpiamos las listas. El último switch en hacer Awake deja la lista lista para todos.
        verdeObjects.Clear();
        azulObjects.Clear();

        // En lugar de FindGameObjectsWithTag (que IGNORA los objetos que apagaste manualmente), 
        // buscamos absolutamente TODOS en la escena, activos o inactivos.
        Transform[] todos = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in todos)
        {
            if (t.CompareTag("ColorVerde")) verdeObjects.Add(t.gameObject);
            else if (t.CompareTag("ColorAzul")) azulObjects.Add(t.gameObject);
        }
    }

    void Start()
    {
        if (indicadorSprite != null)
        {
            baseScale = indicadorSprite.transform.localScale;
        }

        // YA NO forzamos ningún estado de Activar/Desactivar en el Start. 
        // Respetaremos al 100% como los hayas dejado (prendidos o apagados) en tu jerarquía.
        
        // Actualizamos el color al arrancar, dependiendo de cuál grupo esté activo de verdad
        ActualizarColorIndicador();
    }

    void Update()
    {
        // Solo ejecuta si estás en zona, NO te has activado todavía en esta visita, y presionas Espacio
        if (playerInZone && !hasVisitedThisZone && Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time - lastToggleTime < 0.1f) return;
            lastToggleTime = Time.time;

            Debug.Log("El jugador interactuó con el Switch (1 sola vez)");
            
            // Marcamos como usado para que no puedas "spamear"
            hasVisitedThisZone = true;
            
            // Efecto visual: hacemos crecer el indicador un momento
            HacerPalpitarIndicador();

            // Invertimos físicamente el estado actual (sea cual sea) de cada objeto
            invertColor(verdeObjects);
            invertColor(azulObjects);

            // Leemos la nueva realidad física y actualizamos las luces de todos los switches
            SwitchScript[] todosLosSwitches = FindObjectsByType<SwitchScript>(FindObjectsSortMode.None);
            foreach(SwitchScript s in todosLosSwitches)
            {
                s.ActualizarColorIndicador();
            }
        }
    }

    public void ActualizarColorIndicador()
    {
        if (indicadorSprite == null) return;

        // Si el usuario decide manual, ignoramos toda la lógica de aquí en adelante
        if (!cambiarColorAutomaticamente) return;

        bool verdeActivo = false;
        
        // Detectamos directamente si el bloque Verde está activo leyendo la realidad del juego 
        if (verdeObjects.Count > 0 && verdeObjects[0] != null) 
        {
            verdeActivo = verdeObjects[0].activeSelf;
        }
        else if (azulObjects.Count > 0 && azulObjects[0] != null)
        {
            // Si no hay bloques verdes, pero al menos hay azules, lo deducimos de ahí
            verdeActivo = !azulObjects[0].activeSelf;
        }

        // Si verdeActivo es true, pintamos el indicador de Verde, si no, de Azul
        indicadorSprite.color = verdeActivo ? colorIndicadorVerde : colorIndicadorAzul;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            hasVisitedThisZone = false; // Al entrar de nuevo a la zona, te damos permiso una vez más
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInZone = false;
    }

    // Función auxiliar: si un objeto estaba prendido, lo apaga. Si estaba apagado, lo prende.
    private void invertColor(List<GameObject> group)
    {
        foreach (GameObject obj in group)
        {
            if (obj != null)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    }

    private void HacerPalpitarIndicador()
    {
        if (indicadorSprite == null) return;
        
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseRoutine());
    }

    private System.Collections.IEnumerator PulseRoutine()
    {
        Transform t = indicadorSprite.transform;
        Vector3 originalScale = baseScale;               // Usamos la escala original guardada en el Start
        Vector3 targetScale = originalScale * 1.7f;      // Ajustado al 130% para que no crezca tanto

        float duration = 0.15f;
        float elapsed = 0f;

        // Crecer
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }

        // Encoger
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }

        t.localScale = originalScale;
    }
}
