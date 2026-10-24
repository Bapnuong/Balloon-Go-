using System.Collections.Generic;
using UnityEngine;

public class BirdSpawn : MonoBehaviour
{
    public GameObject birdPrefab;
    [Tooltip("Additional bird prefabs. One is selected randomly for each spawn.")]
    public GameObject[] birdPrefabs;
    public float startInterval = 6.5f;
    public float endInterval = 2.4f;
    public float minSpeed = 5.5f;
    public float maxSpeed = 8.5f;
    [Tooltip("Runtime scale keeps the bird proportional to the map.")]
    public float birdScale = 0.18f;

    readonly List<GameObject> spawned = new List<GameObject>();
    Transform spawnRoot;
    float timer;
    float nextSpawnInterval;

    void Awake()
    {
        spawnRoot = new GameObject("SpawnedBirds").transform;
        timer = 0f;
        nextSpawnInterval = startInterval * 0.7f;
    }

    void OnDestroy()
    {
        ClearAll();
        if (spawnRoot != null) Destroy(spawnRoot.gameObject);
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        if (birdPrefab == null && (birdPrefabs == null || birdPrefabs.Length == 0)) return;

        timer += Time.deltaTime;
        float interval = Mathf.Lerp(startInterval, endInterval, GameManager.Instance.Difficulty01);
        if (timer >= nextSpawnInterval)
        {
            timer = 0f;
            nextSpawnInterval = Random.Range(interval * 0.72f, interval * 1.28f);
            Spawn();
        }
    }

    void Spawn()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfWidth = cam.orthographicSize * cam.aspect;
        float x = cam.transform.position.x + halfWidth + 2.2f;
        float y = Random.Range(-2.6f, 2.6f);

        GameObject prefab = birdPrefab;
        if (birdPrefabs != null && birdPrefabs.Length > 0)
            prefab = birdPrefabs[Random.Range(0, birdPrefabs.Length)];
        if (prefab == null) return;

        var go = Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity, spawnRoot);
        go.tag = "Enemy";
        go.layer = LayerMask.NameToLayer("Enemy");
        if (go.GetComponent<BirdEnemy>() == null)
            go.AddComponent<BirdEnemy>();
        if (go.GetComponent<Collider2D>() == null)
        {
            var hitbox = go.AddComponent<CircleCollider2D>();
            hitbox.isTrigger = true;
            hitbox.radius = 0.7f;
        }
        go.transform.localScale = Vector3.one * (birdScale * Random.Range(0.88f, 1.12f));
        var bird = go.GetComponent<BirdEnemy>();
        if (bird != null)
        {
            bird.speed = Mathf.Lerp(minSpeed, maxSpeed, GameManager.Instance.Difficulty01);
            bird.amplitude = Random.Range(0.45f, 1.15f);
            bird.frequency = Random.Range(1.3f, 2.2f);
            bird.phase = Random.Range(0f, Mathf.PI * 2f);
        }
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
        timer = 0f;
        nextSpawnInterval = startInterval * 0.7f;
    }
}
