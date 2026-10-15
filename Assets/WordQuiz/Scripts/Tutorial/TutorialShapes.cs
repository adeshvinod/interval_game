using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// White, procedurally generated sprites for the tutorial, cached by shape.
/// They are tinted with a token colour where they are used.
///
/// - Rounded(r): one 9-sliced sprite per corner radius, so the radius stays at
///   r canvas units at any size (a d x d rect with r = d/2 is a circle).
/// - Ring(r, t): the same, hollow, a t-unit stroke (borders over transparency).
/// - Dashed(w, h, r, t): a dashed outline at one exact size, like a CSS
///   "Npx dashed" border (dash = gap = 3t, fitted to a whole number).
/// - Shadow(w, h, r, blur): a CSS box-shadow (sigma = blur / 2), optionally
///   cut out under the shape for the string glow.
/// - Ellipse(w, h): a true ellipse (border-radius: 50%), not a pill.
/// - VerticalGradient(top, bottom): the dark screen background.
///
/// All of them are generated 1 texel per canvas unit, which is 1 px at the
/// 1920x1080 reference, so edges get a 1 px anti-aliased ramp like the browser.
/// </summary>
public static class TutorialShapes
{
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite Rounded(float r)
    {
        int R = Mathf.Max(1, Mathf.CeilToInt(r));
        string key = "rounded" + R;
        if (cache.TryGetValue(key, out var s)) return s;
        int n = 2 * R + 2;
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = RoundedSdf(x + 0.5f, y + 0.5f, n, n, R);
                px[y * n + x] = White(Mathf.Clamp01(0.5f - d));
            }
        return cache[key] = Make(px, n, n, new Vector4(R, R, R, R));
    }

    public static Sprite Ring(float r, float t)
    {
        int R = Mathf.Max(Mathf.CeilToInt(t) + 1, Mathf.CeilToInt(r));
        string key = $"ring{R}_{t}";
        if (cache.TryGetValue(key, out var s)) return s;
        int n = 2 * R + 2;
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = RoundedSdf(x + 0.5f, y + 0.5f, n, n, R);
                float a = Mathf.Clamp01(0.5f - d) - Mathf.Clamp01(0.5f - (d + t));
                px[y * n + x] = White(a);
            }
        return cache[key] = Make(px, n, n, new Vector4(R, R, R, R));
    }

    /// Pixels per unit multiplier that makes a Rounded/Ring sprite show radius r.
    public static float Multiplier(float r, float t = 0)
    {
        int R = t > 0 ? Mathf.Max(Mathf.CeilToInt(t) + 1, Mathf.CeilToInt(r)) : Mathf.Max(1, Mathf.CeilToInt(r));
        return r <= 0 ? 1 : R / r;
    }

    public static Sprite Dashed(float w, float h, float r, float t)
    {
        int W = Mathf.CeilToInt(w), H = Mathf.CeilToInt(h);
        string key = $"dash{W}x{H}_{r}_{t}";
        if (cache.TryGetValue(key, out var s)) return s;

        // centre line of the stroke
        float x0 = t / 2, y0 = t / 2, x1 = w - t / 2, y1 = h - t / 2;
        float rc = Mathf.Max(0, r - t / 2);
        float ex = x1 - x0 - 2 * rc, ey = y1 - y0 - 2 * rc, arc = Mathf.PI / 2 * rc;
        float perimeter = 2 * ex + 2 * ey + 4 * arc;
        float period = 6 * t;
        int count = Mathf.Max(1, Mathf.RoundToInt(perimeter / period));
        period = perimeter / count;

        var px = new Color32[W * H];
        const int SS = 4;
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                int hit = 0;
                for (int sy = 0; sy < SS; sy++)
                    for (int sx = 0; sx < SS; sx++)
                    {
                        float fx = x + (sx + 0.5f) / SS;
                        float fy = H - (y + (sy + 0.5f) / SS); // texture rows run bottom-up; measure from the top
                        float d = RoundedSdf(fx, fy, w, h, r);
                        if (d > 0 || d < -t) continue;
                        float p = PerimeterPos(fx, fy, x0, y0, x1, y1, rc, ex, ey, arc);
                        // a dash is centred on each period start, so the joins look even
                        float m = Mathf.Repeat(p + period / 4, period);
                        if (m < period / 2) hit++;
                    }
                px[y * W + x] = White(hit / (float)(SS * SS));
            }
        return cache[key] = Make(px, W, H, Vector4.zero);
    }

    // distance along the stroke centre line, clockwise from the top-left
    static float PerimeterPos(float px, float py, float x0, float y0, float x1, float y1,
                              float rc, float ex, float ey, float arc)
    {
        float cxL = x0 + rc, cxR = x1 - rc, cyT = y0 + rc, cyB = y1 - rc;
        bool left = px < cxL, right = px > cxR, top = py < cyT, bottom = py > cyB;
        if (top && right) return ex + Angle(px - cxR, py - cyT, -90) * rc;
        if (bottom && right) return ex + arc + ey + Angle(px - cxR, py - cyB, 0) * rc;
        if (bottom && left) return 2 * ex + 2 * arc + ey + Angle(px - cxL, py - cyB, 90) * rc;
        if (top && left) return 2 * ex + 3 * arc + 2 * ey + Angle(px - cxL, py - cyT, 180) * rc;

        // straight edges: pick the nearest one
        float dt = Mathf.Abs(py - y0), db = Mathf.Abs(py - y1), dl = Mathf.Abs(px - x0), dr = Mathf.Abs(px - x1);
        float min = Mathf.Min(Mathf.Min(dt, db), Mathf.Min(dl, dr));
        if (min == dt) return px - cxL;
        if (min == dr) return ex + arc + (py - cyT);
        if (min == db) return ex + 2 * arc + ey + (cxR - px);
        return 2 * ex + 3 * arc + ey + (cyB - py);
    }

    // clockwise angle (radians, screen space y-down) from a start direction
    static float Angle(float dx, float dy, float startDeg)
    {
        float a = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg - startDeg;
        return Mathf.Repeat(a, 360) * Mathf.Deg2Rad;
    }

    public static Sprite Shadow(float w, float h, float r, float blur, bool cutout = false)
    {
        string key = $"shadow{w}x{h}_{r}_{blur}_{cutout}";
        if (cache.TryGetValue(key, out var s)) return s;
        float sigma = blur / 2;
        float ext = Mathf.Ceil(blur * 1.5f) + 2;
        // shadows are soft: half resolution is invisible and 4x cheaper
        float scale = 0.5f;
        int W = Mathf.CeilToInt((w + 2 * ext) * scale), H = Mathf.CeilToInt((h + 2 * ext) * scale);
        var px = new Color32[W * H];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                float fx = (x + 0.5f) / scale - ext, fy = (y + 0.5f) / scale - ext;
                float d = RoundedSdf(fx, fy, w, h, r);
                float a = sigma > 0 ? 0.5f * Erfc(d / (sigma * 1.41421356f)) : Mathf.Clamp01(0.5f - d);
                if (cutout) a *= Mathf.Clamp01(d * scale + 0.5f);
                px[y * W + x] = White(a);
            }
        var sp = Make(px, W, H, Vector4.zero);
        return cache[key] = sp;
    }

    /// How far a Shadow sprite reaches past its shape on each side.
    public static float ShadowExtent(float blur) => Mathf.Ceil(blur * 1.5f) + 2;

    public static Sprite Ellipse(float w, float h)
    {
        int W = Mathf.CeilToInt(w), H = Mathf.CeilToInt(h);
        string key = $"ellipse{W}x{H}";
        if (cache.TryGetValue(key, out var s)) return s;
        var px = new Color32[W * H];
        const int SS = 4;
        float rx = w / 2, ry = h / 2;
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                int hit = 0;
                for (int sy = 0; sy < SS; sy++)
                    for (int sx = 0; sx < SS; sx++)
                    {
                        float dx = (x + (sx + 0.5f) / SS - rx) / rx, dy = (y + (sy + 0.5f) / SS - ry) / ry;
                        if (dx * dx + dy * dy <= 1) hit++;
                    }
                px[y * W + x] = White(hit / (float)(SS * SS));
            }
        return cache[key] = Make(px, W, H, Vector4.zero);
    }

    public static Sprite VerticalGradient(Color top, Color bottom)
    {
        string key = "grad" + ColorUtility.ToHtmlStringRGBA(top) + ColorUtility.ToHtmlStringRGBA(bottom);
        if (cache.TryGetValue(key, out var s)) return s;
        const int H = 256;
        var tex = new Texture2D(1, H, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        for (int y = 0; y < H; y++) tex.SetPixel(0, y, Color.Lerp(bottom, top, (y + 0.5f) / H));
        tex.Apply(false, true);
        return cache[key] = Sprite.Create(tex, new Rect(0, 0, 1, H), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
    }

    // signed distance to a w x h rounded rect with its corner at (0,0); negative inside
    static float RoundedSdf(float x, float y, float w, float h, float r)
    {
        r = Mathf.Min(r, Mathf.Min(w, h) / 2);
        float qx = Mathf.Abs(x - w / 2) - (w / 2 - r);
        float qy = Mathf.Abs(y - h / 2) - (h / 2 - r);
        float ox = Mathf.Max(qx, 0), oy = Mathf.Max(qy, 0);
        return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0) - r;
    }

    // complementary error function (Abramowitz & Stegun 7.1.26)
    static float Erfc(float x)
    {
        float z = Mathf.Abs(x);
        float t = 1 / (1 + 0.3275911f * z);
        float y = t * (0.254829592f + t * (-0.284496736f + t * (1.421413741f + t * (-1.453152027f + t * 1.061405429f)))) * Mathf.Exp(-z * z);
        return x >= 0 ? y : 2 - y;
    }

    static Color32 White(float a) => new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255));

    static Sprite Make(Color32[] px, int w, int h, Vector4 border)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };
        tex.SetPixels32(px);
        tex.Apply(false, true);
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
    }
}
