using UnityEngine;

public class BalloonSkinSystem : MonoBehaviour
{
    public SpriteRenderer balloonRenderer;

    CircleCollider2D balloonCollider;
    float targetHeight = 1.38f;
    float targetRadius = 0.44f;

    void Awake()
    {
        if (balloonRenderer == null) balloonRenderer = GetComponent<SpriteRenderer>();
        balloonCollider = GetComponent<CircleCollider2D>();
    }

    void OnEnable()
    {
        if (ShopSystem.Instance != null)
        {
            ShopSystem.Instance.OnSelectionChanged += ApplySelected;
            ApplySelected();
        }
    }

    void OnDisable()
    {
        if (ShopSystem.Instance != null)
            ShopSystem.Instance.OnSelectionChanged -= ApplySelected;
    }

    public void ApplySelected()
    {
        if (ShopSystem.Instance == null || ShopSystem.Instance.SelectedItem == null) return;
        Apply(ShopSystem.Instance.SelectedItem);
    }

    public void Apply(ShopItem item)
    {
        if (item == null || item.Sprite == null || balloonRenderer == null) return;

        Sprite sprite = item.Sprite;
        float spriteHeight = Mathf.Max(0.0001f, sprite.bounds.size.y);
        float scale = targetHeight / spriteHeight;

        transform.localScale = new Vector3(scale, scale, 1f);
        balloonRenderer.sprite = sprite;
        balloonRenderer.color = Color.white;
        balloonRenderer.sortingLayerName = "Player";
        balloonRenderer.sortingOrder = 10;

        if (balloonCollider != null)
        {
            balloonCollider.radius = targetRadius / scale;
            balloonCollider.offset = Vector2.zero;
        }
    }
}
