using System;
using UnityEngine;

/// Static facade for the XP / Player Level / collectible-unlock / score-threshold
/// system. Save file (player.fun via SaveSystem) is the source of truth — every
/// public mutator reads, mutates, persists in one shot so scene transitions
/// don't lose state.
///
/// Streak and daily-bonus state are session-only and live as static fields here.
public static class EngagementManager
{
    // ── Tunables ──────────────────────────────────────────────────────────────
    const int   BASE_XP_PER_CORRECT = 10;
    const int   FIRST_TIME_BONUS    = 50;
    const float SPEED_BONUS_THRESHOLD_SECS = 3f;
    const float SPEED_MULT          = 1.25f;
    const float SURPRISE_DROP_RATE  = 0.08f;
    const int   MAX_PLAYER_LEVEL    = 100;

    // Mode multipliers ordered by LearningMode enum: Intervals, Notes, Progressions, Chords
    static readonly float[] modeMultiplier = { 1.5f, 1f, 1f, 2f };

    // Score thresholds [level-1] per mode. Tune via playtest.
    static readonly int[] notesThresholds     = { 80,  150, 250, 400 };
    static readonly int[] intervalsThresholds = { 100, 200, 350, 550 };
    static readonly int[] chordsThresholds    = { 120, 240, 420, 650 };

    // Collectible milestone schedule. Items 1-6 are seeded unlocked on first run
    // (savedData.DefaultUnlockedCollectibles). Items 7-14 unlock at these player levels.
    static readonly (int playerLevel, int itemId)[] collectibleMilestones = new[]
    {
        (3,  7),
        (5,  8),
        (8,  9),
        (12, 10),
        (16, 11),
        (20, 12),
        (25, 13),
        (30, 14),
    };

    // ── Session state (resets on app launch, not on scene change) ─────────────
    static int currentStreak = 0;
    static int sessionXPEarned = 0;       // XP gained since last StartSession()
    static int sessionStartLevel = 1;     // player level at last StartSession()

    // ── Events ────────────────────────────────────────────────────────────────
    public static event Action<int>           OnXPEarned;            // xp delta
    public static event Action<int, int>      OnLevelUp;             // (oldLevel, newLevel)
    public static event Action<int>           OnCollectibleUnlocked; // itemId
    public static event Action<LearningMode, int, int> OnLevelPassed; // (mode, level, newScore)

    // ── XP curve ──────────────────────────────────────────────────────────────
    /// Cumulative XP needed to *reach* level L. L=1 returns 0.
    public static int XPRequiredForLevel(int L)
    {
        if (L <= 1) return 0;
        return Mathf.FloorToInt(50f * Mathf.Pow(L, 1.6f));
    }

    public static int LevelForXP(int totalXP)
    {
        int L = 1;
        while (L < MAX_PLAYER_LEVEL && XPRequiredForLevel(L + 1) <= totalXP) L++;
        return L;
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
            default:                     return int.MaxValue; // Progressions: ignore
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

    /// Call when a question is answered wrong / timed out. Resets streak.
    public static void RegisterMiss()
    {
        currentStreak = 0;
    }

    /// Call once at the start of a play session (game manager Start).
    /// Tracks XP/level deltas so gameover can show "+250 XP" / "Level Up!".
    public static void StartSession()
    {
        sessionXPEarned   = 0;
        sessionStartLevel = GetPlayerLevel();
        currentStreak     = 0;
    }

    public static int  GetSessionXPEarned()  => sessionXPEarned;
    public static int  GetSessionStartLevel() => sessionStartLevel;
    public static bool DidSessionLevelUp()   => GetPlayerLevel() > sessionStartLevel;

    /// Call from gameover scripts after final score is known. Returns true if
    /// player passed the level threshold. Handles next-level unlock + surprise drop.
    public static bool RegisterLevelEnd(LearningMode mode, int level, int finalScore)
    {
        if (level < 1 || level > 4) return false;
        bool passed = finalScore >= ScoreThreshold(mode, level);
        if (!passed) return false;

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

        // Surprise collectible drop
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
        return true;
    }

    public static bool IsLevelUnlocked(LearningMode mode, int level)
    {
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

    public static int GetTotalXP()
    {
        var data = SaveSystem.Loaddata() ?? new savedData();
        return data.totalXP;
    }

    public static int GetPlayerLevel()
    {
        var data = SaveSystem.Loaddata() ?? new savedData();
        return data.playerLevel;
    }

    // ── Internals ─────────────────────────────────────────────────────────────
    static void AwardXP(int xp)
    {
        if (xp <= 0) return;
        var data = SaveSystem.Loaddata() ?? new savedData();
        int oldLevel = data.playerLevel;
        data.totalXP += xp;
        int newLevel = LevelForXP(data.totalXP);
        data.playerLevel = newLevel;

        if (newLevel > oldLevel)
        {
            for (int L = oldLevel + 1; L <= newLevel; L++)
            {
                foreach (var (lvl, itemId) in collectibleMilestones)
                {
                    if (lvl == L && Array.IndexOf(data.unlockedCollectibles, itemId) < 0)
                    {
                        data.unlockedCollectibles = AppendUnique(data.unlockedCollectibles, itemId);
                        OnCollectibleUnlocked?.Invoke(itemId);
                    }
                }
            }
        }

        sessionXPEarned += xp;
        SaveSystem.SavePlayer(data);
        OnXPEarned?.Invoke(xp);
        if (newLevel > oldLevel) OnLevelUp?.Invoke(oldLevel, newLevel);
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

    /// Returns 0 if no locked items remain.
    static int RollLockedItem(savedData data)
    {
        // Locked = id in [1..14] not in unlockedCollectibles. Surprise drops only
        // pick from items not already covered by milestones, so any valid ID works.
        const int totalItemIds = 14;
        var lockedIds = new System.Collections.Generic.List<int>();
        for (int id = 1; id <= totalItemIds; id++)
        {
            if (Array.IndexOf(data.unlockedCollectibles, id) < 0) lockedIds.Add(id);
        }
        if (lockedIds.Count == 0) return 0;
        return lockedIds[UnityEngine.Random.Range(0, lockedIds.Count)];
    }
}
