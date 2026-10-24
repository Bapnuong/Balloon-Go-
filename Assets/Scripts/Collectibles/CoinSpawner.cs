using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    public float startInterval = 2.4f;
    public float endInterval = 1.55f;
    public float minSpeed = 3.2f;
    public float maxSpeed = 4.9f;

    readonly List<GameObject> spawned = new List<GameObject>();
    Transform spawnRoot;
    float timer;
    int enemyMask;

    void Awake()
    {
        spawnRoot = new GameObject("SpawnedCoins").transform;
        timer = 1.1f;
        enemyMask = LayerMask.GetMask("Enemy");
    }

    void OnDestroy()
    {
        ClearAll();
        if (spawnRoot != null) Destroy(spawnRoot.gameObject);
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        if (coinPrefab == null) return;

        timer += Time.deltaTime;
        float interval = Mathf.Lerp(startInterval, endInterval, GameManager.Instance.Difficulty01);
        if (timer >= interval)
        {
            timer = 0f;
            SpawnPattern();
        }
    }

    void SpawnPattern()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfWidth = cam.orthographicSize * cam.aspect;
        float startX = cam.transform.position.x + halfWidth + 2.0f;
        // Keep the whole coin pattern comfortably inside the camera bounds.
        float baseY = Random.Range(-2.8f, 2.8f);
        int pattern = Random.Range(0, 3);
        int count = pattern == 0 ? Random.Range(3, 6) : Random.Range(4, 7);
        float spacing = 0.85f;
        float speed = Mathf.Lerp(minSpeed, maxSpeed, GameManager.Instance.Difficulty01);

        for (int i = 0; i < count; i++)
        {
            float y = baseY;
            if (pattern == 1)
                y = baseY + Mathf.Sin(i * 1.05f) * 0.75f;
            else if (pattern == 2)
                y = baseY + (i - (count - 1) * 0.5f) * (i - (count - 1) * 0.5f) * 0.22f - 0.6f;

            y = Mathf.Clamp(y, -3.2f, 3.2f);
            Vector3 pos = new Vector3(startX + i * spacing, y, 0f);

            if (enemyMask != 0 && Physics2D.OverlapCircle(pos, 0.45f, enemyMask) != null)
                continue;

            var go = Instantiate(coinPrefab, pos, Quaternion.identity, spawnRoot);
            var renderer = go.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
                renderer.sortingLayerName = "Player";
                renderer.sortingOrder = 20;
            }
            var move = go.GetComponent<CoinMove>();
            if (move != null) move.speed = speed;
            spawned.Add(go);
        }
    }

    public void ClearAll()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] != null)
                Destroy(spawned[i]);
        }
        spawned.Clear();
        timer = 1.1f;
    }
}
