using System.Collections.Generic;
using UnityEngine;

public class BirdSpawn : MonoBehaviour
{
    public GameObject birdPrefab;
    public float startInterval = 6.5f;
    public float endInterval = 2.4f;
    public float minSpeed = 4.8f;
    public float maxSpeed = 7.5f;

    readonly List<GameObject> spawned = new List<GameObject>();
    Transform spawnRoot;
    float timer;

    void Awake()
    {
        spawnRoot = new GameObject("SpawnedBirds").transform;
        timer = startInterval * 0.7f;
    }

    void OnDestroy()
    {
        ClearAll();
        if (spawnRoot != null) Destroy(spawnRoot.gameObject);
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        if (birdPrefab == null) return;

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
        float x = cam.transform.position.x + halfWidth + 2.2f;
        float y = Random.Range(-2.6f, 2.6f);

        var go = Instantiate(birdPrefab, new Vector3(x, y, 0f), Quaternion.identity, spawnRoot);
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
        timer = startInterval * 0.7f;
    }
}
