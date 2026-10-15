using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds uGUI the way the HTML prototype lays it out: every element is placed
/// by its top-left corner in canvas units (anchor and pivot top-left,
/// anchoredPosition = (x, -y)), sizes are the spec's px values, and text
/// boxes follow CSS line-height so text sits where the browser puts it.
/// </summary>
public static class TutorialUI
{
    public static TMP_FontAsset mplus, lilita, vt323;
    public static Material mplusRegular;  // M PLUS 1 Code at weight 400, as Chrome on macOS draws it
    public static Material mplusMedium;   // M PLUS 1 Code at weight 500 (markers, chips)

    // ---- rects ------------------------------------------------------------

    public static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
        return rt;
    }

    public static void Place(RectTransform rt, float x, float y) => rt.anchoredPosition = new Vector2(x, -y);
    public static void Size(RectTransform rt, float w, float h) => rt.sizeDelta = new Vector2(w, h);
    public static float W(RectTransform rt) => rt.sizeDelta.x;
    public static float H(RectTransform rt) => rt.sizeDelta.y;

    // ---- boxes ------------------------------------------------------------

    /// A filled rect with corner radius r (r = min(w,h)/2 gives a circle).
    public static Image Box(Transform parent, string name, float x, float y, float w, float h, Color c, float r = 0)
    {
        var img = Rect(parent, name, x, y, w, h).gameObject.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        if (r > 0)
        {
            img.sprite = TutorialShapes.Rounded(r);
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = TutorialShapes.Multiplier(r);
        }
        return img;
    }

    /// A hollow rounded outline of thickness t.
    public static Image Ring(Transform parent, string name, float x, float y, float w, float h, Color c, float r, float t)
    {
        var img = Rect(parent, name, x, y, w, h).gameObject.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        img.sprite = TutorialShapes.Ring(r, t);
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = TutorialShapes.Multiplier(r, t);
        return img;
    }

    /// CSS box with a solid border: border colour underneath, fill inset by bw.
    /// Returns the outer box; the fill is its child "fill".
    public static Image Bordered(Transform parent, string name, float x, float y, float w, float h,
                                 Color fill, Color border, float bw, float r)
    {
        var outer = Box(parent, name, x, y, w, h, border, r);
        Box(outer.transform, "fill", bw, bw, w - 2 * bw, h - 2 * bw, fill, Mathf.Max(0, r - bw));
        return outer;
    }

    /// CSS "Npx dashed" border at an exact size.
    public static Image Dashed(Transform parent, string name, float x, float y, float w, float h, Color c, float r, float t)
    {
        var img = Rect(parent, name, x, y, w, h).gameObject.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        img.sprite = TutorialShapes.Dashed(w, h, r, t);
        return img;
    }

    /// box-shadow: 0 offY blur rgba(0,0,0,alpha) behind a w x h rect at (x, y).
    /// Call it before creating the element so it draws underneath.
    public static Image Shadow(Transform parent, float x, float y, float w, float h, float r,
                               float offY, float blur, Color c, bool cutout = false)
    {
        float e = TutorialShapes.ShadowExtent(blur);
        var img = Rect(parent, "shadow", x - e, y + offY - e, w + 2 * e, h + 2 * e).gameObject.AddComponent<Image>();
        img.sprite = TutorialShapes.Shadow(w, h, r, blur, cutout);
        img.color = c;
        img.raycastTarget = false;
        return img;
    }

    public static Image Picture(Transform parent, string name, Sprite s, float x, float y, float w, float h, Color tint)
    {
        var img = Rect(parent, name, x, y, w, h).gameObject.AddComponent<Image>();
        img.sprite = s;
        img.color = tint;
        img.raycastTarget = false;
        return img;
    }

    /// Makes a rect tappable. The Graphic is a fully transparent Image so the
    /// raycaster has something to hit. Fretboard cells pass clickSound = false,
    /// like the game's own fretboard buttons.
    public static Button Tappable(RectTransform rt, UnityEngine.Events.UnityAction onClick, bool clickSound = true)
    {
        var g = rt.GetComponent<Graphic>();
        if (g == null)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0);
            g = img;
        }
        g.raycastTarget = true;
        var b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = g;
        b.transition = Selectable.Transition.None;
        b.onClick.AddListener(onClick);
        if (clickSound) UIClickSound.Attach(b);
        return b;
    }

    // ---- text -------------------------------------------------------------

    /// CSS "line-height: normal" for a font, as a multiple of the font size.
    public static float NormalLineHeight(TMP_FontAsset f) =>
        (f.faceInfo.ascentLine - f.faceInfo.descentLine) / f.faceInfo.pointSize;

    /// Height of one CSS line box. lh = 0 means "normal".
    public static float LineBox(TMP_FontAsset f, float size, float lh = 0) =>
        (lh > 0 ? lh : NormalLineHeight(f)) * size;

    public class TextStyle
    {
        public TMP_FontAsset font;
        public float size;
        public Color color;
        public float lineHeight;      // CSS multiplier, 0 = normal
        public float letterSpacing;   // CSS px
        public bool bold, medium, underline;
        public bool pretty;           // CSS text-wrap: pretty (no one-word last line)
        public TextAlignmentOptions align = TextAlignmentOptions.TopLeft;

        public TextStyle(TMP_FontAsset font, float size, Color color, float lineHeight = 0)
        {
            this.font = font; this.size = size; this.color = color; this.lineHeight = lineHeight;
        }

        public TextStyle With(TextAlignmentOptions a)
        {
            var c = (TextStyle)MemberwiseClone();
            c.align = a;
            return c;
        }
    }

    /// Creates a TMP label. The rect is the CSS box: (x, y) is its top-left,
    /// w the wrap width (0 = single line, no wrap), h the box height
    /// (0 = as many CSS lines as the text needs). Single-line labels in a
    /// fixed-height box should use a centred alignment.
    public static TextMeshProUGUI Text(Transform parent, string name, string text, TextStyle st,
                                       float x, float y, float w = 0, float h = 0)
    {
        var rt = Rect(parent, name, x, y, Mathf.Max(w, 1), Mathf.Max(h, 1));
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        Apply(t, st);
        text = Arrows(text, st);
        t.text = text;
        t.textWrappingMode = w > 0 ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;

        float line = LineBox(st.font, st.size, st.lineHeight);
        if (w <= 0) Size(rt, Measure(t, text, 0).x, rt.sizeDelta.y);
        if (w > 0 && st.pretty) t.text = text = Pretty(t, text, w);
        if (h <= 0) Size(rt, rt.sizeDelta.x, Lines(t, text, rt.sizeDelta.x) * line);

        // CSS centres the glyphs in each line box (half-leading); TMP puts the
        // first baseline one ascender below the top. Shift by the difference.
        bool top = (st.align & (TextAlignmentOptions)0xFF00) == (TextAlignmentOptions)0x100;
        if (top)
        {
            float halfLeading = (line - LineBox(st.font, st.size)) / 2;
            t.margin = new Vector4(0, halfLeading, 0, 0);
        }
        return t;
    }

    /// Chromium's text-wrap: pretty. When greedy wrapping leaves a single
    /// word on the last line, the paragraph is re-broken to minimise the sum
    /// of squared slack over the other lines, with at least two words on the
    /// last line and no extra line. Breaks are returned as explicit newlines.
    static string Pretty(TextMeshProUGUI t, string text, float w)
    {
        int lines = Lines(t, text, w);
        if (lines < 2 || LastLineWords(t) > 1) return text;

        var words = text.Split(' ');
        int n = words.Length;
        if (n < 3) return text;
        var width = new float[n];
        for (int i = 0; i < n; i++) width[i] = Measure(t, words[i], 0).x;
        float space = Measure(t, "a a", 0).x - 2 * Measure(t, "a", 0).x;

        // best[i, k]: min cost to set words[0..i) on k lines; from[i, k]: start of that last line
        var best = new float[n + 1, lines + 1];
        var from = new int[n + 1, lines + 1];
        for (int i = 0; i <= n; i++) for (int k = 0; k <= lines; k++) best[i, k] = float.MaxValue;
        best[0, 0] = 0;
        for (int end = 1; end <= n; end++)
            for (int start = end - 1; start >= 0; start--)
            {
                float lw = 0;
                for (int i = start; i < end; i++) lw += width[i] + (i > start ? space : 0);
                if (lw > w) break;
                bool last = end == n;
                if (last && end - start < 2) continue;
                float cost = last ? 0 : (w - lw) * (w - lw);
                for (int k = 1; k <= lines; k++)
                    if (best[start, k - 1] < float.MaxValue && best[start, k - 1] + cost < best[end, k])
                    {
                        best[end, k] = best[start, k - 1] + cost;
                        from[end, k] = start;
                    }
            }

        int bestK = -1;
        for (int k = 1; k <= lines; k++)
            if (best[n, k] < float.MaxValue && (bestK < 0 || best[n, k] < best[n, bestK])) bestK = k;
        if (bestK < 0) return text;

        var breaks = new System.Collections.Generic.List<int>();
        for (int e = n, k = bestK; k > 0; k--) { breaks.Add(e); e = from[e, k]; }
        breaks.Reverse();
        var sb = new System.Text.StringBuilder();
        int s0 = 0;
        foreach (int e in breaks)
        {
            if (sb.Length > 0) sb.Append('\n');
            sb.Append(string.Join(" ", words, s0, e - s0));
            s0 = e;
        }
        return sb.ToString();
    }

    static int LastLineWords(TextMeshProUGUI t)
    {
        var info = t.textInfo;
        var last = info.lineInfo[info.lineCount - 1];
        var sb = new System.Text.StringBuilder();
        for (int i = last.firstCharacterIndex; i <= last.lastCharacterIndex; i++) sb.Append(info.characterInfo[i].character);
        return sb.ToString().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// The arrows come from the fallback font, which is not monospaced; in
    /// the browser they take one monospace cell like every other character.
    static string Arrows(string text, TextStyle st)
    {
        if (st.font != mplus || text == null) return text;
        return text.Replace("→", "<mspace=0.5em>→</mspace>").Replace("←", "<mspace=0.5em>←</mspace>");
    }

    public static void Apply(TextMeshProUGUI t, TextStyle st)
    {
        t.font = st.font;
        if (st.font == mplus && mplusRegular != null) t.fontSharedMaterial = st.medium ? mplusMedium : mplusRegular;
        t.fontSize = st.size;
        t.enableAutoSizing = false;
        t.color = st.color;
        t.alignment = st.align;
        t.raycastTarget = false;
        t.richText = true;
        t.extraPadding = false;   // true widens quads past the atlas padding: neighbouring glyphs bleed in
        t.characterSpacing = st.letterSpacing / st.size * 100;
        // TMP's line advance is (ascent - descent) * size + lineSpacing * size / 100
        t.lineSpacing = st.lineHeight > 0 ? (st.lineHeight - NormalLineHeight(st.font)) * 100 : 0;
        var fs = FontStyles.Normal;
        if (st.bold) fs |= FontStyles.Bold;
        if (st.underline) fs |= FontStyles.Underline;
        t.fontStyle = fs;
    }

    /// Single-line width (maxWidth = 0) or wrapped size at maxWidth.
    public static Vector2 Measure(TextMeshProUGUI t, string text, float maxWidth)
    {
        var wrap = t.textWrappingMode;
        t.textWrappingMode = maxWidth > 0 ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        var v = t.GetPreferredValues(text, maxWidth > 0 ? maxWidth : 100000, 100000);
        t.textWrappingMode = wrap;
        return v;
    }

    public static int Lines(TextMeshProUGUI t, string text, float width)
    {
        var size = t.rectTransform.sizeDelta;
        t.rectTransform.sizeDelta = new Vector2(width, 100000);
        t.text = text;
        t.ForceMeshUpdate(true, true);
        int n = Mathf.Max(1, t.textInfo.lineCount);
        t.rectTransform.sizeDelta = size;
        return n;
    }

    /// Width a single line of text takes in a given style (no object created).
    public static float TextWidth(string text, TextStyle st, Transform scratchParent)
    {
        var t = Text(scratchParent, "measure", text, st, 0, 0);
        float w = t.rectTransform.sizeDelta.x;
        Object.DestroyImmediate(t.gameObject);
        return w;
    }

    /// CSS shrink-to-fit: the text's own width, or the max width if it wraps.
    public static float FitWidth(string text, TextStyle st, float maxWidth, Transform scratchParent)
    {
        float w = TextWidth(text, st, scratchParent);
        return Mathf.Min(w, maxWidth);
    }
}
