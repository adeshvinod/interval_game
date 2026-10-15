using UnityEngine;

/// <summary>
/// The design tokens from design_handoff_fretnoodle_tutorial/README.md.
/// Every tutorial colour comes from here, parsed from hex, never typed as floats.
/// </summary>
public static class TutorialTheme
{
    public static readonly Color offWhite = Hex("#F1F1F1");
    public static readonly Color navyTop = Hex("#16303C");
    public static readonly Color navyBottom = Hex("#0E1F28");
    public static readonly Color ink = Hex("#0A141A");
    public static readonly Color coral = Hex("#F2784F");
    public static readonly Color coralText = Hex("#2A1810");
    public static readonly Color teal = Hex("#1B4D55");
    public static readonly Color tealHud = Hex("#134E57");
    public static readonly Color hudBar = Hex("#0A1419");
    public static readonly Color green = Hex("#45C24A");
    public static readonly Color chordSelected = Hex("#0E4A2B");
    public static readonly Color rootRing = Hex("#E6DAD0");
    public static readonly Color beigeBorder = Hex("#D9CBB8");
    public static readonly Color nextBorder = Hex("#F1E6DC");
    public static readonly Color muted = Hex("#A9BCC2");
    public static readonly Color fretLine = Hex("#FFFFFF8C");     // white @ 55%
    public static readonly Color white = Hex("#FFFFFF");
    public static readonly Color ghostRing = Hex("#FFFFFFB3");    // white @ 70%
    public static readonly Color paper = Hex("#F7F6EC");
    public static readonly Color ticketInk = Hex("#141414");      // ticket text (prototype)
    public static readonly Color shadowBlue = Hex("#0F7FA3");
    public static readonly Color glowBand = Hex("#F2784F61");     // coral @ 38%
    public static readonly Color glowHalo = Hex("#F2784F8C");     // coral @ 55%
    public static readonly Color lockMark = Hex("#FFFFFFD9");     // the gallery's "?" (white @ 85%)

    public static Color Shadow(float alpha) => new Color(0, 0, 0, alpha);

    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
}
