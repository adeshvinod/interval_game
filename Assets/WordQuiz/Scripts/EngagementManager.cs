using System;
using UnityEngine;
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
    /// Avatar names in unlock order. Index 0 = stage 1.
    /// Stage 1 is free, stage 2 is earned by finishing a first game, and
    /// stages 3-14 are one per level cleared (12 levels in total), so the last
    /// avatar means every level in every mode has been beaten.
    public static readonly string[] AvatarNames =
    {
        "Guitar Novice",      //  1  start
        "Captain Capo",       //  2  finish your first game
        "Rodeo Rose",         //  3  1 level cleared
        "Disco Duchess",      //  4  2
        "Mohawk Maven",       //  5  3
        "Riff Rager",         //  6  4
        "Midnight Bluesman",  //  7  5
        "Shred Samurai",      //  8  6
        "Count Shredula",     //  9  7
        "Bone Shredder",      // 10  8
        "Robo Riffer",        // 11  9
        "Guitar Sorcerer",    // 12  10
        "Fretboard King",     // 13  11
        "Fretboard Zeus",     // 14  all 12
    };

    /// Sprite file name per stage, so the gallery and the menu agree on which
    /// art belongs to which stage.
    public static readonly string[] AvatarSprites =
    {
        "novice2 2",
        "captain_capo",
        "rodeo_rose",
        "disco_duchess",
        "mohawk_maven",
        "riff_rager",
        "midnight_bluesman",
        "shred_samurai",
        "count_shredula",
        "bone_shredder",
        "robo_riffer",
        "wizard2-removebg-preview 1",
        "fretboard_king",
        "DALL_E_2024-06-05_14.57.43_-_A_minimalistic_cartoon_illustration_of_the_Greek_god_Zeus_playing_an_electric_guitar__standing_against_a_white_background 1",
    };

    public static int AvatarCount => AvatarNames.Length;

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
    /// How many of the 12 levels have been cleared, in any order.
    public static int CountLevelsCleared()
    {
        var data = Load();
        int n = 0;
        foreach (var m in AllModes)
            for (int level = 1; level <= savedData.LEVELS_PER_MODE; level++)
                if (HighScore(data, m, level) >= ScoreThreshold(m, level)) n++;
        return n;
    }

    public static bool HasFinishedAGame() => Load().hasFinishedAGame;

    /// 1..14. One avatar per level cleared, plus one for finishing a first
    /// game, so the player is rewarded before clearing anything.
    public static int GetAvatarStage()
    {
        var data = Load();

        int cleared = 0;
        foreach (var m in AllModes)
            for (int level = 1; level <= savedData.LEVELS_PER_MODE; level++)
                if (HighScore(data, m, level) >= ScoreThreshold(m, level)) cleared++;

        int stage = 1 + (data.hasFinishedAGame ? 1 : 0) + cleared;
        return Mathf.Clamp(stage, 1, AvatarCount);
    }

    public static string GetAvatarName() => AvatarNameForStage(GetAvatarStage());

    public static string AvatarNameForStage(int stage)
    {
        int i = Math.Max(0, Math.Min(AvatarNames.Length - 1, stage - 1));
        return AvatarNames[i];
    }

    /// One line describing what earns the next avatar, or "" when maxed.
    public static string NextAvatarRequirement()
    {
        int stage = GetAvatarStage();
        if (stage >= AvatarCount) return "";

        if (!HasFinishedAGame())
            return "Finish a game to unlock " + AvatarNameForStage(2);

        int remaining = savedData.MODE_COUNT * savedData.LEVELS_PER_MODE - CountLevelsCleared();
        string next = AvatarNameForStage(stage + 1);
        return remaining == 1
            ? $"Clear the last level to unlock {next}"
            : $"Clear any level to unlock {next}";
    }

    /// Progress towards the full set, for a "7 / 14" style readout.
    public static int LevelsClearedOutOf => savedData.MODE_COUNT * savedData.LEVELS_PER_MODE;

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

        // Reaching a gameover screen at all earns the second avatar.
        var start = Load();
        if (!start.hasFinishedAGame)
        {
            start.hasFinishedAGame = true;
            SaveSystem.SavePlayer(start);
        }

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
