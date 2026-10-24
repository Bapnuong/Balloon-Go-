using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public float speed = 1f;
    [Tooltip("Small overlap prevents a visible vertical seam when copies loop.")]
    public float seamOverlap = 0.12f;
    [Tooltip("Smooths the layer into motion instead of starting with a hard jump.")]
    public float motionSmoothing = 8f;

    float width;
    float tileStep;
    float currentSpeed;
    int copies = 1;
    SpriteRenderer spriteRenderer;
    Camera cam;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        if (spriteRenderer == null) return;

        width = spriteRenderer.bounds.size.x;
        if (width <= 0.01f) width = 19.2f;
        tileStep = Mathf.Max(0.1f, width - seamOverlap);
        currentSpeed = 0f;

        // The scene stores two copies of every background layer. Line the copies
        // up edge-to-edge against the camera so the left/right sprite borders are
        // never visible (the old setup left a hard vertical seam on screen).
        var group = new List<ParallaxBackground>();
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
        copies = Mathf.Max(1, group.Count);

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

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        float left = cam.transform.position.x - cam.orthographicSize * cam.aspect;
        if (transform.position.x + width <= left)
            transform.position += new Vector3(tileStep * copies, 0f, 0f);
    }
}
