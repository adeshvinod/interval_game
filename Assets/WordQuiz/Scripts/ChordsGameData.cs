using UnityEngine;
using System.Collections.Generic;

public enum ChordLevel
{
    level1,
    level2,
    level3,
    level4,
    CUSTOM
}

[CreateAssetMenu(fileName = "ChordsGameData", menuName = "Game/Chords Game Data")]
public class ChordsGameData : ScriptableObject
{
    [Header("Level Settings")]
    public ChordLevel currentChordLevel = ChordLevel.level1;
    public List<int> activeChordTypes = new List<int>();

    // ── All chord/scale definitions (interval semitones from root) ──────────
    public static readonly int[] MAJOR       = { 0, 4, 7 };
    public static readonly int[] MINOR       = { 0, 3, 7 };
    public static readonly int[] DIM         = { 0, 3, 6 };
    public static readonly int[] SUS2        = { 0, 2, 7 };
    public static readonly int[] SUS4        = { 0, 5, 7 };
    public static readonly int[] AUG         = { 0, 4, 8 };

    public static readonly int[] MIN7        = { 0, 3, 7, 10 };
    public static readonly int[] MAJ7        = { 0, 4, 7, 11 };
    public static readonly int[] DOM7        = { 0, 4, 7, 10 };
    public static readonly int[] MIN7B5      = { 0, 3, 6, 10 };
    public static readonly int[] DIM7        = { 0, 3, 6, 9 };
    public static readonly int[] MINMAJ7     = { 0, 3, 7, 11 };

    public static readonly int[] MIN9        = { 0, 3, 7, 10, 2 };
    public static readonly int[] MAJ9        = { 0, 4, 7, 11, 2 };
    public static readonly int[] DOM9        = { 0, 4, 7, 10, 2 };
    public static readonly int[] MAJ6        = { 0, 4, 7, 9 };
    public static readonly int[] MIN6        = { 0, 3, 7, 9 };
    public static readonly int[] MINB6       = { 0, 3, 7, 8 };

    public static readonly int[] MAJORSCALE  = { 0, 2, 4, 5, 7, 9, 11 };
    public static readonly int[] MELODICMIN  = { 0, 2, 3, 5, 7, 9, 11 };
    public static readonly int[] HARMONICMIN = { 0, 2, 3, 5, 7, 8, 11 };
    public static readonly int[] HALFWHOLE   = { 0, 1, 3, 4, 6, 7, 9, 10 };
    public static readonly int[] WHOLEHALF   = { 0, 2, 3, 5, 6, 8, 9, 11 };
    public static readonly int[] WHOLETONE   = { 0, 2, 4, 6, 8, 10 };

    // Master list — index = chord type index used everywhere else
    public static readonly int[][] AllChords = {
        MAJOR, MINOR, DIM, SUS2, SUS4, AUG,           // 0-5  (Level 1)
        MIN7, MAJ7, DOM7, MIN7B5, DIM7, MINMAJ7,       // 6-11 (Level 2)
        MIN9, MAJ9, DOM9, MAJ6, MIN6, MINB6,           // 12-17(Level 3)
        MAJORSCALE, MELODICMIN, HARMONICMIN,            // 18-20(Level 4)
        HALFWHOLE, WHOLEHALF, WHOLETONE                 // 21-23(Level 4)
    };

    public static readonly string[] ChordNames = {
        "Major","Minor","Diminished","Sus2","Sus4","Augmented",
        "min7","maj7","dom7","min7b5","dim7","minMaj7",
        "min9","maj9","dom9","maj6","min6","minb6",
        "Major Scale","Melodic Minor","Harmonic Minor",
        "Half-Whole","Whole-Half","Whole Tone"
    };

    // Level presets
    public static readonly int[] level1Chords = { 0,  1,  2,  3,  4,  5 };
    public static readonly int[] level2Chords = { 6,  7,  8,  9, 10, 11 };
    public static readonly int[] level3Chords = {12, 13, 14, 15, 16, 17 };
    public static readonly int[] level4Chords = {18, 19, 20, 21, 22, 23 };

    [Header("Score Data")]
    public int   score          = 0;
    public int   highScore      = 0;
    public float health         = 100f;
    public float timer          = 20f;
    public int   correctAnswers = 0;
    public int   totalQuestions = 72;

    // Runtime-only: chords where user got at least one wrong tap or ran out of time.
    // Populated by chordsGameManager at game-over; consumed by gameover_chords for feedback review.
    [System.NonSerialized] public List<int> wrongChordIndices = new List<int>();
    [System.NonSerialized] public List<int> wrongRootNotes    = new List<int>();

    [Header("Level High Scores")]
    public int level1HighScore = 0;
    public int level2HighScore = 0;
    public int level3HighScore = 0;
    public int level4HighScore = 0;

    public delegate void ChordLevelChangedHandler(ChordLevel newLevel);
    public event ChordLevelChangedHandler OnChordLevelChanged;

    // ── Public API ────────────────────────────────────────────────────────────
    public void SelectChordLevel(int level)
    {
        activeChordTypes.Clear();
        switch (level)
        {
            case 1: currentChordLevel = ChordLevel.level1; activeChordTypes.AddRange(level1Chords); break;
            case 2: currentChordLevel = ChordLevel.level2; activeChordTypes.AddRange(level2Chords); break;
            case 3: currentChordLevel = ChordLevel.level3; activeChordTypes.AddRange(level3Chords); break;
            case 4: currentChordLevel = ChordLevel.level4; activeChordTypes.AddRange(level4Chords); break;
            case 5: currentChordLevel = ChordLevel.CUSTOM; break;
        }
        OnChordLevelChanged?.Invoke(currentChordLevel);
    }

    public void ResetGameData()
    {
        score          = 0;
        health         = 100f;
        timer          = 20f;
        correctAnswers = 0;
        wrongChordIndices = new List<int>();
        wrongRootNotes    = new List<int>();
    }

    public void UpdateHighScore()
    {
        if (score > highScore) highScore = score;
    }

    public void LoadHighScores()
    {
        savedData data = SaveSystem.Loaddata();
        if (data == null) return;
        level1HighScore = data.l1_chords_highscore;
        level2HighScore = data.l2_chords_highscore;
        level3HighScore = data.l3_chords_highscore;
        level4HighScore = data.l4_chords_highscore;
    }

    // Helper: given a chord type index and a root note value,
    // return the set of note values that belong to the chord.
    public static HashSet<int> GetChordNoteValues(int chordTypeIndex, int rootNoteValue)
    {
        var result = new HashSet<int>();
        int[] formula = AllChords[chordTypeIndex];
        foreach (int interval in formula)
            result.Add((rootNoteValue + interval) % 12);
        return result;
    }

    // Helper: interval semitones from root to a note (0-11)
    public static int GetInterval(int rootNoteValue, int noteValue)
    {
        return (noteValue - rootNoteValue + 12) % 12;
    }
}
