using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class IntervalLevelSelectionUI : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private IntervalsGameData intervalsGameData;
    [SerializeField] private Text intervalQuestionText;  // Shows selected intervals
    [SerializeField] private Text intervalStringText;  // Shows selected strings for interval practice
    
    [Header("High Score Displays")]
    [SerializeField] private TextMeshProUGUI level1HighScoreText;
    [SerializeField] private TextMeshProUGUI level2HighScoreText;
    [SerializeField] private TextMeshProUGUI level3HighScoreText;
    [SerializeField] private TextMeshProUGUI level4HighScoreText;
    [SerializeField] private IntervalsGameData gameData;

    private void Start()
    {
        if (gameData != null)
        {
            gameData.LoadHighScores();
            UpdateHighScoreDisplays();
        }
    }

    private void UpdateHighScoreDisplays()
    {
        if (level1HighScoreText != null) level1HighScoreText.text = gameData.level1HighScore.ToString();
        if (level2HighScoreText != null) level2HighScoreText.text = gameData.level2HighScore.ToString();
        if (level3HighScoreText != null) level3HighScoreText.text = gameData.level3HighScore.ToString();
        if (level4HighScoreText != null) level4HighScoreText.text = gameData.level4HighScore.ToString();
    }

    private void OnEnable()
    {
        if (intervalsGameData != null)
        {
           // intervalsGameData.OnIntervalLevelChanged += UpdateIntervalUI;
          //  intervalsGameData.OnIntervalStringSelectionChanged += UpdateIntervalStringList;
          //  gameSettings.OnLearningModeChanged += OnLearningModeChanged;
        }
    }

    private void OnDisable()
    {
        if (intervalsGameData != null)
        {
          //  intervalsGameData.OnIntervalLevelChanged -= UpdateIntervalUI;
          //  intervalsGameData.OnIntervalStringSelectionChanged -= UpdateIntervalStringList;
          //  gameSettings.OnLearningModeChanged -= OnLearningModeChanged;
        }
    }
   

    private string ListToText(System.Collections.Generic.List<int> list)
    {
        string result = "";
        foreach (var listMember in list)
        {
            result += listMember.ToString() + " ";
        }
        return result;
    }

    public void OnIntervalLevelButtonClicked(int level)
    {
        if (intervalsGameData != null)
        {
            intervalsGameData.SelectIntervalLevel(level);
        }
    }

    public void OnIntervalStringButtonClicked(int stringNum)
    {
        if (intervalsGameData != null)
        {
            intervalsGameData.ToggleIntervalString(stringNum);
        }
    }
} 