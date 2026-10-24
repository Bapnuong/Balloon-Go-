using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FloatingUI : MonoBehaviour
{
    public float amplitude = 12f;
    public float speed = 1.1f;
    public float rotation = 4f;

    RectTransform rt;
    Vector2 basePos;
    float phase;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        basePos = rt.anchoredPosition;
        phase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        float t = Time.unscaledTime * speed + phase;
        rt.anchoredPosition = basePos + new Vector2(0f, Mathf.Sin(t) * amplitude);
        rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.65f) * rotation);
    }
}
