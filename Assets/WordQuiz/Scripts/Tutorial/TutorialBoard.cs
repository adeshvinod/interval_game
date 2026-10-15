using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static TutorialUI;
using T = TutorialTheme;

/// <summary>
/// The tutorial's fretboard diagram (README "Fretboard diagram"), built from one
/// board config in tutorial_screens.json:
///
///   cw  = w / N                      column width (column 0 = open strings)
///   gap = h / 5                      string spacing
///   d   = min(cw*.8, gap*.98, 96)    marker diameter
///   string s at y = s*gap, thickness [2, 2.5, 3.5, 4.5, 6, 7]
///   nut at x = cw (6 px), frets at (k+1)*cw (2 px), white @ 55%
///   marker (s, f) centred on ((f + .5)*cw, s*gap)
///
/// Open-string notes sit left of the nut, like the game's Notes learn mode.
/// </summary>
public static class TutorialBoard
{
    public struct Mark
    {
        public int s, f;
        public string label;
        public char kind;   // n note | r root | g ghost | w wrong tap
        public Mark(int s, int f, string label, char kind) { this.s = s; this.f = f; this.label = label; this.kind = kind; }
    }

    static readonly float[] StringThickness = { 2, 2.5f, 3.5f, 4.5f, 6, 7 };

    public static string[] Notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
    public static string[] Intervals = { "R", "b2", "M2", "b3", "M3", "P4", "b5", "P5", "b6", "M6", "b7", "M7" };
    public static string[] OpenStrings = { "E", "B", "G", "D", "A", "E" };

    public static string NoteAt(int s, int f, IList<string> tuning = null)
    {
        tuning = tuning ?? OpenStrings;
        return Notes[(Array.IndexOf(Notes, tuning[s]) + f) % 12];
    }

    public static string IntervalOf(int rootS, int rootF, int s, int f)
    {
        int a = Array.IndexOf(Notes, NoteAt(s, f)), r = Array.IndexOf(Notes, NoteAt(rootS, rootF));
        return Intervals[((a - r) % 12 + 12) % 12];
    }

    public static List<Mark> ParseMarks(List<object> raw, IList<string> tuning)
    {
        var marks = new List<Mark>();
        if (raw == null) return marks;
        foreach (List<object> m in raw)
        {
            int s = (int)(double)m[0], f = (int)(double)m[1];
            string label = (string)m[2];
            if (label == "@") label = NoteAt(s, f, tuning);
            char kind = m.Count > 3 && m[3] is string k && k.Length > 0 ? k[0] : 'n';
            marks.Add(new Mark(s, f, label, kind));
        }
        return marks;
    }

    /// Builds the board column (board, fret numbers, caption) into a new group.
    /// Returns the group, sized to its CSS margin box.
    public static RectTransform Build(Transform parent, Dictionary<string, object> cfg, List<Mark> extra,
                                      Action<int, int> onTap)
    {
        int N = cfg.Int("frets");
        float BW = cfg.Num("w"), BH = cfg.Num("h");
        float cw = BW / N, gap = BH / 5;
        float d = Mathf.Min(cw * 0.8f, gap * 0.98f, 96);
        var tuning = cfg.Has("tuning") ? cfg.Strings("tuning") : new List<string>(OpenStrings);

        float pad = Mathf.Round(d / 2);
        float padL = cfg.Has("names") ? 90 : 0;
        float padR = cfg.Has("side") ? 420 : 0;

        var group = Rect(parent, "board", 0, 0, padL + BW + padR, 0);
        var b = Rect(group, "strings", padL, pad, BW, BH);

        // glow band behind one string: coral @ 38% with a coral @ 55% halo outside it
        if (cfg.Has("glow"))
        {
            float gy = cfg.Int("glow") * gap;
            Shadow(b, -12, gy - 15, BW + 24, 30, 15, 0, 28, T.glowHalo, cutout: true);
            Box(b, "glow", -12, gy - 15, BW + 24, 30, T.glowBand, 15);
        }

        for (int k = 0; k < N; k++)
        {
            float t = k == 0 ? 6 : 2;
            Box(b, k == 0 ? "nut" : "fret" + k, (k + 1) * cw - t / 2, 0, t, BH, T.fretLine);
        }
        for (int s = 0; s < 6; s++)
        {
            float t = StringThickness[s];
            Box(b, "string" + s, 0, s * gap - t / 2, BW, t, T.white);
        }

        if (cfg.Has("names"))
        {
            var st = new TextStyle(lilita, 32, T.muted) { align = TextAlignmentOptions.TopRight };
            float lb = LineBox(lilita, 32);
            for (int s = 0; s < 6; s++) Text(b, "name" + s, tuning[s], st, -84, s * gap - lb / 2, 56, lb);
        }

        if (cfg.Has("side"))
        {
            var st = new TextStyle(mplus, 28, T.coral);
            float lb = LineBox(mplus, 28);
            foreach (List<object> sd in cfg.List("side"))
                Text(b, "side", "← " + (string)sd[1], st, BW + 30, (int)(double)sd[0] * gap - lb / 2);
        }

        var marks = ParseMarks(cfg.List("marks"), tuning);
        if (extra != null) marks.AddRange(extra);
        foreach (char kind in "gnrw")   // ghosts under notes under roots under the wrong tap
            foreach (var m in marks)
                if (m.kind == kind)
                    Marker(b, (m.f + 0.5f) * cw, m.s * gap, d, m.label, kind);

        if (onTap != null)
            for (int s = 0; s < 6; s++)
                for (int f = 0; f < N; f++)
                {
                    int ss = s, ff = f;
                    Tappable(Rect(b, $"cell{s}_{f}", f * cw, s * gap - gap / 2, cw, gap), () => onTap(ss, ff), clickSound: false);
                }

        float y = pad * 2 + BH;
        if (cfg.Has("nums"))
        {
            y += 14 - 8;
            var st = new TextStyle(mplus, 28, T.muted);
            for (int f = 0; f < N; f++)
            {
                var t = Text(group, "num" + f, f == 0 ? "open" : f.ToString(), st, 0, y);
                Place(t.rectTransform, padL + (f + 0.5f) * cw - W(t.rectTransform) / 2, y);
            }
            y += 34;
        }
        if (cfg.Has("cap"))
        {
            y += 14;
            var t = Text(group, "cap", cfg.Str("cap"), new TextStyle(mplus, 30, T.white), 0, y);
            Place(t.rectTransform, padL + (BW - W(t.rectTransform)) / 2, y);
            y += H(t.rectTransform);
        }
        Size(group, padL + BW + padR, y);
        return group;
    }

    /// A round marker of diameter d centred on (cx, cy): fill, ring, label.
    public static RectTransform Marker(Transform parent, float cx, float cy, float d, string label, char kind,
                                       float fontSize = 0)
    {
        if (fontSize <= 0) fontSize = Mathf.Round(d * (label.Length > 2 ? 0.36f : 0.5f));
        float x = cx - d / 2, y = cy - d / 2, r = d / 2;
        Color text = T.white;
        RectTransform box;
        switch (kind)
        {
            case 'r': box = Bordered(parent, "root", x, y, d, d, T.coral, T.rootRing, 8, r).rectTransform; break;
            case 'g':
                box = Box(parent, "ghost", x, y, d, d, T.navyTop, r).rectTransform;
                Dashed(box, "dash", 0, 0, d, d, T.ghostRing, r, 3);
                break;
            case 'w':
                box = Bordered(parent, "wrong", x, y, d, d, T.ink, T.coral, 4, r).rectTransform;
                text = T.coral;
                break;
            default: box = Bordered(parent, "note", x, y, d, d, T.green, T.white, 4, r).rectTransform; break;
        }
        Label(box, label, fontSize, text);
        return box;
    }

    /// A formula chip (picker and 7ths grid): R is coral with a rootRing ring.
    public static RectTransform Chip(Transform parent, float x, float y, float d, string label, float fontSize,
                                     float rootRingWidth, float noteRingWidth)
    {
        bool root = label == "R";
        var box = Bordered(parent, "chip", x, y, d, d, root ? T.coral : T.green, root ? T.rootRing : T.white,
                           root ? rootRingWidth : noteRingWidth, d / 2).rectTransform;
        Label(box, label, fontSize, T.white);
        return box;
    }

    static void Label(RectTransform box, string label, float size, Color c)
    {
        var st = new TextStyle(mplus, size, c) { medium = true, align = TextAlignmentOptions.Center };
        Text(box, "label", label, st, 0, 0, box.sizeDelta.x, box.sizeDelta.y);
    }
}
