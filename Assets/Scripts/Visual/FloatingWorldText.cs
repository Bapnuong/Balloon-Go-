using UnityEngine;

public class FloatingWorldText : MonoBehaviour
{
    public float lifetime = 0.9f;
    public float rise = 1.1f;
    public Color color = Color.white;

    TextMesh textMesh;
    float age;
    Vector3 start;

    void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        start = transform.position;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        age += dt;
        float k = Mathf.Clamp01(age / lifetime);
        transform.position = start + Vector3.up * (rise * Mathf.SmoothStep(0f, 1f, k));
        if (textMesh != null)
        {
            Color c = color;
            c.a = 1f - k;
            textMesh.color = c;
        }
        if (age >= lifetime) Destroy(gameObject);
    }
}
