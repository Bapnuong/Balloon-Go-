
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;

    public float spawnRate = 3f;

    void Start()
    {
        InvokeRepeating("SpawnCoin",2f,spawnRate);
    }

    void SpawnCoin()
    {
        float camHeight = Camera.main.orthographicSize;
        float camWidth = camHeight * Camera.main.aspect;

        float y = Random.Range(-camHeight + 1f, camHeight - 1f);

        Vector3 pos = new Vector3(
            Camera.main.transform.position.x + camWidth + 2f,
            y,
            0
        );

        Instantiate(coinPrefab, pos, Quaternion.identity);
    }
}
