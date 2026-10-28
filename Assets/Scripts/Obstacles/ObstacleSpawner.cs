using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    [Tooltip("Legacy range kept for scene compatibility. Edge branches now grow from the top/bottom.")]
    public Vector2 yRange = new Vector2(-2.2f, 2.2f);
    public float startInterval = 2.7f;
    public float endInterval = 1.4f;
    public float minSpeed = 4f;
    public float maxSpeed = 7.2f;

    [Header("Short edge branches")]
    [Tooltip("Base scale for the shortened branch pieces.")]
    public float branchScale = 0.45f;
    [Tooltip("How far the cut base sits outside the top/bottom edge.")]
    public float baseMargin = 0.35f;
    [Tooltip("Per-variant size multipliers: green, gold, moss.")]
    public float[] variantScales = { 0.95f, 0.9f, 1.05f };

    readonly List<GameObject> spawned = new List<GameObject>();
    Transform spawnRoot;
    float timer;
    int lastVariant = -1;

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
        if (cam == null || obstaclePrefab == null) return;

        int variant = Random.Range(0, 3);
        if (variant == lastVariant) variant = (variant + 1) % 3;
        lastVariant = variant;

        bool fromTop = Random.value > 0.5f;
        float variantScale = variantScales != null && variant < variantScales.Length
            ? variantScales[variant]
            : 1f;
        float scale = branchScale * variantScale;

        var go = Instantiate(obstaclePrefab, Vector3.zero, Quaternion.identity, spawnRoot);
        go.transform.localScale = Vector3.one * scale;

        var branch = go.GetComponent<BranchObstacle>();
        if (branch == null) branch = go.AddComponent<BranchObstacle>();
        branch.Setup(variant, fromTop);

        float halfWidth = cam.orthographicSize * cam.aspect;
        float x = cam.transform.position.x + halfWidth + branch.Thickness * scale * 0.5f + 0.7f;
        float top = cam.transform.position.y + cam.orthographicSize;
        float bottom = cam.transform.position.y - cam.orthographicSize;
        float y = fromTop ? top + baseMargin : bottom - baseMargin;
        go.transform.position = new Vector3(x, y, 0f);

        var obstacle = go.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            float difficulty = GameManager.Instance != null ? GameManager.Instance.Difficulty01 : 0f;
            obstacle.speed = Mathf.Lerp(minSpeed, maxSpeed, difficulty);
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
        timer = startInterval * 0.55f;
        lastVariant = -1;
    }
}
