using UnityEngine;

public class BirdEnemy : MonoBehaviour
{
    public float speed = 5f;
    public float amplitude = 0.8f;
    public float frequency = 1.7f;
    public float phase;

    float baseY;
    float timeOffset;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        baseY = transform.position.y;
        timeOffset = Random.value * 10f;
    }

    void Start()
    {
        baseY = transform.position.y;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        float x = transform.position.x - speed * Time.deltaTime;
        float y = baseY + Mathf.Sin((Time.time + timeOffset) * frequency + phase) * amplitude;
        transform.position = new Vector3(x, y, transform.position.z);
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Cos((Time.time + timeOffset) * frequency + phase) * 8f);

        if (sr != null) sr.flipX = false;

        if (x < -20f)
            Destroy(gameObject);
    }
}
