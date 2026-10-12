using System;
using System.Collections.Generic;

/// <summary>
/// Progression: level unlocks, score goals, and the avatar.
///
/// There is no XP and no currency. The only thing that advances the player is
/// clearing levels, and the avatar is the single visible reward.
///
/// **The avatar rule:** the avatar evolves only when the same level number has
/// been cleared in *all three* modes. Clearing Notes level 1 alone does nothing
/// — Intervals and Chords level 1 are needed too. So there are five avatars for
/// four levels:
///
///   Stage 1  Guitar Novice     fresh install
///   Stage 2  Riff Rager        level 1 cleared in all three modes
///   Stage 3  Guitar Sorcerer   level 2 cleared in all three modes
///   Stage 4  Fretboard King    level 3 cleared in all three modes
///   Stage 5  Fretboard Zeus    level 4 cleared in all three modes
///
/// "Cleared" is derived from the stored high score meeting the level's
/// threshold, so it needs no extra save field and stays correct if a high
/// score is beaten later.
/// </summary>
public static class EngagementManager
{
    /// Avatar names in stage order. Index 0 = stage 1.
    public static readonly string[] AvatarNames =
    {
        "Guitar Novice",
        "Riff Rager",
        "Guitar Sorcerer",
        "Fretboard King",
        "Fretboard Zeus",
    };

    /// The three modes, in the order they are shown to the player.
    public static readonly LearningMode[] AllModes =
    {
        LearningMode.Notes, LearningMode.Intervals, LearningMode.Chords,
    };

    /// Score needed to clear each level, [level-1]. Tune from playtests.
    static readonly int[] notesThresholds     = {  80, 150, 250, 400 };
    static readonly int[] intervalsThresholds = { 100, 200, 350, 550 };
    static readonly int[] chordsThresholds    = { 120, 240, 420, 650 };

    // ── Session state (per app launch) ───────────────────────────────────────
    static int  sessionStartStage = 1;
    static bool isInPlaySession   = false;

    // ── Events ───────────────────────────────────────────────────────────────
    /// A level was cleared (mode, level, score). Fires before OnAvatarEvolved.
    public static event Action<LearningMode, int, int> OnLevelPassed;
    /// The avatar advanced (oldStage, newStage), both 1..5.
    public static event Action<int, int> OnAvatarEvolved;
    /// End of every session, pass or fail. Deferred UI flushes here.
    public static event Action OnSessionEnd;

    /// True between StartSession and EndSession, so UI can hold back popups
    /// while the player is under time pressure.
    public static bool IsInPlaySession => isInPlaySession;

    // ── Score goals ──────────────────────────────────────────────────────────
    /// Score needed to clear this level, or int.MaxValue for ungated levels
    /// (Custom, or anything outside 1..4).
    public static int ScoreThreshold(LearningMode mode, int level)
    {
        if (level < 1 || level > savedData.LEVELS_PER_MODE) return int.MaxValue;
        switch (mode)
        {
            case LearningMode.Notes:     return notesThresholds[level - 1];
            case LearningMode.Intervals: return intervalsThresholds[level - 1];
            case LearningMode.Chords:    return chordsThresholds[level - 1];
            default:                     return int.MaxValue;
        }
    }

    // ── Cleared / unlocked ───────────────────────────────────────────────────
    /// A level is cleared once its stored high score meets the threshold.
    public static bool IsLevelCleared(LearningMode mode, int level)
    {
        if (level < 1 || level > savedData.LEVELS_PER_MODE) return false;
        return HighScore(Load(), mode, level) >= ScoreThreshold(mode, level);
    }

    /// Custom (level 5) is always playable. Level 1 is unlocked on install.
    public static bool IsLevelUnlocked(LearningMode mode, int level)
    {
        int idx = savedData.LevelIndex(mode, level);
        if (idx < 0) return true;
        var data = Load();
        return idx < data.unlockedLevels.Length && data.unlockedLevels[idx] == 1;
    }

    // ── Avatar ───────────────────────────────────────────────────────────────
    /// 1..5. Advances one step per level number cleared in every mode.
    public static int GetAvatarStage()
    {
        var data = Load();
        int stage = 1;
        for (int level = 1; level <= savedData.LEVELS_PER_MODE; level++)
        {
            if (!ClearedInAllModes(data, level)) break;   // tiers are consecutive
            stage++;
        }
        return stage;
    }

    public static string GetAvatarName() => AvatarNameForStage(GetAvatarStage());

    public static string AvatarNameForStage(int stage)
    {
        int i = Math.Max(0, Math.Min(AvatarNames.Length - 1, stage - 1));
        return AvatarNames[i];
    }

    /// The level number the player is currently working towards across all
    /// modes, i.e. the one that will trigger the next evolution. 0 when maxed.
    public static int LevelNeededForNextAvatar()
    {
        int stage = GetAvatarStage();
        return stage > savedData.LEVELS_PER_MODE ? 0 : stage;
    }

    /// Which modes still need <see cref="LevelNeededForNextAvatar"/> cleared.
    /// Empty when the avatar is fully evolved.
    public static List<LearningMode> ModesMissingForNextAvatar()
    {
        var missing = new List<LearningMode>();
        int level = LevelNeededForNextAvatar();
        if (level == 0) return missing;

        var data = Load();
        foreach (var m in AllModes)
            if (HighScore(data, m, level) < ScoreThreshold(m, level)) missing.Add(m);
        return missing;
    }

    /// "Intervals and Chords", "Chords", or "" — ready for a sentence.
    public static string DescribeMissingModes()
    {
        var missing = ModesMissingForNextAvatar();
        if (missing.Count == 0) return "";
        if (missing.Count == 1) return missing[0].ToString();
        if (missing.Count == 2) return $"{missing[0]} and {missing[1]}";
        return $"{missing[0]}, {missing[1]} and {missing[2]}";
    }

    // ── Session hooks ────────────────────────────────────────────────────────
    /// Call from each game manager's Start.
    public static void StartSession()
    {
        sessionStartStage = GetAvatarStage();
        isInPlaySession   = true;
    }

    /// Call from each gameover screen. Unlocks the next level, then reports a
    /// clear and any avatar evolution. Returns true if the level was cleared.
    ///
    /// The gameover scripts persist the new high score *before* calling this,
    /// which is what lets IsLevelCleared see it.
    public static bool EndSession(LearningMode mode, int level, int finalScore, bool isNewHighScore)
    {
        bool passed = finalScore >= ScoreThreshold(mode, level);

        if (passed)
        {
            UnlockLevel(mode, level + 1);
            OnLevelPassed?.Invoke(mode, level, finalScore);

            int newStage = GetAvatarStage();
            if (newStage > sessionStartStage)
                OnAvatarEvolved?.Invoke(sessionStartStage, newStage);
        }

        isInPlaySession = false;
        OnSessionEnd?.Invoke();
        return passed;
    }

    public static int  GetSessionStartStage()   => sessionStartStage;
    public static bool DidSessionAvatarEvolve() => GetAvatarStage() > sessionStartStage;

    // ── Internals ────────────────────────────────────────────────────────────
    static savedData Load() => SaveSystem.Loaddata() ?? new savedData();

    static bool ClearedInAllModes(savedData data, int level)
    {
        foreach (var m in AllModes)
            if (HighScore(data, m, level) < ScoreThreshold(m, level)) return false;
        return true;
    }

    static void UnlockLevel(LearningMode mode, int level)
    {
        int idx = savedData.LevelIndex(mode, level);
        if (idx < 0) return;

        var data = Load();
        if (idx >= data.unlockedLevels.Length || data.unlockedLevels[idx] == 1) return;

        data.unlockedLevels[idx] = 1;
        SaveSystem.SavePlayer(data);
    }

    /// The stored high score for one mode/level. Verbose on purpose — the save
    /// fields are individually named rather than an array.
    static int HighScore(savedData d, LearningMode mode, int level)
    {
        switch (mode)
        {
            case LearningMode.Intervals:
                switch (level)
                {
                    case 1: return d.l1_intervals_highscore;
                    case 2: return d.l2_intervals_highscore;
                    case 3: return d.l3_intervals_highscore;
                    case 4: return d.l4_intervals_highscore;
                }
                break;
            case LearningMode.Notes:
                switch (level)
                {
                    case 1: return d.l1_notes_highscore;
                    case 2: return d.l2_notes_highscore;
                    case 3: return d.l3_notes_highscore;
                    case 4: return d.l4_notes_highscore;
                }
                break;
            case LearningMode.Chords:
                switch (level)
                {
                    case 1: return d.l1_chords_highscore;
                    case 2: return d.l2_chords_highscore;
                    case 3: return d.l3_chords_highscore;
                    case 4: return d.l4_chords_highscore;
                }
                break;
        }
        return 0;
    }
}
