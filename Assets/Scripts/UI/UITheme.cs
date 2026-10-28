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

    // Warm, hand-painted wood tones used by the HUD, panels and buttons.
    public static readonly Color WoodHoney = Hex("#D9A441");
    public static readonly Color WoodOak = Hex("#8A5A36");
    public static readonly Color WoodPanelTone = Hex("#5A3A24");
    public static readonly Color WoodDarkTone = Hex("#3A2416");
    public static readonly Color WoodDeepTone = Hex("#24150D");
    public static readonly Color LeafGreen = Hex("#6E9A4E");
    public static readonly Color LeafDark = Hex("#3E6034");
    public static readonly Color Iron = Hex("#3A2E26");
    public static readonly Color Parchment = Hex("#EAD9B7");

    public static Font Font { get; private set; }
    public static Font DisplayFont { get; private set; }
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
    public static Sprite WoodPanel { get; private set; }
    public static Sprite WoodPanelSmall { get; private set; }
    public static Sprite WoodButton { get; private set; }
    public static Sprite WoodButtonSmall { get; private set; }
    public static Sprite WoodCard { get; private set; }
    public static Sprite WoodFrame { get; private set; }
    public static Sprite Leaf { get; private set; }
    public static Sprite Nail { get; private set; }
    public static Sprite Brush { get; private set; }

    static bool initialized;

    public static void Init()
    {
        if (initialized) return;
        initialized = true;

        Font = Resources.Load<Font>("Fonts/BeVietnamPro-SemiBold");
        DisplayFont = Resources.Load<Font>("Fonts/BeVietnamPro-ExtraBold");
        if (Font == null)
            Font = Resources.Load<Font>("Fonts/LiberationSans");
        if (Font == null)
        {
            // Unity 6 built-in fallback name.
            Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        if (DisplayFont == null) DisplayFont = Font;

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

        WoodPanel = MakeWood(256, 56, 11);
        WoodPanelSmall = MakeWood(128, 26, 23);
        WoodButton = MakeWood(128, 26, 37);
        WoodButtonSmall = MakeWood(64, 15, 41);
        WoodCard = MakeWood(128, 22, 53);
        WoodFrame = MakeWoodFrame(128, 22);
        Leaf = MakeLeaf(64);
        Nail = MakeNail(16);
        Brush = MakeBrush(128, 28);
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

    // Neutral warm wood. The final colour is supplied by Image.color, so the
    // same grain/bevel sprite can become honey, oak, moss or dark walnut.
    static Sprite MakeWood(int size, int border, int seed)
    {
        var tex = NewTex(size, size);
        var px = new Color32[size * size];
        var rng = new System.Random(seed);
        float half = size * 0.5f;
        float radius = size * 0.18f;
        Color low = new Color(0.58f, 0.47f, 0.35f);
        Color high = new Color(0.86f, 0.75f, 0.60f);
        Color dark = new Color(0.14f, 0.085f, 0.055f);
        Color light = new Color(0.98f, 0.90f, 0.78f);

        const int knotCount = 3;
        var kx = new float[knotCount];
        var ky = new float[knotCount];
        var kr = new float[knotCount];
        for (int i = 0; i < knotCount; i++)
        {
            kx[i] = size * (0.15f + 0.70f * (float)rng.NextDouble());
            ky[i] = size * (0.15f + 0.70f * (float)rng.NextDouble());
            kr[i] = size * (0.030f + 0.040f * (float)rng.NextDouble());
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float fx = x + 0.5f;
                float fy = y + 0.5f;
                float dx = Mathf.Abs(fx - half) - (half - radius);
                float dy = Mathf.Abs(fy - half) - (half - radius);
                float outside = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) +
                                           Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f));
                float inside = Mathf.Min(Mathf.Max(dx, dy), 0f);
                float dist = outside + inside - radius;
                float wobble = (Mathf.Sin(fx * 0.37f + seed * 1.7f) +
                                Mathf.Cos(fy * 0.29f - seed * 1.1f)) * 0.55f;
                float alpha = Mathf.Clamp01(0.5f - (dist + wobble));
                if (alpha <= 0.001f)
                {
                    px[y * size + x] = new Color32(0, 0, 0, 0);
                    continue;
                }

                float t = fy / size;
                Color c = Color.Lerp(low, high, t);

                // Long horizontal grain plus fine saw marks.
                float grain = Mathf.Sin(fy * 0.23f + Mathf.Sin(fx * 0.021f + seed) * 2.2f + seed * 0.9f);
                grain = grain * 0.5f + 0.5f;
                float fine = Mathf.Sin(fy * 1.55f + fx * 0.013f + seed * 0.5f) * 0.5f + 0.5f;
                c *= 0.86f + grain * 0.18f + fine * 0.08f;

                // A few soft knots make the planks feel cut rather than printed.
                for (int i = 0; i < knotCount; i++)
                {
                    float nx = (fx - kx[i]) / kr[i];
                    float ny = (fy - ky[i]) / (kr[i] * 0.55f);
                    float nd = Mathf.Sqrt(nx * nx + ny * ny);
                    if (nd < 1.6f)
                    {
                        float ring = Mathf.Sin(nd * 10.5f + seed) * 0.5f + 0.5f;
                        float fade = Mathf.Clamp01(1.6f - nd);
                        c *= 1f - 0.13f * ring * fade;
                        if (nd < 0.22f) c = Color.Lerp(c, dark, 0.45f);
                    }
                }

                // Hand-cut outline and bevel.
                float edge = -dist;
                if (edge < 2.0f)
                    c = Color.Lerp(c, dark, 0.92f);
                else if (edge < 4.0f)
                    c = Color.Lerp(c, dark, 0.38f);
                else if (edge < 14.0f)
                {
                    float k = Mathf.Clamp01(1f - (edge - 4f) / 10f);
                    bool top = fy > half;
                    bool left = fx < half;
                    if (top || left)
                        c = Color.Lerp(c, light, 0.16f * k);
                    else
                        c = Color.Lerp(c, dark, 0.20f * k);
                }

                // Fine carved frame line, only where it will not be stretched.
                float line = size * 0.065f;
                if (size >= 120 && edge > line && edge < line + 1.6f)
                    c = Color.Lerp(c, dark, 0.16f);

                px[y * size + x] = new Color32(
                    (byte)(Mathf.Clamp01(c.r) * 255f),
                    (byte)(Mathf.Clamp01(c.g) * 255f),
                    (byte)(Mathf.Clamp01(c.b) * 255f),
                    (byte)(alpha * 255f));
            }
        }

        tex.SetPixels32(px);
        tex.Apply();
        return ToSprite(tex, new Vector4(border, border, border, border));
    }

    static Sprite MakeWoodFrame(int size, int border)
    {
        var tex = NewTex(size, size);
        var px = new Color32[size * size];
        float half = size * 0.5f;
        float radius = size * 0.18f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - half) - (half - radius);
                float dy = Mathf.Abs(y + 0.5f - half) - (half - radius);
                float outside = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) +
                                           Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f));
                float inside = Mathf.Min(Mathf.Max(dx, dy), 0f);
                float edge = -(outside + inside - radius);

                float a = Mathf.Clamp01(1f - Mathf.Abs(edge - size * 0.075f) / 1.3f);
                float b = Mathf.Clamp01(1f - Mathf.Abs(edge - size * 0.125f) / 0.9f);
                float alpha = Mathf.Max(a, b * 0.55f);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return ToSprite(tex, new Vector4(border, border, border, border));
    }

    static Sprite MakeLeaf(int size)
    {
        var tex = NewTex(size, size);
        var px = new Color32[size * size];
        float cx = size * 0.5f;
        float cy = size * 0.5f;
        Color dark = Hex("#2F4D28");
        Color mid = Hex("#5E8C46");
        Color light = Hex("#8CB968");

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f - cx) / (size * 0.44f);
                float v = (y + 0.5f - cy) / (size * 0.21f);
                if (Mathf.Abs(u) > 1.02f) continue;

                float width = Mathf.Pow(Mathf.Max(0f, 1f - Mathf.Abs(u)), 0.55f);
                float bend = 0.16f * u * u;
                float d = Mathf.Abs(v - bend) / Mathf.Max(0.06f, width * 0.92f);
                float alpha = Mathf.Clamp01((1f - d) * 9f);
                if (alpha <= 0.001f) continue;

                Color c = Color.Lerp(dark, light, Mathf.Clamp01((v + width * 0.5f) / Mathf.Max(0.1f, width)));
                c = Color.Lerp(c, mid, 0.35f);
                if (Mathf.Abs(v - bend) < 0.045f && Mathf.Abs(u) < 0.92f)
                    c = Color.Lerp(c, dark, 0.55f);
                if (d > 0.72f && v > bend)
                    c = Color.Lerp(c, light, 0.35f);

                px[y * size + x] = new Color32(
                    (byte)(Mathf.Clamp01(c.r) * 255f),
                    (byte)(Mathf.Clamp01(c.g) * 255f),
                    (byte)(Mathf.Clamp01(c.b) * 255f),
                    (byte)(alpha * 255f));
            }
        }

        // Short stem.
        int stemStart = Mathf.RoundToInt(size * 0.02f);
        int stemEnd = Mathf.RoundToInt(size * 0.16f);
        int stemY = Mathf.RoundToInt(cy);
        for (int x = stemStart; x < stemEnd; x++)
        {
            for (int y = stemY - 1; y <= stemY + 1; y++)
            {
                int i = y * size + x;
                if (i >= 0 && i < px.Length)
                    px[i] = new Color32(63, 93, 49, 255);
            }
        }

        tex.SetPixels32(px);
        tex.Apply();
        return ToSprite(tex);
    }

    static Sprite MakeNail(int size)
    {
        var tex = NewTex(size, size);
        var px = new Color32[size * size];
        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        float r = size * 0.44f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Vector2.Distance(p, c);
                if (d > r) continue;
                float alpha = Mathf.Clamp01(r - d + 0.5f);
                Color col = Hex("#4A3D33");
                if (d > r * 0.70f) col = Hex("#18120E");
                if (Vector2.Distance(p, c + new Vector2(-r * 0.26f, r * 0.26f)) < r * 0.26f)
                    col = Hex("#A08A72");
                px[y * size + x] = new Color32(
                    (byte)(col.r * 255f), (byte)(col.g * 255f), (byte)(col.b * 255f),
                    (byte)(alpha * 255f));
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return ToSprite(tex);
    }

    static Sprite MakeBrush(int w, int h)
    {
        var tex = NewTex(w, h);
        var px = new Color32[w * h];
        float half = (h - 1) * 0.5f;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float u = (x + 0.5f) / w;
                float v = (y + 0.5f - half) / half;
                float taper = Mathf.Pow(Mathf.Sin(Mathf.PI * u), 0.42f);
                float wobble = Mathf.Sin(u * 19f) * 0.05f + Mathf.Sin(u * 7f + 1.4f) * 0.035f;
                float d = Mathf.Abs(v + wobble) / Mathf.Max(0.08f, taper * 0.86f);
                float alpha = Mathf.Clamp01((1f - d) * 5f);
                if (alpha <= 0.001f) continue;
                float bristle = Mathf.Sin(y * 2.3f + x * 0.21f) * 0.5f + 0.5f;
                alpha *= 0.72f + bristle * 0.28f;
                px[y * w + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
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
        bool large = size.y >= 180f || size.x >= 500f;
        Sprite sprite = large ? WoodPanel : WoodPanelSmall;
        Color tone = PanelTone(color, size);
        var img = CreateImage(parent, name, sprite, tone, true);
        img.type = Image.Type.Sliced;
        var rt = img.rectTransform;
        Center(rt, size);
        if (shadow)
        {
            var sh = img.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.42f);
            sh.effectDistance = new Vector2(0f, -7f);
            sh.useGraphicAlpha = true;
        }

        AddNails(rt, size);
        if (large && size.x >= 500f && size.y >= 400f)
            AddLeafSprig(rt);
        return rt;
    }

    static Color PanelTone(Color color, Vector2 size)
    {
        if (color.r > 0.60f && color.g > 0.40f && color.b < 0.55f)
            return WoodHoney;
        return (size.y >= 180f || size.x >= 500f) ? WoodPanelTone : WoodDarkTone;
    }

    static Color ButtonTone(Color bg)
    {
        float lum = bg.r * 0.30f + bg.g * 0.59f + bg.b * 0.11f;
        if (bg.r > 0.60f && bg.g > 0.40f && bg.b < 0.55f)
            return WoodHoney;
        if (lum < 0.13f || bg.a < 0.80f)
            return WoodDarkTone;
        return WoodOak;
    }

    public static void AddNails(RectTransform panel, Vector2 size)
    {
        if (size.x < 100f || size.y < 40f) return;
        float inset = Mathf.Clamp(Mathf.Min(size.x, size.y) * 0.16f, 8f, 18f);
        float nailSize = Mathf.Clamp(Mathf.Min(size.x, size.y) * 0.09f, 7f, 13f);
        AddNail(panel, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(inset, inset), nailSize);
        AddNail(panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(inset, -inset), nailSize);
        AddNail(panel, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-inset, inset), nailSize);
        AddNail(panel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-inset, -inset), nailSize);
    }

    static void AddNail(Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, float size)
    {
        var img = CreateImage(parent, "Nail", Nail, Color.white, false);
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(size, size);
    }

    static void AddLeafSprig(RectTransform panel)
    {
        var leaf = CreateImage(panel, "Leaf", Leaf, Color.white, false);
        var rt = leaf.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-8f, -6f);
        rt.sizeDelta = new Vector2(96f, 56f);
        rt.localRotation = Quaternion.Euler(0f, 0f, -18f);

        var leaf2 = CreateImage(panel, "LeafSmall", Leaf, new Color(0.78f, 0.9f, 0.70f, 0.92f), false);
        var rt2 = leaf2.rectTransform;
        rt2.anchorMin = rt2.anchorMax = new Vector2(1f, 0f);
        rt2.pivot = new Vector2(1f, 0f);
        rt2.anchoredPosition = new Vector2(-74f, 22f);
        rt2.sizeDelta = new Vector2(56f, 32f);
        rt2.localRotation = Quaternion.Euler(0f, 0f, 26f);
    }

    public static Text CreateText(Transform parent, string name, string content, int fontSize, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        bool display = (style & FontStyle.Bold) == FontStyle.Bold;
        t.font = display ? DisplayFont : Font;
        t.text = content;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = anchor;
        t.fontStyle = FontStyle.Normal;
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
        bool small = size.x < 120f || size.y < 54f;
        img.sprite = small ? WoodButtonSmall : WoodButton;
        img.type = Image.Type.Sliced;
        img.color = ButtonTone(bg);

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;
        var cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.04f, 1.04f, 1.04f, 1f);
        cb.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        cb.selectedColor = Color.white;
        cb.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.5f);
        cb.fadeDuration = 0.08f;
        btn.colors = cb;

        var drop = go.AddComponent<Shadow>();
        drop.effectColor = new Color(0f, 0f, 0f, 0.38f);
        drop.effectDistance = new Vector2(0f, -5f);
        drop.useGraphicAlpha = true;

        var labelText = CreateText(go.transform, "Label", label, fontSize, textColor, TextAnchor.MiddleCenter, FontStyle.Bold);
        var lrt = labelText.rectTransform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = new Vector2(18f, 6f);
        lrt.offsetMax = new Vector2(-18f, -6f);

        bool darkText = textColor.r + textColor.g + textColor.b < 1.25f;
        var labelShadow = labelText.gameObject.AddComponent<Shadow>();
        labelShadow.effectColor = darkText
            ? new Color(1f, 0.90f, 0.70f, 0.18f)
            : new Color(0f, 0f, 0f, 0.45f);
        labelShadow.effectDistance = new Vector2(1f, -2f);
        labelShadow.useGraphicAlpha = true;

        if (border)
        {
            var ol = go.AddComponent<Outline>();
            ol.effectColor = WithAlpha(Gold, 0.45f);
            ol.effectDistance = new Vector2(2f, -2f);
            ol.useGraphicAlpha = true;
        }

        AddNails(rt, size);

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
