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
    [Header("Short edge branches")]
    [Tooltip("Base scale for the shortened branch pieces.")]
    public float branchScale = 0.56f;
    [Tooltip("How far the branch is allowed to enter the play area from the top/bottom edge.")]
    public float edgeVisibleDepth = 1.1f;

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
        int variant = Random.Range(0, 3);
        bool fromTop = Random.value > 0.5f;
        float variantScale = new[] { 0.88f, 1f, 1.12f }[variant];
        float pieceHalfHeight = 2.4f * branchScale * variantScale * 0.5f;
        float top = cam.transform.position.y + cam.orthographicSize;
        float bottom = cam.transform.position.y - cam.orthographicSize;
        float y = fromTop
            ? top - edgeVisibleDepth + pieceHalfHeight
            : bottom + edgeVisibleDepth - pieceHalfHeight;

        // Only one short branch is spawned per interval. Its cut end stays
        // beyond the top or bottom boundary so it is hidden by the screen edge.
        SpawnPiece(x, y, variant, fromTop);
    }

    void SpawnPiece(float x, float y, int variant, bool fromTop)
    {
        if (obstaclePrefab == null) return;

        float[] variantScales = { 0.88f, 1f, 1.12f };
        float[] variantAngles = { -4f, 0f, 5f };
        float scale = branchScale * variantScales[Mathf.Clamp(variant, 0, 2)];
        float angle = variantAngles[Mathf.Clamp(variant, 0, 2)] * (fromTop ? 1f : -1f);

        var go = Instantiate(obstaclePrefab, new Vector3(x, y, 0f), obstaclePrefab.transform.rotation, spawnRoot);
        // The upper branch is upside down so its cut end is hidden behind
        // the top boundary; the lower branch keeps the normal orientation.
        go.transform.localScale = new Vector3(scale, fromTop ? -scale : scale, scale);
        go.transform.Rotate(0f, 0f, angle);

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
