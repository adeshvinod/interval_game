using UnityEngine;
using UnityEngine.SceneManagement;
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
        if (chordsGameData != null)
        {
            chordsGameData.LoadHighScores();
            RefreshHighScores();
        }
    }

    private void RefreshHighScores()
    {
        if (level1HighScoreText != null) level1HighScoreText.text = chordsGameData.level1HighScore.ToString();
        if (level2HighScoreText != null) level2HighScoreText.text = chordsGameData.level2HighScore.ToString();
        if (level3HighScoreText != null) level3HighScoreText.text = chordsGameData.level3HighScore.ToString();
        if (level4HighScoreText != null) level4HighScoreText.text = chordsGameData.level4HighScore.ToString();
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
