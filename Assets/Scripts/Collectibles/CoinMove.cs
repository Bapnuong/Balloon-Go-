using UnityEngine;

public class CoinMove : MonoBehaviour
{
    public float speed = 3.2f;
    public float bobAmplitude = 0.12f;
    public float spinSpeed = 3.2f;

    float baseY;
    float phase;
    Vector3 baseScale;

    void Start()
    {
        baseY = transform.position.y;
        baseScale = transform.localScale;
        phase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        float x = transform.position.x - speed * Time.deltaTime;
        float y = baseY + Mathf.Sin(Time.time * 2.2f + phase) * bobAmplitude;
        transform.position = new Vector3(x, y, transform.position.z);

        float spin = 0.72f + 0.28f * Mathf.Abs(Mathf.Cos(Time.time * spinSpeed + phase));
        transform.localScale = new Vector3(baseScale.x * spin, baseScale.y, baseScale.z);

        if (x < -20f)
            Destroy(gameObject);
    }
}
