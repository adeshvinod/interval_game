using System;
using UnityEngine;

/// XP / avatar-stage / collectible-unlock / score-threshold spine.
///
/// Save file (player.fun via SaveSystem) is the source of truth — every
/// public mutator reads, mutates, persists in one shot so scene transitions
/// don't lose state.
///
/// Streak, daily-bonus, in-play state are session-level static fields here.
public static class EngagementManager
{
    // ── XP economy tunables ───────────────────────────────────────────────────
    const int   BASE_XP_PER_CORRECT = 3;     // Tiny per-correct trickle.
    const int   FIRST_TIME_BONUS    = 20;
    const float SPEED_BONUS_THRESHOLD_SECS = 3f;
    const float SPEED_MULT          = 1.25f;
    const float SURPRISE_DROP_RATE  = 0.08f;

    // Big chunks (the rewards that should *feel* big)
    const int   LEVEL_PASS_BASE_XP   = 250;   // multiplied by level number → L1=250, L4=1000
    const int   NEW_HIGH_SCORE_XP    = 200;

    // Mode multipliers ordered by LearningMode enum: Intervals, Notes, Progressions, Chords
    static readonly float[] modeMultiplier = { 1.5f, 1f, 1f, 2f };

    // Score thresholds [level-1] per mode. Tune via playtest.
    static readonly int[] notesThresholds     = { 80,  150, 250, 400 };
    static readonly int[] intervalsThresholds = { 100, 200, 350, 550 };
    static readonly int[] chordsThresholds    = { 120, 240, 420, 650 };

    // ── Avatar stage thresholds ───────────────────────────────────────────────
    // Stage = 1..10. Stage 1 = fresh install. Stage 2 unlocks at 500 XP, etc.
    // Front-loaded so first evolution arrives in ~1 great session.
    static readonly int[] avatarStageXP =
    {
        0,      // stage 1 (start)
        500,    // stage 2
        1500,   // stage 3
        3000,   // stage 4
        6000,   // stage 5
        10000,  // stage 6
        16000,  // stage 7
        24000,  // stage 8
        36000,  // stage 9
        50000,  // stage 10
    };

    // ── Collectible XP-unlock schedule ────────────────────────────────────────
    // Item IDs 1..14, unlocked when totalXP first crosses the threshold. Some
    // align with avatar stages so a single moment delivers a compound payoff.
    static readonly (int xp, int itemId)[] collectibleUnlocks = new[]
    {
        (100,    1),
        (300,    2),
        (700,    3),
        (1500,   4),
        (2500,   5),
        (4000,   6),
        (6000,   7),   // ~ avatar stage 5
        (9000,   8),
        (13000,  9),
        (18000, 10),   // ~ avatar stage 7
        (25000, 11),
        (33000, 12),
        (42000, 13),
        (50000, 14),   // legendary, aligns with stage 10
    };

    // ── Session state (resets on app launch, not on scene change) ─────────────
    static int  currentStreak    = 0;
    static int  sessionXPEarned  = 0;
    static int  sessionStartStage = 1;
    static bool isInPlaySession  = false;

    // ── Events ────────────────────────────────────────────────────────────────
    /// Fired on every XP grant. UI may want to ignore this during gameplay.
    public static event Action<int>           OnXPEarned;
    /// Fired when avatar stage advances (oldStage, newStage). Stages are 1..10.
    public static event Action<int, int>      OnAvatarEvolved;
    /// Fired the moment a collectible is unlocked. May fire mid-gameplay.
    public static event Action<int>           OnCollectibleUnlocked;
    /// Fired from gameover when a level is cleared. (mode, level, score).
    public static event Action<LearningMode, int, int> OnLevelPassed;
    /// Fired at the very end of every session (pass or fail). Subscribers can
    /// flush any deferred UI here.
    public static event Action OnSessionEnd;

    /// True between StartSession() and EndSession(). UI uses this to defer
    /// distracting popups until the player isn't focused on gameplay.
    public static bool IsInPlaySession => isInPlaySession;

    // ── Avatar stage (derived from totalXP) ───────────────────────────────────
    public static int AvatarStageForXP(int xp)
    {
        int stage = 1;
        for (int i = 0; i < avatarStageXP.Length; i++)
            if (xp >= avatarStageXP[i]) stage = i + 1;
        return stage;
    }

    public static int GetAvatarStage()  => AvatarStageForXP(GetTotalXP());
    public static int GetTotalXP()
    {
        var data = SaveSystem.Loaddata() ?? new savedData();
        return data.totalXP;
    }

    /// XP needed for the next avatar stage, or -1 if at max.
    public static int XPToNextAvatarStage()
    {
        int xp = GetTotalXP();
        for (int i = 0; i < avatarStageXP.Length; i++)
            if (xp < avatarStageXP[i]) return avatarStageXP[i] - xp;
        return -1;
    }

    // ── Score thresholds ──────────────────────────────────────────────────────
    public static int ScoreThreshold(LearningMode mode, int level)
    {
        if (level < 1 || level > 4) return int.MaxValue;
        switch (mode)
        {
            case LearningMode.Notes:     return notesThresholds[level - 1];
            case LearningMode.Intervals: return intervalsThresholds[level - 1];
            case LearningMode.Chords:    return chordsThresholds[level - 1];
            default:                     return int.MaxValue;
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────
    /// Call on every correct answer. Returns XP awarded.
    public static int RegisterCorrectAnswer(LearningMode mode, float secondsToAnswer, bool firstTimeOnPosition)
    {
        currentStreak++;
        float streakMult =
            currentStreak >= 20 ? 3f :
            currentStreak >= 10 ? 2f :
            currentStreak >= 5  ? 1.5f : 1f;

        float speedMult  = secondsToAnswer < SPEED_BONUS_THRESHOLD_SECS ? SPEED_MULT : 1f;
        float modeMult   = modeMultiplier[(int)mode];
        float dailyMult  = TryConsumeDailyFirst() ? 2f : 1f;

        int xp = Mathf.RoundToInt(BASE_XP_PER_CORRECT * streakMult * speedMult * modeMult * dailyMult);
        if (firstTimeOnPosition) xp += FIRST_TIME_BONUS;

        AwardXP(xp);
        return xp;
    }

    public static void RegisterMiss() => currentStreak = 0;

    /// Call once at the start of a play session (game-manager Start).
    public static void StartSession()
    {
        sessionXPEarned    = 0;
        sessionStartStage  = GetAvatarStage();
        currentStreak      = 0;
        isInPlaySession    = true;
    }

    /// Call from gameover scripts. Awards level-pass + new-HS chunks, unlocks
    /// next level, rolls surprise drop, and finally fires OnSessionEnd so any
    /// deferred UI (popups) can flush.
    public static bool EndSession(LearningMode mode, int level, int finalScore, bool isNewHighScore)
    {
        bool passed = level >= 1 && level <= 4 && finalScore >= ScoreThreshold(mode, level);

        if (passed)
        {
            // Big chunk for clearing — bigger for harder levels.
            AwardXP(LEVEL_PASS_BASE_XP * level);
        }

        if (isNewHighScore)
        {
            AwardXP(NEW_HIGH_SCORE_XP);
        }

        if (passed)
        {
            var data = SaveSystem.Loaddata() ?? new savedData();
            bool dirty = false;

            if (level < 4)
            {
                int idx = LevelIndex(mode, level + 1);
                if (idx >= 0 && idx < data.unlockedLevels.Length && data.unlockedLevels[idx] == 0)
                {
                    data.unlockedLevels[idx] = 1;
                    dirty = true;
                }
            }

            if (UnityEngine.Random.value < SURPRISE_DROP_RATE)
            {
                int rolled = RollLockedItem(data);
                if (rolled > 0)
                {
                    data.unlockedCollectibles = AppendUnique(data.unlockedCollectibles, rolled);
                    dirty = true;
                    OnCollectibleUnlocked?.Invoke(rolled);
                }
            }

            if (dirty) SaveSystem.SavePlayer(data);
            OnLevelPassed?.Invoke(mode, level, finalScore);
        }

        isInPlaySession = false;
        OnSessionEnd?.Invoke();
        return passed;
    }

    public static bool IsLevelUnlocked(LearningMode mode, int level)
    {
        // Custom (level 5) is always allowed — never gated.
        if (level >= 5) return true;
        var data = SaveSystem.Loaddata() ?? new savedData();
        int idx = LevelIndex(mode, level);
        if (idx < 0 || idx >= data.unlockedLevels.Length) return false;
        return data.unlockedLevels[idx] == 1;
    }

    public static bool IsCollectibleUnlocked(int itemId)
    {
        var data = SaveSystem.Loaddata() ?? new savedData();
        return Array.IndexOf(data.unlockedCollectibles, itemId) >= 0;
    }

    public static int GetSessionXPEarned()    => sessionXPEarned;
    public static int GetSessionStartStage()  => sessionStartStage;
    public static bool DidSessionAvatarEvolve() => GetAvatarStage() > sessionStartStage;

    // ── Internals ─────────────────────────────────────────────────────────────
    static void AwardXP(int xp)
    {
        if (xp <= 0) return;
        var data = SaveSystem.Loaddata() ?? new savedData();
        int oldXP    = data.totalXP;
        int oldStage = AvatarStageForXP(oldXP);
        data.totalXP += xp;
        int newXP    = data.totalXP;
        int newStage = AvatarStageForXP(newXP);

        // Unlock any collectibles whose XP threshold was crossed.
        foreach (var (threshold, itemId) in collectibleUnlocks)
        {
            if (oldXP < threshold && newXP >= threshold &&
                Array.IndexOf(data.unlockedCollectibles, itemId) < 0)
            {
                data.unlockedCollectibles = AppendUnique(data.unlockedCollectibles, itemId);
                OnCollectibleUnlocked?.Invoke(itemId);
            }
        }

        sessionXPEarned += xp;
        SaveSystem.SavePlayer(data);
        OnXPEarned?.Invoke(xp);
        if (newStage > oldStage) OnAvatarEvolved?.Invoke(oldStage, newStage);
    }

    static bool TryConsumeDailyFirst()
    {
        int todayYmd = TodayYmd();
        var data = SaveSystem.Loaddata() ?? new savedData();
        if (data.lastDailyBonusYmd != todayYmd)
        {
            data.lastDailyBonusYmd = todayYmd;
            SaveSystem.SavePlayer(data);
            return true;
        }
        return false;
    }

    static int TodayYmd()
    {
        var d = DateTime.Now.Date;
        return d.Year * 10000 + d.Month * 100 + d.Day;
    }

    static int LevelIndex(LearningMode mode, int level)
    {
        if (level < 1 || level > 4) return -1;
        return (int)mode * 4 + (level - 1);
    }

    static int[] AppendUnique(int[] arr, int val)
    {
        if (arr == null) return new[] { val };
        if (Array.IndexOf(arr, val) >= 0) return arr;
        var copy = new int[arr.Length + 1];
        Array.Copy(arr, copy, arr.Length);
        copy[arr.Length] = val;
        return copy;
    }

    static int RollLockedItem(savedData data)
    {
        const int totalItemIds = 14;
        var lockedIds = new System.Collections.Generic.List<int>();
        for (int id = 1; id <= totalItemIds; id++)
            if (Array.IndexOf(data.unlockedCollectibles, id) < 0) lockedIds.Add(id);
        if (lockedIds.Count == 0) return 0;
        return lockedIds[UnityEngine.Random.Range(0, lockedIds.Count)];
    }
}
