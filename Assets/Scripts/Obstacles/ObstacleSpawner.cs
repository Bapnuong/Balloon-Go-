
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstacles;

    public float spawnRate = 2f;
    public float heightOffset = 3f;

    void Start()
    {
        InvokeRepeating("SpawnObstacle",1f,spawnRate);
    }

    void SpawnObstacle()
    {
        if(obstacles.Length == 0) return;

        int index = Random.Range(0, obstacles.Length);

        Vector3 pos = transform.position;
        pos.y += Random.Range(-heightOffset,heightOffset);

        Instantiate(obstacles[index],pos,Quaternion.identity);
    }
}
