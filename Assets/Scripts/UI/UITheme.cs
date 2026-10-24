using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class UITheme
{
    // ---------- Palette ----------
    public static readonly Color Bg = Hex("#0A1614");
    public static readonly Color BgSoft = Hex("#122622");
    public static readonly Color Panel = Hex("#10231F");
    public static readonly Color PanelLight = Hex("#18352D");
    public static readonly Color PanelDark = Hex("#0B1B18");
    public static readonly Color Gold = Hex("#F2C14E");
    public static readonly Color GoldDark = Hex("#C9962E");
    public static readonly Color Mint = Hex("#7FE3C3");
    public static readonly Color MintDark = Hex("#3D8E78");
    public static readonly Color Coral = Hex("#FF7A8A");
    public static readonly Color Cream = Hex("#F7F0E3");
    public static readonly Color Muted = Hex("#A6BDB4");
    public static readonly Color Danger = Hex("#E4575F");

    public static Font Font { get; private set; }
    public static Sprite Rounded { get; private set; }
    public static Sprite RoundedSmall { get; private set; }
    public static Sprite HandDrawn { get; private set; }
    public static Sprite Circle { get; private set; }
    public static Sprite SoftCircle { get; private set; }
    public static Sprite Coin { get; private set; }
    public static Sprite GradientDown { get; private set; }
    public static Sprite GradientUp { get; private set; }
    public static Sprite Vignette { get; private set; }
    public static Sprite FogNoise { get; private set; }
    public static Sprite Ring { get; private set; }

    static bool initialized;

    public static void Init()
    {
        if (initialized) return;
        initialized = true;

        Font = Resources.Load<Font>("Fonts/LiberationSans");
        if (Font == null)
        {
            // Unity 6 built-in fallback name.
            Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        Rounded = MakeRounded(64, 20, 24);
        RoundedSmall = MakeRounded(48, 14, 18);
        HandDrawn = MakeHandDrawn(128);
        Circle = MakeCircle(64);
        SoftCircle = MakeSoftCircle(128);
        Coin = MakeCoin(64);
        GradientDown = MakeGradient(8, 128, true);
        GradientUp = MakeGradient(8, 128, false);
        Vignette = MakeVignette(256);
        FogNoise = MakeFog(256);
        Ring = MakeRing(128, 6);
    }

    public static Color Hex(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }

    public static Color WithAlpha(Color c, float a)
    {
        c.a = a;
        return c;
    }

    // ---------- Sprite generation ----------
    static Texture2D NewTex(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    static Sprite ToSprite(Texture2D tex, Vector4 border = default)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
    }

    static Sprite MakeRounded(int size, int radius, int border)
    {
        var tex = NewTex(size, size);
        var px = new Color32[size * size];
        float half = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - half) - (half - radius);
                float dy = Mathf.Abs(y + 0.5f - half) - (half - radius);
                float outside = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f));
                float inside = Mathf.Min(Mathf.Max(dx, dy), 0f);
                float dist = outside + inside - radius;
                float a = Mathf.Clamp01(0.5f - dist);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return ToSprite(tex, new Vector4(border, border, border, border));
    }

    static Sprite MakeHandDrawn(int size)
    {
        var tex = NewTex(size, size);
        var px = new Color32[size * size];
        float half = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x + 0.5f - half) / half;
                float ny = (y + 0.5f - half) / half;
                float wobble = Mathf.Sin(x * 0.31f) * 0.018f + Mathf.Cos(y * 0.27f) * 0.014f;
                float edge = Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny)) + wobble;
                float alpha = Mathf.Clamp01((1f - edge) * 18f);
                if (alpha <= 0f)
                {
                    px[y * size + x] = new Color32(0, 0, 0, 0);
                    continue;
                }

                bool ink = edge > 0.88f;
                px[y * size + x] = ink
                    ? new Color32(48, 57, 52, (byte)(alpha * 190f))
                    : new Color32(255, 255, 255, (byte)(alpha * 255f));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return ToSprite(tex, new Vector4(28f, 28f, 28f, 28f));
    }

    static Sprite MakeCircle(int size)
    {
        var tex = NewTex(size, size);
        var px = new Color32[size * size];
        float r = size * 0.5f - 1f;
        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                float a = Mathf.Clamp01(r - d + 0.5f);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return ToSprite(tex);
    }

    static Sprite MakeSoftCircle(int size)
    {
        var tex = NewTex(size, size);
        var px = new Color32[size * size];
        float r = size * 0.5f;
        Vector2 c = new Vector2(r, r);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c) / r;
                float a = Mathf.Clamp01(1f - d);
                a = a * a * (3f - 2f * a);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return ToSprite(tex);
    }

    static Sprite MakeRing(int size, int thickness)
    {
        var tex = NewTex(size, size);
        var px = new Color32[size * size];
        float outer = size * 0.5f - 1f;
        float inner = outer - thickness;
        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                float a = Mathf.Clamp01(outer - d + 0.5f) * Mathf.Clamp01(d - inner + 0.5f);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return ToSprite(tex);
    }

    static Sprite MakeCoin(int size)
    {
        var tex = NewTex(size, size);
        var px = new Color32[size * size];
        float r = size * 0.5f - 1f;
        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        Color rim = Hex("#B7791F");
        Color body = Hex("#F2C14E");
        Color shine = Hex("#FFE8A3");
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Vector2.Distance(p, c);
                float a = Mathf.Clamp01(r - d + 0.5f);
                Color col = body;
                if (d > r * 0.82f) col = rim;
                else if (Vector2.Distance(p, c + new Vector2(-r * 0.28f, r * 0.28f)) < r * 0.28f) col = shine;
                px[y * size + x] = new Color32((byte)(col.r * 255), (byte)(col.g * 255), (byte)(col.b * 255), (byte)(a * 255));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return ToSprite(tex);
    }

    static Sprite MakeGradient(int w, int h, bool topOpaque)
    {
        var tex = NewTex(w, h);
        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
        {
            float t = (float)y / (h - 1);
            float a = topOpaque ? t : 1f - t;
            a = Mathf.SmoothStep(0f, 1f, a);
            for (int x = 0; x < w; x++)
                px[y * w + x] = new Color32(255, 255, 255, (byte)(a * 255));
        }
        tex.SetPixels32(px);
        tex.Apply();
        return ToSprite(tex);
    }

    static Sprite MakeVignette(int size)
    {
        var tex = NewTex(size, size);
        var px = new Color32[size * size];
        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        float max = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c) / max;
                float a = Mathf.SmoothStep(0.28f, 0.92f, d);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return ToSprite(tex);
    }

    static Sprite MakeFog(int size)
    {
        var tex = NewTex(size, size);
        var px = new Color32[size * size];
        var rng = new System.Random(1337);
        int blobs = 90;
        float[] alpha = new float[size * size];
        for (int b = 0; b < blobs; b++)
        {
            float cx = (float)rng.NextDouble() * size;
            float cy = (float)rng.NextDouble() * size;
            float rad = Mathf.Lerp(18f, 62f, (float)rng.NextDouble());
            float strength = Mathf.Lerp(0.06f, 0.24f, (float)rng.NextDouble());
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - cx);
                    float dy = Mathf.Abs(y - cy);
                    dx = Mathf.Min(dx, size - dx);
                    dy = Mathf.Min(dy, size - dy);
                    float d = Mathf.Sqrt(dx * dx + dy * dy) / rad;
                    if (d >= 1f) continue;
                    float fall = 1f - d;
                    fall = fall * fall * (3f - 2f * fall);
                    alpha[y * size + x] += fall * strength;
                }
            }
        }
        for (int i = 0; i < alpha.Length; i++)
        {
            float a = Mathf.Clamp01(alpha[i]);
            px[i] = new Color32(255, 255, 255, (byte)(a * 255));
        }
        tex.SetPixels32(px);
        tex.Apply();
        return ToSprite(tex);
    }

    // ---------- Layout helpers ----------
    public static RectTransform CreateRect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    public static void Stretch(RectTransform rt, float padding = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(padding, padding);
        rt.offsetMax = new Vector2(-padding, -padding);
    }

    public static void Center(RectTransform rt, Vector2 size, Vector2 offset = default)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = offset;
        rt.sizeDelta = size;
    }

    public static void TopLeft(RectTransform rt, Vector2 size, Vector2 offset)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(offset.x, -offset.y);
        rt.sizeDelta = size;
    }

    public static void TopRight(RectTransform rt, Vector2 size, Vector2 offset)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-offset.x, -offset.y);
        rt.sizeDelta = size;
    }

    public static void BottomCenter(RectTransform rt, Vector2 size, float y)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = size;
    }

    // ---------- Widgets ----------
    public static Image CreateImage(Transform parent, string name, Sprite sprite, Color color, bool raycast = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = raycast;
        if (sprite != null && sprite.border != Vector4.zero)
            img.type = Image.Type.Sliced;
        return img;
    }

    public static RectTransform CreatePanel(Transform parent, string name, Vector2 size, Color color, bool shadow = true)
    {
        var img = CreateImage(parent, name, Rounded, color, true);
        img.type = Image.Type.Sliced;
        var rt = img.rectTransform;
        Center(rt, size);
        if (shadow)
        {
            var sh = img.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.35f);
            sh.effectDistance = new Vector2(0f, -7f);
            sh.useGraphicAlpha = true;
        }
        return rt;
    }

    public static Text CreateText(Transform parent, string name, string content, int fontSize, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = Font;
        t.text = content;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = anchor;
        t.fontStyle = style;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.supportRichText = true;
        t.raycastTarget = false;
        t.alignByGeometry = true;
        return t;
    }

    public static Text CreateShadowText(Transform parent, string name, string content, int fontSize, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal)
    {
        var t = CreateText(parent, name, content, fontSize, color, anchor, style);
        var sh = t.gameObject.AddComponent<Shadow>();
        sh.effectColor = new Color(0f, 0f, 0f, 0.55f);
        sh.effectDistance = new Vector2(2f, -3f);
        sh.useGraphicAlpha = true;
        return t;
    }

    public static Button CreateButton(Transform parent, string name, string label, Vector2 size, Color bg, Color textColor, UnityAction onClick, int fontSize = 28, bool border = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        if (size != Vector2.zero)
            rt.sizeDelta = size;

        var img = go.GetComponent<Image>();
        img.sprite = HandDrawn;
        img.type = Image.Type.Sliced;
        img.color = bg;

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;
        var cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.03f, 1.03f, 1.03f, 1f);
        cb.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        cb.selectedColor = Color.white;
        cb.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.5f);
        cb.fadeDuration = 0.08f;
        btn.colors = cb;

        var ink = go.AddComponent<Outline>();
        ink.effectColor = new Color(0.04f, 0.07f, 0.06f, 0.55f);
        ink.effectDistance = new Vector2(1.5f, -1.5f);
        ink.useGraphicAlpha = true;

        var labelText = CreateText(go.transform, "Label", label, fontSize, textColor, TextAnchor.MiddleCenter, FontStyle.Bold);
        var lrt = labelText.rectTransform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(16f, 6f);
        lrt.offsetMax = new Vector2(-16f, -6f);

        if (border)
        {
            var ol = go.AddComponent<Outline>();
            ol.effectColor = WithAlpha(Mint, 0.4f);
            ol.effectDistance = new Vector2(2f, -2f);
            ol.useGraphicAlpha = true;
        }

        go.AddComponent<ButtonFX>();
        btn.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
            onClick?.Invoke();
        });

        if (size != Vector2.zero)
        {
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = size.x;
            le.preferredHeight = size.y;
        }
        return btn;
    }

    public static void SetButtonLabel(Button button, string label)
    {
        var t = button.GetComponentInChildren<Text>();
        if (t != null) t.text = label;
    }

    public static void AddOutline(Graphic g, Color color, Vector2 distance)
    {
        var o = g.gameObject.AddComponent<Outline>();
        o.effectColor = color;
        o.effectDistance = distance;
        o.useGraphicAlpha = true;
    }
}
