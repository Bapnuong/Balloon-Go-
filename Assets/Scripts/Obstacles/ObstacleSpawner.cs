using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public Vector2 yRange = new Vector2(-2.2f, 2.2f);
    public float startInterval = 2.7f;
    public float endInterval = 1.4f;
    public float minSpeed = 4f;
    public float maxSpeed = 7.2f;
    public float branchScale = 0.8f;

    readonly List<GameObject> spawned = new List<GameObject>();
    Transform spawnRoot;
    float timer;

    void Awake()
    {
        spawnRoot = new GameObject("SpawnedBranches").transform;
        timer = startInterval * 0.55f;
    }

    void OnDestroy()
    {
        ClearAll();
        if (spawnRoot != null) Destroy(spawnRoot.gameObject);
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        if (obstaclePrefab == null) return;

        timer += Time.deltaTime;
        float interval = Mathf.Lerp(startInterval, endInterval, GameManager.Instance.Difficulty01);
        if (timer >= interval)
        {
            timer = 0f;
            Spawn();
        }
    }

    void Spawn()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfWidth = cam.orthographicSize * cam.aspect;
        float branchHalfWidth = 15.36f * branchScale * 0.5f;
        float x = cam.transform.position.x + halfWidth + branchHalfWidth + 0.6f;
        float y = Random.Range(yRange.x, yRange.y);

        var go = Instantiate(obstaclePrefab, new Vector3(x, y, 0f), obstaclePrefab.transform.rotation, spawnRoot);
        go.transform.localScale = Vector3.one * branchScale;

        var obstacle = go.GetComponent<Obstacle>();
        if (obstacle != null)
            obstacle.speed = Mathf.Lerp(minSpeed, maxSpeed, GameManager.Instance.Difficulty01);

        spawned.Add(go);
    }

    public void ClearAll()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] != null)
                Destroy(spawned[i]);
        }
        spawned.Clear();
        timer = startInterval * 0.55f;
    }
}
