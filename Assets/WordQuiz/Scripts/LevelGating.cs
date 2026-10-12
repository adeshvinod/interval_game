using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shared behaviour for the three level-select screens (notes_levels,
/// interval_levels, chords_levels).
///
/// Levels 2-4 stay locked until the level before them is cleared. Locking is
/// applied in exactly one place — <see cref="Button.interactable"/> — so a
/// locked level simply cannot be tapped. Do not add extra guards inside the
/// click handlers: that is what previously broke Custom (level 5).
///
/// A locked knob shows "LOCKED" where its high score would be, so the player
/// can see why it will not respond.
/// </summary>
public static class LevelGating
{
    public const string LockedLabel = "LOCKED";

    /// <summary>
    /// Greys out locked level buttons and fills in each knob's high score.
    /// </summary>
    /// <param name="mode">Which mode's unlock state to read.</param>
    /// <param name="highScoreLabels">Labels for levels 1-4, in order. Entries
    /// may be null if a scene has not wired them yet.</param>
    /// <param name="highScores">High scores for levels 1-4, in order.</param>
    public static void Apply(LearningMode mode, TextMeshProUGUI[] highScoreLabels, int[] highScores)
    {
        for (int level = 1; level <= savedData.LEVELS_PER_MODE; level++)
        {
            bool unlocked = EngagementManager.IsLevelUnlocked(mode, level);

            // Buttons are named lv1_btn ... lv4_btn in all three scenes.
            // lv5_btn is Custom and is deliberately never touched.
            var go = GameObject.Find($"lv{level}_btn");
            if (go != null)
            {
                var btn = go.GetComponent<Button>();
                if (btn != null) btn.interactable = unlocked;
            }

            int i = level - 1;
            var label = (highScoreLabels != null && i < highScoreLabels.Length) ? highScoreLabels[i] : null;
            if (label == null) continue;

            int score = (highScores != null && i < highScores.Length) ? highScores[i] : 0;
            SetFitted(label, unlocked ? score.ToString() : LockedLabel);
        }
    }

    // The knob labels are ~108px wide and were authored at a fixed 36pt with
    // wrapping on, which is fine for "300" but splits "LOCKED" across two lines.
    // Force one line and let TMP shrink only as far as it needs to.
    const float FontSizeMin = 14f;
    const float FontSizeMax = 36f;

    static void SetFitted(TextMeshProUGUI label, string text)
    {
        label.text              = text;
        label.textWrappingMode  = TextWrappingModes.NoWrap;
        label.overflowMode      = TextOverflowModes.Overflow;
        label.enableAutoSizing  = true;
        label.fontSizeMin       = FontSizeMin;
        label.fontSizeMax       = FontSizeMax;
    }
}
