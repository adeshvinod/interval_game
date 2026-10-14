/// <summary>
/// Everything the game persists, in one flat object.
///
/// JsonUtility serialises this type directly — there is no wrapper class and no
/// array flattening, because no field here is a multidimensional array. If you
/// ever add an <c>int[,]</c>, JsonUtility will silently drop it; use a flat
/// array instead.
///
/// Three modes only: Intervals, Notes, Chords (see <see cref="LearningMode"/>).
/// There is no XP or currency: the avatar is derived from the high scores
/// below meeting EngagementManager's thresholds, so it needs no field here.
/// </summary>
[System.Serializable]
public class savedData
{
    /// Levels per mode (1-4). Level 5 is "Custom" and is never gated or scored.
    public const int LEVELS_PER_MODE = 4;
    /// Number of LearningMode values. Keep in sync with the enum.
    public const int MODE_COUNT = 3;
    /// Guitar strings, for the tuning array.
    public const int STRING_COUNT = 6;

    // ── Per-level high scores ────────────────────────────────────────────────
    public int l1_intervals_highscore;
    public int l2_intervals_highscore;
    public int l3_intervals_highscore;
    public int l4_intervals_highscore;

    public int l1_notes_highscore;
    public int l2_notes_highscore;
    public int l3_notes_highscore;
    public int l4_notes_highscore;

    public int l1_chords_highscore;
    public int l2_chords_highscore;
    public int l3_chords_highscore;
    public int l4_chords_highscore;

    // ── Tuning ───────────────────────────────────────────────────────────────
    /// Semitone offset per string, index 0 = high E.
    public int[] transposedNotes_audio;

    // ── Progression ──────────────────────────────────────────────────────────
    /// Length MODE_COUNT * LEVELS_PER_MODE. Index = (int)mode * LEVELS_PER_MODE
    /// + (level - 1). 0 = locked, 1 = unlocked. Only L1 of each mode starts
    /// unlocked; L2-L4 are earned by clearing the level before them.
    public int[] unlockedLevels;

    /// True once the player has reached a gameover screen at least once, win or
    /// lose. Earns the second avatar, so the very first session is rewarded
    /// before anything has been cleared.
    public bool hasFinishedAGame;


    public savedData()
    {
        transposedNotes_audio = new int[STRING_COUNT];
        unlockedLevels = DefaultUnlockedLevels();
    }

    /// Fresh install: only level 1 of each mode is playable.
    public static int[] DefaultUnlockedLevels()
    {
        var arr = new int[MODE_COUNT * LEVELS_PER_MODE];
        for (int mode = 0; mode < MODE_COUNT; mode++)
            arr[mode * LEVELS_PER_MODE] = 1;
        return arr;
    }

    /// Flat index into <see cref="unlockedLevels"/>, or -1 for a level that is
    /// never gated (Custom, or anything out of range).
    public static int LevelIndex(LearningMode mode, int level)
    {
        if (level < 1 || level > LEVELS_PER_MODE) return -1;
        return (int)mode * LEVELS_PER_MODE + (level - 1);
    }
}
