using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the chords_levels scene.
/// Wire level buttons to OnChordLevelButtonClicked(1-4 or 5 for custom).
/// </summary>
public class ChordsLevelSelectionUI : MonoBehaviour
{
    [SerializeField] private ChordsGameData chordsGameData;
    [SerializeField] private GameSettings   gameSettings;

    [Header("High Score Text Fields")]
    [SerializeField] private TextMeshProUGUI level1HighScoreText;
    [SerializeField] private TextMeshProUGUI level2HighScoreText;
    [SerializeField] private TextMeshProUGUI level3HighScoreText;
    [SerializeField] private TextMeshProUGUI level4HighScoreText;

    private void Start()
    {
        // Gating must run even if the data reference is unwired, otherwise
        // every level would stay tappable. It reads the save, not this field.
        if (chordsGameData != null) chordsGameData.LoadHighScores();
        else Debug.LogError("ChordsLevelSelectionUI: ChordsGameData reference is missing!");

        ApplyGating();
    }

    private void ApplyGating()
    {
        var labels = new[] { level1HighScoreText, level2HighScoreText, level3HighScoreText, level4HighScoreText };
        var scores = chordsGameData != null
            ? new[] { chordsGameData.level1HighScore, chordsGameData.level2HighScore,
                      chordsGameData.level3HighScore, chordsGameData.level4HighScore }
            : new[] { 0, 0, 0, 0 };

        LevelGating.Apply(LearningMode.Chords, labels, scores);
    }

    /// <summary>Wire this to each level button (pass 1-4, or 5 for Custom).</summary>
    public void OnChordLevelButtonClicked(int level)
    {
        if (chordsGameData != null)
            chordsGameData.SelectChordLevel(level);

        // Tell prog_button to initialise in Chords mode (stride 5)
        if (gameSettings != null)
            gameSettings.SetLearningMode((int)LearningMode.Chords);

        SceneManager.LoadScene("learnModeChords");
    }
}
