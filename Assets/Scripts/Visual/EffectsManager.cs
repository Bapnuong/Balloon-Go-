using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance;

    Camera cam;
    CameraShake shake;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
        if (cam != null) shake = cam.GetComponent<CameraShake>();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Shake(float duration, float magnitude)
    {
        if (shake == null && cam != null) shake = cam.GetComponent<CameraShake>();
        if (shake != null) shake.Shake(duration, magnitude);
    }

    public void SpawnPop(Vector3 position, Color tint)
    {
        Color a = UITheme.Gold;
        Color b = UITheme.Cream;
        Color c = UITheme.Coral;
        if (tint != Color.white) c = tint;

        for (int i = 0; i < 16; i++)
        {
            Color col = i % 3 == 0 ? a : (i % 3 == 1 ? b : c);
            Vector2 dir = Random.insideUnitCircle.normalized;
            float speed = Random.Range(2.2f, 6.2f);
            SpawnBit(position, dir * speed, col, Random.Range(0.16f, 0.34f), Random.Range(0.55f, 0.95f), -4.5f);
        }
    }

    public void SpawnCoinBurst(Vector3 position)
    {
        for (int i = 0; i < 9; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            float speed = Random.Range(1.5f, 4.2f);
            SpawnBit(position, dir * speed, UITheme.Gold, Random.Range(0.07f, 0.16f), Random.Range(0.35f, 0.6f), -1.5f);
        }
        SpawnFloatingText(position + Vector3.up * 0.2f, "+1", UITheme.Gold);
    }

    public void SpawnFlapPuff(Vector3 position)
    {
        for (int i = 0; i < 4; i++)
        {
            Vector2 dir = new Vector2(Random.Range(-1.4f, -0.2f), Random.Range(-1.2f, 0.2f));
            SpawnBit(position + Vector3.down * 0.55f, dir, new Color(1f, 1f, 1f, 0.55f), Random.Range(0.08f, 0.18f), 0.4f, -1.2f);
        }
    }

    public void SpawnTrail(Vector3 position)
    {
        SpawnBit(position + new Vector3(-0.35f, -0.5f, 0f), new Vector2(-1.4f, -0.2f), new Color(1f, 1f, 1f, 0.3f), 0.1f, 0.35f, 0f);
    }

    void SpawnBit(Vector3 position, Vector2 velocity, Color color, float scale, float lifetime, float gravity)
    {
        var go = new GameObject("fx_bit");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * scale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = UITheme.SoftCircle;
        sr.color = color;
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 30;

        var bit = go.AddComponent<ParticleBit>();
        bit.velocity = velocity;
        bit.lifetime = lifetime;
        bit.gravity = gravity;
    }

    public void SpawnFloatingText(Vector3 position, string text, Color color)
    {
        var go = new GameObject("fx_text");
        go.transform.position = position;

        var tm = go.AddComponent<TextMesh>();
        tm.font = UITheme.Font;
        tm.fontSize = 64;
        tm.characterSize = 0.11f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;
        tm.text = text;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sharedMaterial = UITheme.Font.material;
            mr.sortingLayerName = "Player";
            mr.sortingOrder = 35;
        }

        var ft = go.AddComponent<FloatingWorldText>();
        ft.color = color;
    }
}
