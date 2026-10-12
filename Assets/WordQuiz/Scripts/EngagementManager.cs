using System;
using UnityEngine;

/// <summary>
/// The whole progression system: XP, avatar stage, level unlocks, score goals.
///
/// There is exactly one number the player sees grow: total XP. Avatar stage is
/// derived from it, never stored. Three modes only — Intervals, Notes, Chords.
///
/// The save file is the source of truth. Every mutator loads, changes and
/// persists in one go, so nothing is lost across a scene change.
/// </summary>
public static class EngagementManager
{
    // ── XP economy ───────────────────────────────────────────────────────────
    const int   BASE_XP_PER_CORRECT = 3;
    const int   FIRST_TIME_BONUS    = 20;
    const float SPEED_BONUS_SECS    = 3f;
    const float SPEED_MULT          = 1.25f;

    // The chunks that should actually feel like a reward.
    const int   LEVEL_PASS_BASE_XP  = 250;   // × level, so L1=250 ... L4=1000
    const int   NEW_HIGH_SCORE_XP   = 200;

    /// Indexed by LearningMode: Intervals, Notes, Chords.
    static readonly float[] modeMultiplier = { 1.5f, 1f, 2f };

    /// Score needed to clear each level, [level-1]. Tune from playtests.
    static readonly int[] notesThresholds     = {  80, 150, 250, 400 };
    static readonly int[] intervalsThresholds = { 100, 200, 350, 550 };
    static readonly int[] chordsThresholds    = { 120, 240, 420, 650 };

    /// Total XP required for avatar stages 1-10. Front-loaded so the first
    /// evolution lands after roughly one good session.
    static readonly int[] avatarStageXP =
    {
        0,      //  stage 1 (fresh install)
        500,    //  stage 2
        1500,   //  stage 3
        3000,   //  stage 4
        6000,   //  stage 5
        10000,  //  stage 6
        16000,  //  stage 7
        24000,  //  stage 8
        36000,  //  stage 9
        50000,  //  stage 10
    };

    // ── Session state (per app launch, not per scene) ────────────────────────
    static int  currentStreak     = 0;
    static int  sessionXPEarned   = 0;
    static int  sessionStartStage = 1;
    static bool isInPlaySession   = false;

    // ── Events ───────────────────────────────────────────────────────────────
    /// Every XP grant. Fires during gameplay too, so UI should stay quiet.
    public static event Action<int>      OnXPEarned;
    /// Avatar stage advanced (oldStage, newStage), both 1-10.
    public static event Action<int, int> OnAvatarEvolved;
    /// A level was cleared (mode, level, score).
    public static event Action<LearningMode, int, int> OnLevelPassed;
    /// End of every session, pass or fail. Deferred UI flushes here.
    public static event Action OnSessionEnd;

    /// True between StartSession and EndSession. UI uses this to hold back
    /// popups while the player is under time pressure.
    public static bool IsInPlaySession => isInPlaySession;

    // ── Avatar stage ─────────────────────────────────────────────────────────
    public static int AvatarStageForXP(int xp)
    {
        int stage = 1;
        for (int i = 0; i < avatarStageXP.Length; i++)
            if (xp >= avatarStageXP[i]) stage = i + 1;
        return stage;
    }

    public static int GetAvatarStage() => AvatarStageForXP(GetTotalXP());

    public static int GetTotalXP() => Load().totalXP;

    /// XP still needed for the next stage, or -1 at max stage.
    public static int XPToNextAvatarStage()
    {
        int xp = GetTotalXP();
        for (int i = 0; i < avatarStageXP.Length; i++)
            if (xp < avatarStageXP[i]) return avatarStageXP[i] - xp;
        return -1;
    }

    // ── Score goals ──────────────────────────────────────────────────────────
    /// Score needed to clear this level, or int.MaxValue for ungated levels.
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

    // ── Gameplay hooks ───────────────────────────────────────────────────────
    /// Call on every correct answer. Returns the XP awarded.
    public static int RegisterCorrectAnswer(LearningMode mode, float secondsToAnswer, bool firstTimeOnPosition)
    {
        currentStreak++;
        float streakMult =
            currentStreak >= 20 ? 3f :
            currentStreak >= 10 ? 2f :
            currentStreak >= 5  ? 1.5f : 1f;

        float speedMult = secondsToAnswer < SPEED_BONUS_SECS ? SPEED_MULT : 1f;
        float modeMult  = modeMultiplier[(int)mode];
        float dailyMult = TryConsumeDailyFirst() ? 2f : 1f;

        int xp = Mathf.RoundToInt(BASE_XP_PER_CORRECT * streakMult * speedMult * modeMult * dailyMult);
        if (firstTimeOnPosition) xp += FIRST_TIME_BONUS;

        AwardXP(xp);
        return xp;
    }

    public static void RegisterMiss() => currentStreak = 0;

    /// Call from each game manager's Start.
    public static void StartSession()
    {
        sessionXPEarned   = 0;
        sessionStartStage = GetAvatarStage();
        currentStreak     = 0;
        isInPlaySession   = true;
    }

    /// Call from each gameover screen. Awards the pass and high-score chunks,
    /// unlocks the next level, then fires OnSessionEnd so deferred UI can show.
    /// Returns true if the level was cleared.
    public static bool EndSession(LearningMode mode, int level, int finalScore, bool isNewHighScore)
    {
        bool passed = finalScore >= ScoreThreshold(mode, level);

        if (passed)         AwardXP(LEVEL_PASS_BASE_XP * level);
        if (isNewHighScore) AwardXP(NEW_HIGH_SCORE_XP);

        if (passed)
        {
            UnlockLevel(mode, level + 1);
            OnLevelPassed?.Invoke(mode, level, finalScore);
        }

        isInPlaySession = false;
        OnSessionEnd?.Invoke();
        return passed;
    }

    // ── Level unlocks ────────────────────────────────────────────────────────
    /// Custom (level 5) is always playable. L1 is unlocked on a fresh install.
    public static bool IsLevelUnlocked(LearningMode mode, int level)
    {
        int idx = savedData.LevelIndex(mode, level);
        if (idx < 0) return true;              // Custom / out of range: never gated
        var data = Load();
        return idx < data.unlockedLevels.Length && data.unlockedLevels[idx] == 1;
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

    // ── Session readouts for the gameover screen ─────────────────────────────
    public static int  GetSessionXPEarned()     => sessionXPEarned;
    public static int  GetSessionStartStage()   => sessionStartStage;
    public static bool DidSessionAvatarEvolve() => GetAvatarStage() > sessionStartStage;

    // ── Internals ────────────────────────────────────────────────────────────
    static savedData Load() => SaveSystem.Loaddata() ?? new savedData();

    static void AwardXP(int xp)
    {
        if (xp <= 0) return;

        var data = Load();
        int oldStage = AvatarStageForXP(data.totalXP);
        data.totalXP += xp;
        int newStage = AvatarStageForXP(data.totalXP);

        sessionXPEarned += xp;
        SaveSystem.SavePlayer(data);

        OnXPEarned?.Invoke(xp);
        if (newStage > oldStage) OnAvatarEvolved?.Invoke(oldStage, newStage);
    }

    /// True once per real-world day, for the first-answer-of-the-day bonus.
    static bool TryConsumeDailyFirst()
    {
        int today = TodayYmd();
        var data = Load();
        if (data.lastDailyBonusYmd == today) return false;

        data.lastDailyBonusYmd = today;
        SaveSystem.SavePlayer(data);
        return true;
    }

    static int TodayYmd()
    {
        var d = DateTime.Now.Date;
        return d.Year * 10000 + d.Month * 100 + d.Day;
    }
}
