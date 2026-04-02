
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float spawnInterval = 2f;
    private float timer;
    void Update()
    {
        if (GameManager.Instance.gameOver) return;
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnObstacle();
            timer = 0f;
        }
    }
    void SpawnObstacle()
    {
        float yPos = Random.Range(-3f, -3f);
        Vector3 spawnPos = new Vector3(transform.position.x, yPos, 0);
        Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
    }
}
