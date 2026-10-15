using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TutorialUI;
using T = TutorialTheme;

/// <summary>
/// The 30-screen tutorial opened from the "?" on the main menu.
///
/// Everything is built at runtime from tutorial_screens.json, following
/// design_handoff_fretnoodle_tutorial/README.md: a 1920x1080 stage (scaled to
/// fit, letterboxed in ink), light title screens and dark teaching screens,
/// with the fretboard drawn by TutorialBoard. Positions are the spec's px
/// values measured from the stage's top-left.
///
/// The last screen index is kept in PlayerPrefs, so reopening resumes there.
/// </summary>
public class TutorialController : MonoBehaviour
{
    [Serializable]
    public class Guide
    {
        public string key;      // boy, duchess, mohawk, riff, bluesman
        public Sprite sprite;   // the game's character art
        public Rect crop;       // opaque bounds in texture pixels (set by the editor setup)
        [NonSerialized] public Sprite cropped;
    }

    [SerializeField] TextAsset screensJson;
    [SerializeField] TMP_FontAsset mplusFont, lilitaFont, vt323Font;
    [SerializeField] Guide[] guides;
    [SerializeField] RectTransform stage;
    [SerializeField] GameSettings gameSettings;
    [SerializeField] NotesGameData notesGameData;
    [SerializeField] string menuScene = "notes-intervals-menu";
    [SerializeField] string notesLearnScene = "learnMode_Notes";
    [Tooltip("Block NEXT on a Try-it screen until it is solved (off in the design).")]
    [SerializeField] bool gateTryIts;

    public const string PrefKey = "fretnoodle-tut-idx";

    Dictionary<string, object> data, widgets;
    List<Dictionary<string, object>> screens;
    int index;
    readonly Dictionary<string, string> found = new Dictionary<string, string>();   // "s-f" -> label
    TutorialBoard.Mark? wrong;
    int chord;
    RectTransform scratch;

    void Awake()
    {
        mplus = mplusFont;
        lilita = lilitaFont;
        vt323 = vt323Font;
        // TMP's SDF draws M PLUS lighter than Chrome on macOS does; dilate to match
        mplusRegular = new Material(mplusFont.material) { name = "MPLUS1Code Regular" };
        mplusRegular.SetFloat(ShaderUtilities.ID_WeightNormal, 0.3f);
        mplusMedium = new Material(mplusFont.material) { name = "MPLUS1Code Medium" };
        mplusMedium.SetFloat(ShaderUtilities.ID_WeightNormal, 0.5f);

        data = (Dictionary<string, object>)TutorialJson.Parse(screensJson.text);
        widgets = data.Dict("widgets");
        screens = data.List("screens").Cast<Dictionary<string, object>>().ToList();
        TutorialBoard.Notes = data.Strings("notes").ToArray();
        TutorialBoard.Intervals = data.Strings("intervals").ToArray();
        TutorialBoard.OpenStrings = data.Strings("tuningTopToBottom").ToArray();

        foreach (var g in guides)
            if (g.sprite != null)
            {
                var c = g.crop.width > 0 ? g.crop : g.sprite.rect;
                g.cropped = Sprite.Create(g.sprite.texture, c, new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect);
            }

        index = Mathf.Clamp(PlayerPrefs.GetInt(PrefKey, 0), 0, screens.Count - 1);
        Render();
    }

    void Update()
    {
        FitStage();
        if (Input.GetKeyDown(KeyCode.RightArrow)) Go(1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Go(-1);
        if (Input.GetKeyDown(KeyCode.Escape)) Close();   // Android back
    }

    /// Scale the 1920x1080 stage to fit the canvas, centred (letterbox = ink).
    void FitStage()
    {
        var canvas = (RectTransform)stage.parent;
        float s = Mathf.Min(canvas.rect.width / 1920f, canvas.rect.height / 1080f);
        if (!Mathf.Approximately(stage.localScale.x, s)) stage.localScale = new Vector3(s, s, 1);
    }

    // for the editor capture tool
    public RectTransform Stage => stage;
    public int Index => index;
    public int ScreenCount => screens.Count;
    public void Show(int i) => Jump(i);
    public void FitStageNow() => FitStage();

    // ---- navigation --------------------------------------------------------

    void Jump(int i)
    {
        index = Mathf.Clamp(i, 0, screens.Count - 1);
        PlayerPrefs.SetInt(PrefKey, index);
        PlayerPrefs.Save();
        found.Clear();
        wrong = null;
        Render();
    }

    void Go(int d)
    {
        if (d > 0 && gateTryIts && TapConfig(Screen) != null && !TryDone()) return;
        if (index + d < 0 || index + d >= screens.Count) return;
        Jump(index + d);
    }

    void Close() => SceneManager.LoadScene(menuScene);

    /// "PLAY NOTES L1": exactly what the Notes level-select L1 button does.
    void PlayNotesLevel1()
    {
        if (gameSettings != null) gameSettings.SetLearningMode((int)LearningMode.Notes);
        if (notesGameData != null) notesGameData.SelectLevel(0);   // Notes levels are 0-indexed
        SceneManager.LoadScene(notesLearnScene);
    }

    Dictionary<string, object> Screen => screens[index];

    static Dictionary<string, object> TapConfig(Dictionary<string, object> s)
    {
        var boards = s.List("boards");
        return boards != null && boards.Count > 0 ? ((Dictionary<string, object>)boards[0]).Dict("tap") : null;
    }

    bool TryDone()
    {
        var t = TapConfig(Screen);
        return t != null && t.Strings("need").All(l => found.ContainsValue(l));
    }

    void Tap(int s, int f)
    {
        var t = TapConfig(Screen);
        string label = t.Str("mode") == "note"
            ? TutorialBoard.NoteAt(s, f)
            : TutorialBoard.IntervalOf(Root(t).x, Root(t).y, s, f);
        if (t.Has("root") && Root(t).x == s && Root(t).y == f) return;
        bool okString = !t.Has("strings") || t.List("strings").Any(o => (int)(double)o == s);
        bool ok = okString && (t.Has("accept") ? t.Strings("accept") : t.Strings("need")).Contains(label);
        if (ok) { found[s + "-" + f] = label; wrong = null; }
        else wrong = new TutorialBoard.Mark(s, f, label, 'w');
        Render();
    }

    static Vector2Int Root(Dictionary<string, object> t)
    {
        var r = t.List("root");
        return r == null ? new Vector2Int(-1, -1) : new Vector2Int((int)(double)r[0], (int)(double)r[1]);
    }

    // ---- rendering ---------------------------------------------------------

    void Render()
    {
        // Render can run inside a tapped button's onClick, so tear down with a
        // deferred Destroy, not DestroyImmediate.
        for (int i = stage.childCount - 1; i >= 0; i--)
        {
            var old = stage.GetChild(i).gameObject;
            old.SetActive(false);
            old.transform.SetParent(null, false);
            Destroy(old);
        }
        // TMP only lays out active objects, so the measuring area stays active;
        // whatever is measured in it is destroyed straight away.
        scratch = Rect(stage, "scratch", 0, 0, 0, 0);

        var s = Screen;
        if (s.Int("light") == 1) BuildLight(s);
        else BuildDark(s);
        BuildChrome(s);
        Destroy(scratch.gameObject);
    }

    Guide FindGuide(string key) => guides.FirstOrDefault(g => g.key == key);

    float Aspect(Guide g) => g?.cropped == null ? 1 : g.cropped.rect.width / g.cropped.rect.height;

    // ---- chrome: top bar, PREV, NEXT -----------------------------------------

    void BuildChrome(Dictionary<string, object> s)
    {
        var close = Box(stage, "close", 60, 36, 96, 96, T.teal, 18);
        Text(close.transform, "x", "×", new TextStyle(mplus, 52, T.white) { align = TextAlignmentOptions.Center }, 0, 0, 96, 96);
        Tappable(close.rectTransform, Close);

        string counter = (index + 1).ToString("00") + " / " + screens.Count;
        var counterStyle = new TextStyle(mplus, 30, T.white) { align = TextAlignmentOptions.Center };
        float counterW = TextWidth(counter, counterStyle, scratch) + 56;
        var pill = Box(stage, "counter", 1860 - counterW, 40, counterW, 88, T.teal, 22);
        Text(pill.transform, "text", counter, counterStyle, 0, 0, counterW, 88);

        var parts = data.Strings("parts");
        var tabStyle = new TextStyle(mplus, 28, T.white) { align = TextAlignmentOptions.Center };
        var widths = parts.Select(p => TextWidth(p, tabStyle, scratch) + 60).ToList();
        float left = 60 + 96 + 18, right = 1860 - counterW - 18;
        float x = left + (right - left - (widths.Sum() + 14 * (parts.Count - 1))) / 2;
        int part = s.Int("p");
        for (int p = 0; p < parts.Count; p++)
        {
            bool active = p == part;
            var tab = Box(stage, "tab_" + parts[p], x, 40, widths[p], 88, active ? T.coral : T.teal, 22);
            var st = new TextStyle(mplus, 28, active ? T.coralText : T.white) { align = TextAlignmentOptions.Center };
            Text(tab.transform, "label", parts[p], st, 0, 0, widths[p], 88);
            int first = screens.FindIndex(sc => sc.Int("p") == p);
            Tappable(tab.rectTransform, () => Jump(first));
            x += widths[p] + 14;
        }

        if (index > 0)
        {
            var prev = Bordered(stage, "prev", 60, 906, 130, 130, T.teal, T.beigeBorder, 5, 24);
            Text(prev.transform, "label", "<", new TextStyle(mplus, 56, T.white) { align = TextAlignmentOptions.Center }, 0, 0, 130, 130);
            Tappable(prev.rectTransform, () => Go(-1));
        }

        if (index < screens.Count - 1)
        {
            string label = s.Str("next") ?? widgets.Str("nextLabel");
            var st = new TextStyle(lilita, 56, T.white) { align = TextAlignmentOptions.Center };
            float w = TextWidth(label, st, scratch) + 2 * 54 + 2 * 5;
            Shadow(stage, 1860 - w, 906, w, 130, 28, 14, 28, T.Shadow(0.25f));
            var next = Bordered(stage, "next", 1860 - w, 906, w, 130, T.coral, T.nextBorder, 5, 28);
            Text(next.transform, "label", label, st, 0, 0, w, 130);
            Tappable(next.rectTransform, () => Go(1));
        }
    }

    // ---- light title screens -------------------------------------------------

    void BuildLight(Dictionary<string, object> s)
    {
        Box(stage, "background", 0, 0, 1920, 1080, T.offWhite);

        float y = 200;
        var kicker = new TextStyle(mplus, 34, T.teal) { bold = true, letterSpacing = 6 };
        y += H(Text(stage, "kicker", s.Str("kicker"), kicker, 160, y, 920).rectTransform) + 26;
        y += H(Text(stage, "title", s.Str("title"), new TextStyle(lilita, 148, T.navyTop, 0.95f), 160, y, 920).rectTransform) + 26;
        y += H(Text(stage, "sub", s.Str("sub"), new TextStyle(mplus, 40, T.navyTop, 1.35f) { pretty = true }, 160, y, 920).rectTransform);

        if (s.Has("topics"))
        {
            y += 26 + 18;
            var topics = s.Strings("topics");
            var num = new TextStyle(lilita, 34, T.white) { align = TextAlignmentOptions.Center };
            var label = new TextStyle(mplus, 34, T.navyTop) { align = TextAlignmentOptions.Left };
            for (int k = 0; k < topics.Count; k++)
            {
                var sq = Box(stage, "topic" + (k + 1), 160, y, 64, 64, T.teal, 14);
                Text(sq.transform, "n", (k + 1).ToString(), num, 0, 0, 64, 64);
                Text(stage, "topic_label", topics[k], label, 160 + 64 + 24, y, 920 - 88, 64);
                y += 64 + 16;
            }
        }

        if (s.Int("finish") == 1)
        {
            y += 26 + 40;
            var play = new TextStyle(mplus, 40, T.coralText) { align = TextAlignmentOptions.Center };
            float pw = TextWidth(widgets.Str("finishPlay"), play, scratch) + 96;
            Shadow(stage, 160, y, pw, 128, 28, 18, 36, T.Shadow(0.18f));
            var pb = Box(stage, "play_notes_l1", 160, y, pw, 128, T.coral, 28);
            Text(pb.transform, "label", widgets.Str("finishPlay"), play, 0, 0, pw, 128);
            Tappable(pb.rectTransform, PlayNotesLevel1);

            var replay = new TextStyle(mplus, 36, T.white) { align = TextAlignmentOptions.Center };
            float rw = TextWidth(widgets.Str("finishReplay"), replay, scratch) + 80;
            var rb = Box(stage, "replay", 160 + pw + 32, y, rw, 128, T.teal, 28);
            Text(rb.transform, "label", widgets.Str("finishReplay"), replay, 0, 0, rw, 128);
            Tappable(rb.rectTransform, () => Jump(0));
        }

        // characters: x 1080..1860, bottom-aligned at y 930, centred, 40 apart
        var keys = s.Strings("guides");
        var items = keys.Select(k =>
        {
            bool boy = k == "boy";
            float h = boy ? 720 : keys.Count > 1 ? 470 : 600;
            float sw = boy ? 0 : keys.Count > 1 ? 300 : 380;
            var g = FindGuide(k);
            float iw = h * Aspect(g);
            return (g, h, iw, sw, w: Mathf.Max(iw, sw), ih: boy ? h : h + 44 - 30);
        }).ToList();
        float total = items.Sum(i => i.w) + 40 * (items.Count - 1);
        float x = 1080 + (780 - total) / 2;
        foreach (var it in items)
        {
            float top = 930 - it.ih;
            if (it.sw > 0)
            {
                var e = Rect(stage, "floor", x + (it.w - it.sw) / 2, top + it.h - 30, it.sw, 44).gameObject.AddComponent<Image>();
                e.sprite = TutorialShapes.Ellipse(it.sw, 44);
                e.color = T.shadowBlue;
                e.raycastTarget = false;
            }
            if (it.g?.cropped != null) Picture(stage, it.g.key, it.g.cropped, x + (it.w - it.iw) / 2, top, it.iw, it.h, T.white);
            x += it.w + 40;
        }
    }

    // ---- dark teaching screens ----------------------------------------------

    struct VItem
    {
        public RectTransform rt;
        public bool alignStart;   // align-self: flex-start
        public float marginLeft, marginTop;
    }

    void BuildDark(Dictionary<string, object> s)
    {
        var bg = Box(stage, "background", 0, 0, 1920, 1080, T.white);
        bg.sprite = TutorialShapes.VerticalGradient(T.navyTop, T.navyBottom);

        var title = Text(stage, "title", s.Str("title"), new TextStyle(lilita, 72, T.white, 1), 120, 168, 1680);
        Text(stage, "body", s.Str("body"), new TextStyle(mplus, 34, T.white, 1.42f) { pretty = true }, 120, 168 + H(title.rectTransform) + 14, 1560);

        var vis = Rect(stage, "visual", 60, 410, 1800, 500);
        var items = new List<VItem>();
        string w = s.Str("w") ?? "";

        var boardCfgs = (s.List("boards") ?? new List<object>()).Cast<Dictionary<string, object>>().ToList();
        if (w == "picker")
        {
            var ch = (Dictionary<string, object>)data.List("chordsLevel1")[chord];
            boardCfgs = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { ["frets"] = 5.0, ["w"] = 1000.0, ["h"] = 250.0, ["names"] = 1.0, ["marks"] = ch.List("m") },
            };
        }
        if (boardCfgs.Count > 0) items.Add(new VItem { rt = BoardsRow(vis, boardCfgs) });

        var tap = TapConfig(s);
        if (tap != null) items.Add(new VItem { rt = TryPill(vis, tap) });

        switch (w)
        {
            case "chroma": items.Add(new VItem { rt = Chroma(vis, false) }); break;
            case "sharps": items.Add(new VItem { rt = Chroma(vis, true) }); break;
            case "ladder":
                items.Add(new VItem { rt = Ladder(vis) });
                items.Add(new VItem { rt = Legend(vis) });
                break;
            case "picker":
                items.Add(new VItem { rt = ChordButtons(vis) });
                items.Add(new VItem { rt = Formula(vis) });
                break;
            case "ext":
                var heading = new TextStyle(mplus, 28, T.coral) { letterSpacing = 3 };
                items.Add(new VItem { rt = Text(vis, "ext_heading", widgets.Str("extHeading"), heading, 0, 0).rectTransform, alignStart = true, marginLeft = 40 });
                items.Add(new VItem { rt = SeventhsGrid(vis) });
                items.Add(new VItem { rt = ExtLines(vis), alignStart = true, marginLeft = 40, marginTop = 6 });
                break;
            case "modes": items.Add(new VItem { rt = Modes(vis) }); break;
            case "hud": items.Add(new VItem { rt = Hud(vis) }); break;
            case "rules": items.Add(new VItem { rt = Rules(vis) }); break;
            case "avatars": items.Add(new VItem { rt = Avatars(vis) }); break;
            case "tuning": items.Add(new VItem { rt = TuningCard(vis) }); break;
        }

        if (w == "tuning")
        {
            // row-reverse, gap 120: the card on the left, the board on the right
            items.Reverse();
            float total = items.Sum(i => W(i.rt)) + 120 * (items.Count - 1);
            float x = (1800 - total) / 2;
            foreach (var it in items)
            {
                Place(it.rt, x, (500 - H(it.rt)) / 2);
                x += W(it.rt) + 120;
            }
        }
        else
        {
            // column, gap 22, centred both ways (overflow spills evenly, like flexbox)
            float total = items.Sum(i => H(i.rt) + i.marginTop) + 22 * (items.Count - 1);
            float y = (500 - total) / 2;
            foreach (var it in items)
            {
                y += it.marginTop;
                Place(it.rt, it.alignStart ? it.marginLeft : (1800 - W(it.rt)) / 2, y);
                y += H(it.rt) + 22;
            }
        }

        if (s.Has("tip")) TipRow(s);
    }

    void TipRow(Dictionary<string, object> s)
    {
        // x 230..1500, bottom at 1050: guide 150 tall, 18 gap, bubble 14 above the bottom
        var g = FindGuide(widgets.Strings("tipGuideByPart")[s.Int("p")]);
        float iw = 150 * Aspect(g);
        if (g?.cropped != null) Picture(stage, "tip_guide", g.cropped, 230, 900, iw, 150, T.white);

        var st = new TextStyle(mplus, 28, T.white, 1.35f) { pretty = true };
        float maxText = 1270 - iw - 18 - 60;
        float tw = FitWidth(s.Str("tip"), st, maxText, scratch);
        var probe = Text(scratch, "probe", s.Str("tip"), st, 0, 0, tw);
        float bh = H(probe.rectTransform) + 40;
        DestroyImmediate(probe.gameObject);
        var bubble = Box(stage, "tip", 230 + iw + 18, 1036 - bh, tw + 60, bh, T.teal, 24);
        Text(bubble.transform, "text", s.Str("tip"), st, 30, 20, tw);
    }

    // ---- boards ---------------------------------------------------------------

    RectTransform BoardsRow(Transform parent, List<Dictionary<string, object>> cfgs)
    {
        var row = Rect(parent, "boards", 0, 0, 0, 0);
        float x = 0, h = 0;
        for (int k = 0; k < cfgs.Count; k++)
        {
            List<TutorialBoard.Mark> extra = null;
            Action<int, int> onTap = null;
            if (k == 0 && cfgs[k].Has("tap"))
            {
                extra = found.Select(kv =>
                {
                    var sf = kv.Key.Split('-');
                    return new TutorialBoard.Mark(int.Parse(sf[0]), int.Parse(sf[1]), kv.Value, 'n');
                }).ToList();
                if (wrong.HasValue) extra.Add(wrong.Value);
                onTap = Tap;
            }
            var b = TutorialBoard.Build(row, cfgs[k], extra, onTap);
            Place(b, x, 0);
            x += W(b) + 80;
            h = Mathf.Max(h, H(b));
        }
        Size(row, x - 80, h);
        return row;
    }

    RectTransform TryPill(Transform parent, Dictionary<string, object> t)
    {
        bool done = TryDone();
        string text = done ? t.Str("ok")
            : wrong.HasValue ? string.Format(widgets.Str("wrongTap"), wrong.Value.label)
            : t.Str("ask");
        var st = new TextStyle(mplus, 32, done ? T.coralText : T.white);
        float tw = TextWidth(text, st, scratch);
        float h = LineBox(mplus, 32) + 32;
        var pill = Box(parent, "try_status", 0, 0, tw + 72, h, done ? T.coral : T.teal, 20);
        Text(pill.transform, "text", text, st, 36, 16);
        return pill.rectTransform;
    }

    // ---- part B widgets ------------------------------------------------------------

    RectTransform Chroma(Transform parent, bool brackets)
    {
        var row = Rect(parent, "chroma", 0, 0, 13 * 104 + 12 * 20, 0);
        var notes = data.Strings("notes");
        notes.Add(notes[0]);
        var sub = new TextStyle(mplus, 24, T.muted);
        float colH = 0;
        for (int k = 0; k < 13; k++)
        {
            float x = k * 124;
            bool sharp = notes[k].Contains("#"), again = k == 12;
            var st = new TextStyle(lilita, 48, sharp || again ? T.white : T.navyTop) { align = TextAlignmentOptions.Center };
            RectTransform chip;
            if (again) chip = Dashed(row, "chip_again", x, 0, 104, 120, T.muted, 20, 3).rectTransform;
            else chip = Box(row, "chip_" + notes[k], x, 0, 104, 120, sharp ? T.teal : T.offWhite, 20).rectTransform;
            Text(chip, "n", notes[k], st, 0, 0, 104, 120);
            var l = Text(row, "index", again ? widgets.Str("chromaAgain") : k.ToString(), sub, 0, 120 + 14);
            Place(l.rectTransform, x + (104 - W(l.rectTransform)) / 2, 120 + 14);
            colH = 120 + 14 + H(l.rectTransform);
        }
        if (brackets)
            foreach (List<object> br in widgets.List("sharpsBrackets"))
            {
                float bx = (int)(double)br[0] * 124;
                // 4px coral sides and bottom, bottom corners r12: a ring with its top clipped off
                var clip = Rect(row, "bracket", bx, 184, 228, 22);
                clip.gameObject.AddComponent<RectMask2D>();
                Ring(clip, "line", 0, -20, 228, 42, T.coral, 12, 4);
                var t = Text(row, "bracket_label", (string)br[1], new TextStyle(mplus, 26, T.coral), 0, 184 + 22 + 10);
                Place(t.rectTransform, bx + (228 - W(t.rectTransform)) / 2, 184 + 22 + 10);
            }
        Size(row, W(row), colH + 120);   // padding-bottom 120 leaves room for the brackets
        return row;
    }

    // ---- part C widgets ------------------------------------------------------------

    RectTransform Ladder(Transform parent)
    {
        var row = Rect(parent, "ladder", 0, 0, 13 * 124 + 12 * 6, 0);
        Box(row, "line", 40, 96, W(row) - 80, 5, T.white);
        var stSt = new TextStyle(mplus, 26, T.muted);
        var nameSt = new TextStyle(mplus, 24, T.white, 1.25f) { align = TextAlignmentOptions.Top };
        float h = 0;
        var ladder = data.List("intervalLadder");
        for (int k = 0; k < ladder.Count; k++)
        {
            var e = (List<object>)ladder[k];
            string label = (string)e[0], name = (string)e[1];
            float x = k * 130;
            var n = Text(row, "semitones", k.ToString(), stSt, 0, 0, 0, 36);
            Place(n.rectTransform, x + (124 - W(n.rectTransform)) / 2, 0);
            bool root = label == "R";
            var m = Bordered(row, root ? "root" : "note", x + 18, 48, 88, 88, root ? T.coral : T.green,
                             root ? T.rootRing : T.white, root ? 7 : 4, 44);
            Text(m.transform, "label", label, new TextStyle(mplus, root ? 40 : 38, T.white) { medium = true, align = TextAlignmentOptions.Center }, 0, 0, 88, 88);
            var nm = Text(row, "name", name, nameSt, x, 48 + 88 + 12, 124);
            h = Mathf.Max(h, 48 + 88 + 12 + H(nm.rectTransform));
        }
        Size(row, W(row), h);
        return row;
    }

    RectTransform Legend(Transform parent)
    {
        var row = Rect(parent, "legend", 0, 0, 0, 0);
        var st = new TextStyle(mplus, 28, T.muted);
        float x = 0, h = 0;
        foreach (var l in widgets.Strings("ladderLegend"))
        {
            var t = Text(row, "legend", l, st, x, 0);
            x += W(t.rectTransform) + 48;
            h = H(t.rectTransform);
        }
        Size(row, x - 48, h);
        return row;
    }

    // ---- part D widgets ------------------------------------------------------------

    RectTransform ChordButtons(Transform parent)
    {
        var chords = data.List("chordsLevel1");
        var row = Rect(parent, "chord_types", 0, 0, chords.Count * 236 + (chords.Count - 1) * 24, 96);
        var st = new TextStyle(mplus, 32, T.white) { align = TextAlignmentOptions.Center };
        for (int k = 0; k < chords.Count; k++)
        {
            string name = ((Dictionary<string, object>)chords[k]).Str("name");
            var b = Box(row, "chord_" + name, k * 260, 0, 236, 96, k == chord ? T.chordSelected : T.ink, 26);
            Text(b.transform, "label", name, st, 0, 0, 236, 96);
            int kk = k;
            Tappable(b.rectTransform, () => { chord = kk; Render(); });
        }
        return row;
    }

    RectTransform Formula(Transform parent)
    {
        var c = (Dictionary<string, object>)data.List("chordsLevel1")[chord];
        var row = Rect(parent, "formula", 0, 0, 0, 0);
        var f = c.Strings("f");

        var nameSt = new TextStyle(lilita, 44, T.white) { align = TextAlignmentOptions.TopRight };
        float nameW = Mathf.Max(260, TextWidth(c.Str("name"), nameSt, scratch));
        float nameH = LineBox(lilita, 44);
        var plusSt = new TextStyle(mplus, 36, T.muted);
        float plusW = TextWidth("+", plusSt, scratch), plusH = LineBox(mplus, 36);
        // each chip and its "+" are one flex unit (gap 14); the units are 14 apart too
        float chipsW = f.Count * (72 + 14) + (f.Count - 1) * (plusW + 14);
        var feelSt = new TextStyle(mplus, 30, T.white);
        float feelW = FitWidth(c.Str("feel"), feelSt, 640, scratch);
        var feel = Text(row, "feel", c.Str("feel"), feelSt, 0, 0, feelW);
        float h = Mathf.Max(72, nameH, H(feel.rectTransform));

        // align-items: center
        Text(row, "name", c.Str("name"), nameSt, 0, (h - nameH) / 2, nameW, nameH);
        float x = nameW + 28;
        for (int k = 0; k < f.Count; k++)
        {
            TutorialBoard.Chip(row, x, (h - 72) / 2, 72, f[k], 32, 6, 3);
            x += 72 + 14;
            if (k < f.Count - 1)
            {
                Text(row, "plus", "+", plusSt, x, (h - plusH) / 2);
                x += plusW + 14;
            }
        }
        x = nameW + 28 + chipsW + 28;
        Place(feel.rectTransform, x, (h - H(feel.rectTransform)) / 2);
        Size(row, x + feelW, h);
        return row;
    }

    RectTransform SeventhsGrid(Transform parent)
    {
        var ext = data.List("level2Sevenths");
        var grid = Rect(parent, "sevenths", 0, 0, 3 * 520 + 2 * 20, 2 * 112 + 20);
        var nameSt = new TextStyle(lilita, 36, T.white) { align = TextAlignmentOptions.Left };
        for (int k = 0; k < ext.Count; k++)
        {
            var e = (List<object>)ext[k];
            float x = (k % 3) * 540, y = (k / 3) * 132;
            var card = Box(grid, "card_" + e[0], x, y, 520, 112, T.teal, 24);
            Text(card.transform, "name", (string)e[0], nameSt, 28, 0, 150, 112);
            var f = ((List<object>)e[1]).Cast<string>().ToList();
            for (int c = 0; c < f.Count; c++)
                TutorialBoard.Chip(card.transform, 28 + 150 + 22 + c * 70, 26, 60, f[c], 26, 6, 3);
        }
        return grid;
    }

    RectTransform ExtLines(Transform parent)
    {
        var col = Rect(parent, "ext_lines", 0, 0, 0, 0);
        var st = new TextStyle(mplus, 28, T.white);
        string coral = "#" + ColorUtility.ToHtmlStringRGB(T.coral);
        float y = 0, w = 0;
        foreach (List<object> l in widgets.List("extLines"))
        {
            var t = Text(col, "line", $"<color={coral}>{l[0]}</color>{l[1]}", st, 0, y);
            w = Mathf.Max(w, W(t.rectTransform));
            y += H(t.rectTransform) + 12;
        }
        Size(col, w, y - 12);
        return col;
    }

    // ---- part E widgets ------------------------------------------------------------

    RectTransform Modes(Transform parent)
    {
        var modes = widgets.List("modes");
        var row = Rect(parent, "modes", 0, 0, modes.Count * 520 + (modes.Count - 1) * 44, 440);
        var header = new TextStyle(mplus, 44, T.coralText) { align = TextAlignmentOptions.Center };
        var what = new TextStyle(mplus, 32, T.white, 1.3f);
        var q = new TextStyle(mplus, 26, T.white, 1.3f);
        var meta = new TextStyle(mplus, 26, T.muted);
        for (int k = 0; k < modes.Count; k++)
        {
            var m = (Dictionary<string, object>)modes[k];
            var card = Box(row, "mode_" + m.Str("name"), k * 564, 0, 520, 440, T.teal, 30).transform;
            var whatT = Text(card, "what", m.Str("what"), what, 34, 0, 452);
            var qT = m.Strings("qs").Select(line => Text(card, "q", "→ " + line, q, 34, 0, 452)).ToList();
            float mh = LineBox(mplus, 26);
            // The card is a 440-tall flex column and its content overflows, so the
            // header (the only item that can shrink) gives up the difference,
            // down to its text's height.
            float content = 34 + 92 + 22 + H(whatT.rectTransform) + 22 + qT.Sum(t => H(t.rectTransform)) + 10 * (qT.Count - 1) + 22 + mh + 34;
            float hh = Mathf.Clamp(92 - (content - 440), LineBox(mplus, 44), 92);
            Shadow(card, 34, 34, 452, hh, 24, 12, 24, T.Shadow(0.25f));
            var hb = Box(card, "header", 34, 34, 452, hh, T.coral, 24);
            Text(hb.transform, "label", m.Str("name"), header, 0, 0, 452, hh);
            hb.transform.SetSiblingIndex(0);
            card.Find("shadow").SetSiblingIndex(0);
            float y = 34 + hh + 22;
            Place(whatT.rectTransform, 34, y);
            y += H(whatT.rectTransform) + 22;
            foreach (var t in qT)
            {
                Place(t.rectTransform, 34, y);
                y += H(t.rectTransform) + 10;
            }
            // margin-top: auto pins the meta line to the bottom when there is room
            Text(card, "meta", m.Str("meta"), meta, 34, Mathf.Max(y - 10 + 22, 440 - 34 - mh), 452);
        }
        return row;
    }

    RectTransform Hud(Transform parent)
    {
        var hud = widgets.Dict("hud");
        var col = Rect(parent, "hud", 0, 0, 1680, 0);

        // the question and its callout, centred
        var q = new TextStyle(lilita, 96, T.white, 1);
        var call = new TextStyle(mplus, 28, T.coral);
        float qw = TextWidth(hud.Str("question"), q, scratch), cw = TextWidth(hud.Str("questionCallout"), call, scratch);
        float cx = (1680 - (qw + 30 + cw)) / 2;
        Text(col, "question", hud.Str("question"), q, cx, 0);
        Text(col, "callout", hud.Str("questionCallout"), call, cx + qw + 30, (96 - LineBox(mplus, 28)) / 2);
        float y = 96 + 10 + 16;

        // three callouts over the bar, each with a 4x30 coral pointer
        var lab = new TextStyle(mplus, 28, T.white) { align = TextAlignmentOptions.Top };
        var callouts = hud.Strings("callouts");
        float colH = 0;
        for (int k = 0; k < 3; k++)
        {
            var t = Text(col, "callout" + k, callouts[k], lab, k * 560, y, 560);
            Box(col, "pointer", k * 560 + 278, y + H(t.rectTransform) + 8, 4, 30, T.coral);
            colH = H(t.rectTransform) + 8 + 30;
        }
        y += colH + 10;

        // the bar: hudBar, a tealHud trapezoid at 500..1180, clock + time, score, health
        var bar = Box(col, "bar", 0, y, 1680, 128, T.hudBar, 20).transform;
        var trapGo = Rect(bar, "centre", 500, 0, 680, 128).gameObject;
        trapGo.AddComponent<CanvasRenderer>();
        var trap = trapGo.AddComponent<TutorialPolygon>();
        trap.color = T.tealHud;
        trap.raycastTarget = false;
        trap.points = new[] { new Vector2(0, 0), new Vector2(680, 0), new Vector2(612, 128), new Vector2(68, 128) };

        var clock = Box(bar, "clock", 220, 33, 62, 62, T.offWhite, 31).transform;
        Box(clock, "hand_up", 28, 12, 7, 22, T.hudBar, 3.5f);
        var hand = Box(clock, "hand_down", 30, 28, 7, 18, T.hudBar, 3.5f).rectTransform;
        // CSS rotate(-45deg) around the top centre
        hand.pivot = new Vector2(0.5f, 1);
        hand.anchoredPosition += new Vector2(3.5f, 0);
        hand.localEulerAngles = new Vector3(0, 0, 45);

        var big = new TextStyle(mplus, 64, T.white);
        var t52 = new TextStyle(mplus, 52, T.white);
        Text(bar, "time", hud.Str("time"), t52, 220 + 62 + 28, (128 - LineBox(mplus, 52)) / 2);
        var score = Text(bar, "score", hud.Str("score"), big, 0, (128 - LineBox(mplus, 64)) / 2);
        Place(score.rectTransform, (1680 - W(score.rectTransform)) / 2, (128 - LineBox(mplus, 64)) / 2);
        var health = Text(bar, "health", hud.Str("health"), big, 0, 0);
        Place(health.rectTransform, 1680 - 190 - W(health.rectTransform), (128 - LineBox(mplus, 64)) / 2);

        Size(col, 1680, y + 128);
        return col;
    }

    RectTransform Rules(Transform parent)
    {
        var rules = widgets.Dict("rules");
        var row = Rect(parent, "rules", 0, 0, 0, 0);

        // two cards on the left
        var left = Rect(row, "cards", 0, 0, 560, 0);
        var titleSt = new TextStyle(lilita, 40, T.white);
        var rowSt = new TextStyle(mplus, 30, T.white);
        float y = 0;
        foreach (Dictionary<string, object> c in rules.List("cards"))
        {
            var valueSt = new TextStyle(mplus, 30, c.Str("color") == "green" ? T.green : T.coral) { align = TextAlignmentOptions.TopRight };
            var rows = c.List("rows");
            float rh = LineBox(mplus, 30);
            float h = 28 + LineBox(lilita, 40) + rows.Count * (12 + rh) + 28;
            var card = Box(left, "card", 0, y, 560, h, T.teal, 28).transform;
            float cy = 28;
            cy += H(Text(card, "title", c.Str("title"), titleSt, 34, cy).rectTransform);
            foreach (List<object> r in rows)
            {
                cy += 12;
                Text(card, "label", (string)r[0], rowSt, 34, cy);
                Text(card, "value", (string)r[1], valueSt, 34, cy, 492, rh);
                cy += rh;
            }
            y += h + 22;
        }
        Size(left, 560, y - 22);

        // the VT323 ticket, rotated -2deg
        float headH = LineBox(vt323, 56) + 28;
        float cellH = LineBox(vt323, 44);
        var table = new List<List<string>> { rules.Strings("ticketHeader") };
        table.AddRange(rules.List("ticketRows").Cast<List<object>>().Select(r => r.Cast<string>().ToList()));
        float bodyH = 22 + table.Count * cellH + (table.Count - 1) * 14 + 30;
        float th = headH + bodyH;
        var ticket = Rect(row, "ticket", 560 + 40, 0, 900, th);
        Shadow(ticket, 0, 0, 900, th, 18, 24, 48, T.Shadow(0.35f));
        Box(ticket, "paper", 0, 0, 900, th, T.paper, 18);
        Box(ticket, "header", 0, 0, 900, headH, T.coral, 18);
        Box(ticket, "header_square", 0, headH - 18, 900, 18, T.coral);   // square bottom corners
        Text(ticket, "title", rules.Str("ticketTitle"), new TextStyle(vt323, 56, T.white), 40, 14);
        float colW = (900 - 80 - 300) / 4f;
        for (int r = 0; r < table.Count; r++)
            for (int c = 0; c < table[r].Count; c++)
            {
                if (string.IsNullOrEmpty(table[r][c])) continue;
                var st = new TextStyle(vt323, 44, T.ticketInk) { underline = r == 0 };
                Text(ticket, "cell", table[r][c], st, 40 + (c == 0 ? 0 : 300 + (c - 1) * colW), headH + 22 + r * (cellH + 14));
            }

        float h2 = Mathf.Max(H(left), th);
        Place(left, 0, (h2 - H(left)) / 2);
        Place(ticket, 600, (h2 - th) / 2);
        // rotate about the centre, like CSS transform-origin: 50% 50%
        ticket.pivot = new Vector2(0.5f, 0.5f);
        ticket.anchoredPosition += new Vector2(450, -th / 2);
        ticket.localEulerAngles = new Vector3(0, 0, 2);
        Size(row, 600 + 900, h2);
        return row;
    }

    RectTransform Avatars(Transform parent)
    {
        var av = widgets.Dict("avatars");
        var card = Box(parent, "avatars", 0, 0, 1600, 470, T.teal, 30).rectTransform;
        var slots = av.List("slots");
        float free = 1500 - slots.Count * 260, around = free / slots.Count;
        var howSt = new TextStyle(mplus, 24, T.muted) { align = TextAlignmentOptions.Top };
        for (int k = 0; k < slots.Count; k++)
        {
            var sl = (Dictionary<string, object>)slots[k];
            bool unlocked = sl.Int("unlocked") == 1;
            float x = 50 + around / 2 + k * (260 + around);
            float nameH = LineBox(mplus, 28), howH = LineBox(mplus, 24);
            float colH = 230 + 16 + nameH + 16 + howH;
            float y = 30 + 340 - colH;   // align-items: flex-end
            var g = FindGuide(sl.Str("guide"));
            float iw = 230 * Aspect(g);
            if (g?.cropped != null)
            {
                var img = Picture(card, sl.Str("guide"), g.cropped, x + (260 - iw) / 2, y, iw, 230, unlocked ? T.white : Color.black);
                if (!unlocked)   // the gallery's lock: a white "?" over a black silhouette
                    Text(img.transform, "lock", "?", new TextStyle(mplus, 140, T.lockMark) { align = TextAlignmentOptions.Center }, 0, 0, iw, 230);
            }
            Text(card, "name", sl.Str("name"), new TextStyle(mplus, 28, unlocked ? T.coral : T.muted) { align = TextAlignmentOptions.Top }, x - 100, y + 246, 460, nameH);
            Text(card, "how", sl.Str("how"), howSt, x, y + 246 + nameH + 16, 260, howH);
        }
        float fh = LineBox(mplus, 30);
        Text(card, "footer", av.Str("footer"), new TextStyle(mplus, 30, T.white) { align = TextAlignmentOptions.Top }, 50, 470 - 30 - fh, 1500, fh);
        return card;
    }

    RectTransform TuningCard(Transform parent)
    {
        var rows = TutorialBoard.OpenStrings.Take(5).Concat(new[] { "D" }).ToList();
        var boards = Screen.List("boards");
        if (boards != null && boards.Count > 0 && ((Dictionary<string, object>)boards[0]).Has("tuning"))
            rows = ((Dictionary<string, object>)boards[0]).Strings("tuning");

        float titleH = LineBox(lilita, 52);
        float h = 24 + titleH + 6 + rows.Count * (8 + 50) + 30;
        var holder = Rect(parent, "tuning", 0, 0, 440, h);
        Shadow(holder, 0, 0, 440, h, 8, 20, 40, T.Shadow(0.35f));
        var card = Box(holder, "card", 0, 0, 440, h, T.teal, 8).transform;
        var title = new TextStyle(lilita, 52, T.white) { align = TextAlignmentOptions.Top };
        Text(card, "title", widgets.Str("tuningTitle"), title, 0, 24, 440, titleH);
        float y = 24 + titleH + 6;
        for (int k = 0; k < rows.Count; k++)
        {
            y += 8;
            var minus = Box(card, "minus", 40, y, 50, 50, T.coral);
            Text(minus.transform, "t", "−", new TextStyle(lilita, 34, T.white) { align = TextAlignmentOptions.Center }, 0, 0, 50, 50);
            var plus = Box(card, "plus", 440 - 40 - 50, y, 50, 50, T.coral);
            Text(plus.transform, "t", "+", new TextStyle(lilita, 36, T.white) { align = TextAlignmentOptions.Center }, 0, 0, 50, 50);
            var letter = new TextStyle(lilita, 40, k == rows.Count - 1 ? T.coral : T.white) { align = TextAlignmentOptions.Center };
            Text(card, "string", rows[k], letter, 90, y, 260, 50);
            y += 50;
        }
        return holder;
    }
}
