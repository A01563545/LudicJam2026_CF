using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchScript : MonoBehaviour
{
    private static List<GameObject> verdeObjects = new List<GameObject>();
    private static List<GameObject> azulObjects = new List<GameObject>();
    private static float lastToggleTime = 0f;

    public SpriteRenderer indicadorSprite;

    [Header("Configuracion de Colores")]
    [Tooltip("Si es True, el color cambiara automaticamente segun el estado del bloque verde/azul. Si es False, mantendra sus colores originales.")]
    public bool cambiarColorAutomaticamente = true;

    [Tooltip("Color que toma cuando los bloques AZULES estan activos")]
    public Color colorIndicadorAzul = Color.blue;

    [Tooltip("Color que toma cuando los bloques VERDES estan activos")]
    public Color colorIndicadorVerde = Color.green;

    private bool playerInZone = false;
    private bool hasVisitedThisZone = false;
    private Coroutine pulseCoroutine;
    private Vector3 baseScale;

    void Awake()
    {
        verdeObjects.Clear();
        azulObjects.Clear();

        Transform[] todos = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in todos)
        {
            if (t.CompareTag("ColorVerde"))
            {
                verdeObjects.Add(t.gameObject);
            }
            else if (t.CompareTag("ColorAzul"))
            {
                azulObjects.Add(t.gameObject);
            }
        }
    }

    void Start()
    {
        if (indicadorSprite != null)
        {
            baseScale = indicadorSprite.transform.localScale;
        }

        NormalizeColorGroups();
        ActualizarColorIndicador();
    }

    void Update()
    {
        if (playerInZone && !hasVisitedThisZone && Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time - lastToggleTime < 0.1f)
            {
                return;
            }

            lastToggleTime = Time.time;
            hasVisitedThisZone = true;

            HacerPalpitarIndicador();
            ToggleColorGroups();

            SwitchScript[] todosLosSwitches = FindObjectsByType<SwitchScript>(FindObjectsSortMode.None);
            foreach (SwitchScript s in todosLosSwitches)
            {
                s.ActualizarColorIndicador();
            }
        }
    }

    public void ActualizarColorIndicador()
    {
        if (indicadorSprite == null || !cambiarColorAutomaticamente)
        {
            return;
        }

        bool verdeActivo = IsGroupActive(verdeObjects);
        indicadorSprite.color = verdeActivo ? colorIndicadorVerde : colorIndicadorAzul;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            hasVisitedThisZone = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
        }
    }

    private void ToggleColorGroups()
    {
        bool verdeActivo = IsGroupActive(verdeObjects);
        SetGroupActive(verdeObjects, !verdeActivo);
        SetGroupActive(azulObjects, verdeActivo);
    }

    private void NormalizeColorGroups()
    {
        bool verdeActivo = IsGroupActive(verdeObjects);
        bool azulActivo = IsGroupActive(azulObjects);

        if (verdeActivo == azulActivo)
        {
            SetGroupActive(verdeObjects, true);
            SetGroupActive(azulObjects, false);
        }
    }

    private bool IsGroupActive(List<GameObject> group)
    {
        foreach (GameObject obj in group)
        {
            if (obj != null)
            {
                return obj.activeSelf;
            }
        }

        return false;
    }

    private void SetGroupActive(List<GameObject> group, bool isActive)
    {
        foreach (GameObject obj in group)
        {
            if (obj != null)
            {
                obj.SetActive(isActive);
            }
        }
    }

    private void HacerPalpitarIndicador()
    {
        if (indicadorSprite == null)
        {
            return;
        }

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }

        pulseCoroutine = StartCoroutine(PulseRoutine());
    }

    private System.Collections.IEnumerator PulseRoutine()
    {
        Transform t = indicadorSprite.transform;
        Vector3 originalScale = baseScale;
        Vector3 targetScale = originalScale * 1.7f;

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }

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
