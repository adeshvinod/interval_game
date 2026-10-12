using UnityEngine;
using TMPro;

/// <summary>
/// Drives the interval_levels scene: shows each level's high score and locks
/// levels 2-4 until the one before them is cleared.
/// </summary>
public class IntervalLevelSelectionUI : MonoBehaviour
{
    [SerializeField] private IntervalsGameData intervalsGameData;

    [Header("High Score Displays (levels 1-4)")]
    [SerializeField] private TextMeshProUGUI level1HighScoreText;
    [SerializeField] private TextMeshProUGUI level2HighScoreText;
    [SerializeField] private TextMeshProUGUI level3HighScoreText;
    [SerializeField] private TextMeshProUGUI level4HighScoreText;

    private void Start()
    {
        // Gating must run even if the data reference is unwired, otherwise
        // every level would stay tappable. It reads the save, not this field.
        if (intervalsGameData != null) intervalsGameData.LoadHighScores();
        else Debug.LogError("IntervalLevelSelectionUI: IntervalsGameData reference is missing!");

        ApplyGating();
    }

    private void ApplyGating()
    {
        var labels = new[] { level1HighScoreText, level2HighScoreText, level3HighScoreText, level4HighScoreText };
        var scores = intervalsGameData != null
            ? new[] { intervalsGameData.level1HighScore, intervalsGameData.level2HighScore,
                      intervalsGameData.level3HighScore, intervalsGameData.level4HighScore }
            : new[] { 0, 0, 0, 0 };

        LevelGating.Apply(LearningMode.Intervals, labels, scores);
    }

    /// <summary>Wired to lv1_btn - lv5_btn, which pass 1-5 (5 = Custom).
    /// NOTE: Intervals and Chords are 1-indexed here, but Notes is 0-indexed
    /// (NotesLevelSelectionUI casts straight to the NoteLevel enum). The scene
    /// button arguments encode this, so do not "normalise" one without the other.</summary>
    public void OnIntervalLevelButtonClicked(int level)
    {
        if (intervalsGameData != null)
            intervalsGameData.SelectIntervalLevel(level);
    }
}
