using UnityEngine;
using System;
using System.Collections.Generic;

public enum IntervalLevel
{
    level1,
    level2,
    level3,
    level4,
    CUSTOM
}

[CreateAssetMenu(fileName = "IntervalsGameData", menuName = "Game/Intervals Game Data")]
public class IntervalsGameData : ScriptableObject
{
    [Header("Interval Level Settings")]
    public IntervalLevel currentIntervalLevel = IntervalLevel.CUSTOM;
    public List<int> intervalQuestionList = new List<int>();
    public List<int> selectedIntervalStrings = new List<int>();  // Strings used for interval practice
    private int[] allIntervalStrings = new int[] { 0, 1, 2, 3, 4, 5 };  // All available strings for interval practice

    [Header("Interval Level Presets")]
    public static int[] intervalLevel1 = new int[] {0,7};  // Perfect Unison, Perfect Octave
    public static int[] intervalLevel2 = new int[] { 0, 7, 3, 4 };  // Adding Minor/Major Third
    public static int[] intervalLevel3 = new int[] { 0, 7, 3, 4, 1, 2, 10, 11 };  // Adding Minor/Major Second, Minor/Major Seventh
    public static int[] intervalLevel4 = new int[] { 0, 7, 3, 4, 1, 2, 10, 11, 5, 6, 8, 9 };  // All intervals

    // Events
    public delegate void IntervalLevelChangedHandler(IntervalLevel newLevel, List<int> selectedItems, List<int> strings);
    public event IntervalLevelChangedHandler OnIntervalLevelChanged;

    public delegate void IntervalStringSelectionChangedHandler(List<int> strings);
    public event IntervalStringSelectionChangedHandler OnIntervalStringSelectionChanged;

    [Header("Score Data")]
    public int score = 0;
    public int highScore = 0;
    public int lives = 3;
    public float timer = 10f;

    [Header("Level High Scores")]
    public int level1HighScore = 0;
    public int level2HighScore = 0;
    public int level3HighScore = 0;
    public int level4HighScore = 0;

    [Header("Question History")]
    public float[] accuracies = new float[12];
    public float[] reactiontimes = new float[12];
    public int[] questioncounter = new int[12];

    [Header("Detailed History")]
    public float[,] questionHistory_accuracy = new float[6, 42];
    public float[,] questionHistory_rxntimes = new float[6, 42];
    public float[,] questionHistory_Counter = new float[6, 42];

    [Header("Current Question Data")]
    public int currentQuestion_Question_node;
    public int currentQuestion_Answer_node;
    public int intervalquestion_val;

    private void OnEnable()
    {
        intervalQuestionList.AddRange(intervalLevel4);
    }

    public void SelectIntervalLevel(int level)
    {
        intervalQuestionList.Clear();
        selectedIntervalStrings.Clear();
        Debug.Log("SelectIntervalLevel: "+level);
        
        switch (level)
        {
            case 1:
                currentIntervalLevel = IntervalLevel.level1;
                intervalQuestionList.AddRange(intervalLevel1);
                selectedIntervalStrings.AddRange(allIntervalStrings);
                break;

            case 2:
                currentIntervalLevel = IntervalLevel.level2;
                intervalQuestionList.AddRange(intervalLevel2);
                selectedIntervalStrings.AddRange(allIntervalStrings);
                break;

            case 3:
                currentIntervalLevel = IntervalLevel.level3;
                intervalQuestionList.AddRange(intervalLevel3);
                selectedIntervalStrings.AddRange(allIntervalStrings);
                break;

            case 4:
                currentIntervalLevel = IntervalLevel.level4;
                intervalQuestionList.AddRange(intervalLevel4);
                selectedIntervalStrings.AddRange(allIntervalStrings);
                break;

            case 5:
                currentIntervalLevel = IntervalLevel.CUSTOM;
                intervalQuestionList.AddRange(new List<int>());
                selectedIntervalStrings.AddRange(allIntervalStrings);
                break;
        }

        OnIntervalLevelChanged?.Invoke(currentIntervalLevel, intervalQuestionList, selectedIntervalStrings);
        Debug.Log($"Interval level selected: {currentIntervalLevel} intervalQuestions: {ListToText(intervalQuestionList)}");
    }

    public void ToggleIntervalString(int stringNum)
    {
        if(currentIntervalLevel!=IntervalLevel.CUSTOM)
        {
            return;
        }//we dont want to toggle strings in other levels

        if (selectedIntervalStrings.Contains(stringNum))
        {
            selectedIntervalStrings.Remove(stringNum);
        }
        else
        {
            selectedIntervalStrings.Add(stringNum);
        }
        OnIntervalStringSelectionChanged?.Invoke(selectedIntervalStrings);
        Debug.Log("Interval string list updated: " + ListToText(selectedIntervalStrings));
    }

    private string ListToText(List<int> list)
    {
        string result = "";
        foreach (var listMember in list)
        {
            result += listMember.ToString() + " ";
        }
        return result;
    }

    public void ResetGameData()
    {
        score = 0;
        lives = 3;
        timer = 10f;

        // Reset arrays
        for (int i = 0; i < 12; i++)
        {
            accuracies[i] = 0;
            reactiontimes[i] = 0;
            questioncounter[i] = 0;
        }

        // Reset 2D arrays
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 42; j++)
            {
                questionHistory_accuracy[i, j] = -1;
                questionHistory_rxntimes[i, j] = -1;
                questionHistory_Counter[i, j] = 0;
            }
        }

        // Reset current question data
        currentQuestion_Question_node = 0;
        currentQuestion_Answer_node = 0;
        intervalquestion_val = 0;
    }

    public void UpdateHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
        }
    }

    public void LoadHighScores()
    {
        
            savedData data = SaveSystem.Loaddata();
            if (data != null)
            {
                level1HighScore = data.l1_intervals_highscore;
                level2HighScore = data.l2_intervals_highscore;
                level3HighScore = data.l3_intervals_highscore;
                level4HighScore = data.l4_intervals_highscore;
            }
        
    }
} 