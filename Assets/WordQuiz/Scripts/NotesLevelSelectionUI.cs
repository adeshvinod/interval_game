using UnityEngine;
using TMPro;

/// <summary>
/// Drives the notes_levels scene: shows each level's high score and locks
/// levels 2-4 until the one before them is cleared.
/// </summary>
public class NotesLevelSelectionUI : MonoBehaviour
{
    [SerializeField] private NotesGameData gameData;
    [SerializeField] private EventManager eventManager;
    [SerializeField] private GameSettings gameSettings;

    [Header("High Score Displays (levels 1-4)")]
    [SerializeField] private TextMeshProUGUI level1HighScoreText;
    [SerializeField] private TextMeshProUGUI level2HighScoreText;
    [SerializeField] private TextMeshProUGUI level3HighScoreText;
    [SerializeField] private TextMeshProUGUI level4HighScoreText;

    private void Start()
    {
        // Gating must run even if a reference below is unwired, otherwise every
        // level would stay tappable. It reads the save, not these fields.
        if (gameData != null) gameData.LoadHighScores();
        ApplyGating();

        if (gameData == null)     Debug.LogError("NotesLevelSelectionUI: NotesGameData reference is missing!");
        if (eventManager == null) Debug.LogError("NotesLevelSelectionUI: EventManager reference is missing!");
    }

    private void ApplyGating()
    {
        var labels = new[] { level1HighScoreText, level2HighScoreText, level3HighScoreText, level4HighScoreText };
        var scores = gameData != null
            ? new[] { gameData.level1HighScore, gameData.level2HighScore, gameData.level3HighScore, gameData.level4HighScore }
            : new[] { 0, 0, 0, 0 };

        LevelGating.Apply(LearningMode.Notes, labels, scores);
    }

    /// <summary>Wired to lv1_btn - lv5_btn, which pass 0-4 (4 = Custom) because
    /// SelectLevel casts straight to the NoteLevel enum. Intervals and Chords are
    /// 1-indexed instead — see IntervalLevelSelectionUI.</summary>
    public void OnLevelButtonClicked(int level)
    {
        if (gameData == null || eventManager == null) return;

        gameData.SelectLevel(level);
        eventManager.SelectNoteLevel(level);
    }
}
