using UnityEngine;

public class ParticleBit : MonoBehaviour
{
    public Vector2 velocity;
    public float gravity = -3.5f;
    public float lifetime = 0.65f;
    public float drag = 1.2f;

    float age;
    SpriteRenderer sr;
    Color baseColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        age += dt;
        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        velocity += Vector2.up * gravity * dt;
        velocity *= Mathf.Clamp01(1f - drag * dt);
        transform.position += (Vector3)(velocity * dt);

        float k = 1f - age / lifetime;
        if (sr != null)
        {
            Color c = baseColor;
            c.a = baseColor.a * k;
            sr.color = c;
        }
        transform.localScale = transform.localScale * (1f - dt * 0.35f);
    }
}
