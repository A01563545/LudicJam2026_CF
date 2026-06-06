using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ObstacleChartSpawner : MonoBehaviour
{
    [Header("Music Clock")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private bool playMusicOnStart = true;

    [Header("Player Movement")]
    [SerializeField] private Transform player;
    [SerializeField] private float fallbackPlayerSpeed = 5f;
    [SerializeField] private float firstTimestampOffset = 0f;

    [Header("Chart JSON")]
    [SerializeField] private TextAsset chartJson;
    [SerializeField] private string persistentFileName = "timestamps.json";

    [Header("Placement")]
    [SerializeField] private float switchObstacleXOffset = 0f;
    [SerializeField] private float switchObstacleY = 2.3f;
    [SerializeField] private float jumpZoneY = 2.92f;
    [SerializeField] private Transform obstacleParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject switchObstaclePrefab;
    [SerializeField] private GameObject jumpZonePrefab;

    private readonly List<RecordedTimestamp> chart = new List<RecordedTimestamp>();

    private void Start()
    {
        LoadChart();
        BuildLevelFromChart();

        if (musicSource != null && playMusicOnStart)
        {
            musicSource.Play();
        }
    }

    private void LoadChart()
    {
        string json = "";

        if (chartJson != null)
        {
            json = chartJson.text;
        }
        else
        {
            string path = Path.Combine(Application.persistentDataPath, persistentFileName);

            if (File.Exists(path))
            {
                json = File.ReadAllText(path);
            }
            else
            {
                Debug.LogWarning("No timestamp JSON found at: " + path);
                return;
            }
        }

        TimestampRecording recording = JsonUtility.FromJson<TimestampRecording>(json);

        if (recording == null || recording.timestamps == null)
        {
            Debug.LogWarning("Timestamp JSON could not be read.");
            return;
        }

        chart.Clear();
        chart.AddRange(recording.timestamps);
        chart.Sort((a, b) => a.time.CompareTo(b.time));

        Debug.Log("Loaded " + chart.Count + " timestamps.");
    }

    private void BuildLevelFromChart()
    {
        if (chart.Count == 0)
        {
            return;
        }

        float playerStartX = player != null ? player.position.x : 0f;
        PlayerController playerController = player != null ? player.GetComponent<PlayerController>() : null;
        float playerSpeed = playerController != null ? playerController.moveSpeed : fallbackPlayerSpeed;

        foreach (RecordedTimestamp timestamp in chart)
        {
            float xPosition = playerStartX + (timestamp.time + firstTimestampOffset) * playerSpeed;
            PlaceForTimestamp(timestamp, xPosition);
        }
    }

    private void PlaceForTimestamp(RecordedTimestamp timestamp, float xPosition)
    {
        if (timestamp.key == "Jump")
        {
            PlaceJumpZone(xPosition);
        }
        else if (timestamp.key == "SwitchPhase1")
        {
            PlaceSwitchObstacle(xPosition + switchObstacleXOffset, true);
        }
        else if (timestamp.key == "SwitchPhase2")
        {
            PlaceSwitchObstacle(xPosition + switchObstacleXOffset, false);
        }
        else
        {
            Debug.LogWarning("Unknown timestamp key: " + timestamp.key);
        }
    }

    private void PlaceSwitchObstacle(float xPosition, bool greenActive)
    {
        if (switchObstaclePrefab == null)
        {
            Debug.LogWarning("Switch obstacle prefab is not set up.");
            return;
        }

        GameObject switchInstance = PlacePrefab(switchObstaclePrefab, xPosition, switchObstacleY);
        ApplySwitchPhase(switchInstance, greenActive);
    }

    private void PlaceJumpZone(float xPosition)
    {
        if (jumpZonePrefab == null)
        {
            Debug.LogWarning("Jump zone prefab is not set up.");
            return;
        }

        PlacePrefab(jumpZonePrefab, xPosition, jumpZoneY);
    }

    private GameObject PlacePrefab(GameObject prefab, float xPosition, float yPosition)
    {
        GameObject instance = Instantiate(prefab);
        instance.transform.position = new Vector3(xPosition, yPosition, 0f);

        if (obstacleParent != null)
        {
            instance.transform.SetParent(obstacleParent, true);
        }

        return instance;
    }

    private void ApplySwitchPhase(GameObject switchInstance, bool greenActive)
    {
        if (switchInstance == null)
        {
            return;
        }

        Transform[] children = switchInstance.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.CompareTag("ColorVerde"))
            {
                child.gameObject.SetActive(greenActive);
            }
            else if (child.CompareTag("ColorAzul"))
            {
                child.gameObject.SetActive(!greenActive);
            }
        }
    }
}
