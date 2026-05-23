using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;

    public float spawnRate = 3f;
    public float spawnX = 12f;
    public float spawnY = -3f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnRate)
        {
            SpawnObstacle();
            timer = 0f;
        }
    }

    void SpawnObstacle()
    {
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

        Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
    }
}