using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IntervalLevelSelectionUI : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private Text intervalQuestionText;  // Shows selected intervals
    [SerializeField] private Text intervalStringText;  // Shows selected strings for interval practice

    private void OnEnable()
    {
        if (gameSettings != null)
        {
           // gameSettings.OnIntervalLevelChanged += UpdateIntervalUI;
          //  gameSettings.OnIntervalStringSelectionChanged += UpdateIntervalStringList;
          //  gameSettings.OnLearningModeChanged += OnLearningModeChanged;
        }
    }

    private void OnDisable()
    {
        if (gameSettings != null)
        {
          //  gameSettings.OnIntervalLevelChanged -= UpdateIntervalUI;
          //  gameSettings.OnIntervalStringSelectionChanged -= UpdateIntervalStringList;
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
        if (gameSettings != null)
        {
            gameSettings.SelectIntervalLevel(level);
        }
    }

    public void OnIntervalStringButtonClicked(int stringNum)
    {
        if (gameSettings != null)
        {
            gameSettings.ToggleIntervalString(stringNum);
        }
    }
} 