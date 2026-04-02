using UnityEngine;

public class BirdSpawn : MonoBehaviour
{
    public GameObject birdPrefab;
    public float spawnInterval = 10f;
    private float timer;
    void Update()
    {
        if (GameManager.Instance.gameOver) return;
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnBird();
            timer = 0f;
        }
    }
    void SpawnBird()
    {
        float yPos = Random.Range(-3f, 3f);
        Vector3 spawnPos = new Vector3(transform.position.x, yPos, 0);
        Instantiate(birdPrefab, spawnPos, Quaternion.identity);
    }
}
