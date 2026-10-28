using UnityEngine;

// Replaces the old baked branch sprite with one of three smaller branch
// variants. Each branch grows out of the top or bottom screen edge, so its
// cut-off base is always hidden outside the play area.
public class BranchObstacle : MonoBehaviour
{
    public SpriteRenderer art;
    public BoxCollider2D box;

    public bool FromTop { get; private set; }
    public float Length { get; private set; }
    public float Thickness { get; private set; }

    public Vector3 TipWorldPosition
    {
        get
        {
            if (art == null) return transform.position;
            return art.transform.TransformPoint(new Vector3(-Length, 0f, 0f));
        }
    }

    static Sprite[] sprites;
    static bool loaded;

    void Awake()
    {
        // The old prefab contains a huge horizontal branch and its own collider.
        var oldSprite = GetComponent<SpriteRenderer>();
        if (oldSprite != null) oldSprite.enabled = false;

        var oldPoly = GetComponent<PolygonCollider2D>();
        if (oldPoly != null) oldPoly.enabled = false;

        var body = GetComponent<Rigidbody2D>();
        if (body != null) body.bodyType = RigidbodyType2D.Kinematic;

        transform.rotation = Quaternion.identity;

        if (art == null)
        {
            var artGo = new GameObject("BranchArt");
            artGo.transform.SetParent(transform, false);
            art = artGo.AddComponent<SpriteRenderer>();
            art.sortingOrder = 2;
        }

        if (box == null) box = GetComponent<BoxCollider2D>();
        if (box == null) box = gameObject.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.offset = Vector2.zero;
        box.size = new Vector2(1f, 1f);
    }

    // variant: 0 = green, 1 = gold, 2 = moss.
    // fromTop: branch hangs down from the top edge, otherwise grows up from the bottom.
    public float Setup(int variant, bool fromTop)
    {
        FromTop = fromTop;
        LoadSprites();

        variant = Mathf.Clamp(variant, 0, sprites.Length - 1);
        Sprite sprite = sprites[variant];
        art.sprite = sprite;
        art.color = Color.white;
        art.transform.localPosition = Vector3.zero;
        art.transform.localScale = Vector3.one;

        float tilt = variant == 1 ? 4f : variant == 2 ? -2.5f : -3f;
        float direction = fromTop ? 1f : -1f;
        art.transform.localRotation = Quaternion.Euler(0f, 0f, direction * (90f + tilt));

        Length = sprite != null ? sprite.bounds.size.x : 6f;
        Thickness = sprite != null ? Mathf.Min(sprite.bounds.size.y * 0.35f, 1.6f) : 1f;

        box.size = new Vector2(Thickness, Length * 0.92f);
        box.offset = new Vector2(0f, fromTop ? -Length * 0.46f : Length * 0.46f);
        return Length;
    }

    static void LoadSprites()
    {
        if (loaded) return;
        loaded = true;

        sprites = new Sprite[3];
        sprites[0] = MakeSprite("Obstacles/BranchGreen", new Vector2(1f, 0.56f));
        sprites[1] = MakeSprite("Obstacles/BranchGold", new Vector2(1f, 0.57f));
        sprites[2] = MakeSprite("Obstacles/BranchMoss", new Vector2(1f, 0.64f));
    }

    static Sprite MakeSprite(string resource, Vector2 pivot)
    {
        Texture2D tex = Resources.Load<Texture2D>(resource);
        if (tex == null)
        {
            Debug.LogWarning("BranchObstacle: missing texture " + resource);
            return null;
        }
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), pivot, 100f, 0,
            SpriteMeshType.FullRect);
    }
}
