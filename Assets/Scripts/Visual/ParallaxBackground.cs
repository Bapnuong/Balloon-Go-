using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public float speed = 1f;
    [Tooltip("Extra overlap between copies. The forest textures are seamless, so 0 is enough.")]
    public float seamOverlap = 0f;
    [Tooltip("Smooths the layer into motion instead of starting with a hard jump.")]
    public float motionSmoothing = 8f;
    [Tooltip("Keep one copy centered and scaled to cover the screen. Use for full-screen fog overlays.")]
    public bool staticOverlay = false;

    float width;
    float tileStep;
    float currentSpeed;
    SpriteRenderer spriteRenderer;
    Camera cam;
    List<ParallaxBackground> group;
    bool leader;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        if (spriteRenderer == null) return;

        width = spriteRenderer.bounds.size.x;
        if (width <= 0.01f) width = 19.2f;
        tileStep = Mathf.Max(0.1f, width - seamOverlap);
        currentSpeed = 0f;

        if (staticOverlay)
        {
            // A repeating fog texture shows its edge where the pattern changes.
            // Keep a single copy stretched slightly beyond the camera instead.
            if (gameObject.name.Contains("(1)"))
            {
                gameObject.SetActive(false);
                return;
            }

            if (cam != null)
            {
                float camWidth = cam.orthographicSize * cam.aspect * 2f;
                float scale = Mathf.Max(1f, camWidth / width) * 1.06f;
                transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, transform.position.z);
                transform.localScale = new Vector3(scale, scale, 1f);
            }
            enabled = false;
            return;
        }

        group = new List<ParallaxBackground>();
        var all = FindObjectsByType<ParallaxBackground>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            var other = all[i];
            if (other == null) continue;
            var otherRenderer = other.GetComponent<SpriteRenderer>();
            if (otherRenderer == null || otherRenderer.sprite != spriteRenderer.sprite) continue;
            if (!Mathf.Approximately(other.speed, speed)) continue;
            group.Add(other);
        }

        group.Sort((a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));
        leader = group.Count > 0 && group[0] == this;

        if (cam == null) return;
        float left = cam.transform.position.x - cam.orthographicSize * cam.aspect;
        for (int i = 0; i < group.Count; i++)
        {
            var t = group[i].transform;
            t.position = new Vector3(left + i * tileStep, t.position.y, t.position.z);
        }
    }

    void LateUpdate()
    {
        if (width <= 0.01f) return;

        float blend = 1f - Mathf.Exp(-motionSmoothing * Time.deltaTime);
        currentSpeed = Mathf.Lerp(currentSpeed, speed, blend);
        transform.Translate(Vector2.left * currentSpeed * Time.deltaTime, Space.World);

        if (!leader) return;
        if (cam == null) cam = Camera.main;
        if (cam == null || group == null || group.Count < 2) return;

        // Keep the right edge of the rightmost copy at or beyond the camera's
        // right edge. If it falls inside the view, move the leftmost copy to
        // the right of the chain. This keeps the screen covered at all times;
        // the old per-copy wrap left the right half of the mist layer exposed.
        float camRight = cam.transform.position.x + cam.orthographicSize * cam.aspect;
        for (int guard = 0; guard < group.Count; guard++)
        {
            ParallaxBackground right = group[0];
            ParallaxBackground left = group[0];
            for (int i = 1; i < group.Count; i++)
            {
                if (group[i].transform.position.x > right.transform.position.x) right = group[i];
                if (group[i].transform.position.x < left.transform.position.x) left = group[i];
            }

            if (right.transform.position.x + width * 0.5f >= camRight) break;

            var t = left.transform;
            t.position = new Vector3(right.transform.position.x + tileStep, t.position.y, t.position.z);
        }
    }
}
