using UnityEngine;
using UnityEngine.UI;

public class FogController : MonoBehaviour
{
    public static FogController Instance;

    RawImage fog;
    Image vignette;
    Image dangerTop;
    Image dangerBottom;
    Color topBase;
    Color bottomBase;
    Camera cam;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
        Build();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Build()
    {
        var canvasGo = new GameObject("FogCanvas", typeof(Canvas), typeof(CanvasScaler));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var root = new GameObject("FogRoot", typeof(RectTransform));
        root.transform.SetParent(canvasGo.transform, false);
        var rootRt = (RectTransform)root.transform;
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        // Drifting mist.
        var fogGo = new GameObject("Mist", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        fogGo.transform.SetParent(root.transform, false);
        fog = fogGo.GetComponent<RawImage>();
        fog.texture = UITheme.FogNoise.texture;
        fog.texture.wrapMode = TextureWrapMode.Repeat;
        fog.color = new Color(0.78f, 1f, 0.92f, 0.075f);
        fog.raycastTarget = false;
        fog.uvRect = new Rect(0f, 0f, 3.2f, 2.1f);
        var fogRt = fog.rectTransform;
        fogRt.anchorMin = Vector2.zero;
        fogRt.anchorMax = Vector2.one;
        fogRt.offsetMin = Vector2.zero;
        fogRt.offsetMax = Vector2.zero;

        // Dust motes.
        for (int i = 0; i < 14; i++)
        {
            var moteGo = new GameObject("Mote", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            moteGo.transform.SetParent(root.transform, false);
            var img = moteGo.GetComponent<Image>();
            img.sprite = UITheme.SoftCircle;
            img.color = new Color(1f, 0.97f, 0.85f, Random.Range(0.05f, 0.12f));
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            float size = Random.Range(5f, 15f);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(Random.Range(-940f, 940f), Random.Range(-520f, 520f));
            var mote = moteGo.AddComponent<MoteUI>();
            mote.velocity = new Vector2(Random.Range(-8f, -2f), Random.Range(2f, 8f));
            mote.bounds = new Vector2(980f, 560f);
        }

        // Vignette.
        var vigGo = new GameObject("Vignette", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        vigGo.transform.SetParent(root.transform, false);
        vignette = vigGo.GetComponent<Image>();
        vignette.sprite = UITheme.Vignette;
        vignette.color = new Color(0.015f, 0.06f, 0.05f, 0.78f);
        vignette.raycastTarget = false;
        var vigRt = vignette.rectTransform;
        vigRt.anchorMin = Vector2.zero;
        vigRt.anchorMax = Vector2.one;
        vigRt.offsetMin = Vector2.zero;
        vigRt.offsetMax = Vector2.zero;

        // Danger bands.
        topBase = new Color(0.06f, 0.17f, 0.15f, 0.5f);
        bottomBase = new Color(0.06f, 0.17f, 0.15f, 0.5f);

        dangerTop = MakeBand(root.transform, "DangerTop", UITheme.GradientDown, true);
        dangerBottom = MakeBand(root.transform, "DangerBottom", UITheme.GradientUp, false);
    }

    Image MakeBand(Transform parent, string name, Sprite sprite, bool top)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = top ? topBase : bottomBase;
        img.raycastTarget = false;
        var rt = img.rectTransform;
        if (top)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, 210f);
        }
        else
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, 210f);
        }
        return img;
    }

    void Update()
    {
        if (fog != null)
            fog.uvRect = new Rect(Time.time * 0.012f, Time.time * 0.005f, 3.2f, 2.1f);

        float danger = 0f;
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.Playing &&
            GameManager.Instance.Player != null && cam != null)
        {
            float y = GameManager.Instance.Player.transform.position.y - cam.transform.position.y;
            float distanceToEdge = Mathf.Min(cam.orthographicSize - y, y + cam.orthographicSize);
            danger = Mathf.Clamp01(1f - distanceToEdge / 1.45f);
        }

        float pulse = 0.85f + 0.15f * Mathf.Sin(Time.unscaledTime * 7f);
        Color top = Color.Lerp(topBase, new Color(UITheme.Coral.r, UITheme.Coral.g, UITheme.Coral.b, 0.72f), danger);
        Color bottom = Color.Lerp(bottomBase, new Color(UITheme.Coral.r, UITheme.Coral.g, UITheme.Coral.b, 0.72f), danger);
        top.a = Mathf.Lerp(topBase.a, 0.72f, danger) * (danger > 0.01f ? pulse : 1f);
        bottom.a = Mathf.Lerp(bottomBase.a, 0.72f, danger) * (danger > 0.01f ? pulse : 1f);

        if (dangerTop != null) dangerTop.color = top;
        if (dangerBottom != null) dangerBottom.color = bottom;
    }
}

public class MoteUI : MonoBehaviour
{
    public Vector2 velocity;
    public Vector2 bounds;

    RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        Vector2 p = rt.anchoredPosition + velocity * Time.unscaledDeltaTime;
        if (p.x < -bounds.x) p.x = bounds.x;
        if (p.x > bounds.x) p.x = -bounds.x;
        if (p.y < -bounds.y) p.y = bounds.y;
        if (p.y > bounds.y) p.y = -bounds.y;
        rt.anchoredPosition = p;
    }
}
