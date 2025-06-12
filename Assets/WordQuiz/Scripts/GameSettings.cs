using UnityEngine;
using System.Collections.Generic;

public enum LearningMode
{
    Intervals,
    Notes,
    Progressions
}

public enum IntervalLevel
{
    level1,
    level2,
    level3,
    level4,
    CUSTOM
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Game Settings")]
public class GameSettings : ScriptableObject
{
    public static GameSettings instance;

    [Header("Learning Mode")]
    public LearningMode currentLearningMode = LearningMode.Intervals;
    public IntervalLevel currentIntervalLevel = IntervalLevel.CUSTOM;

    [Header("Interval Mode Settings")]
    public List<int> intervalQuestionList = new List<int>();
    public List<int> selectedIntervalStrings = new List<int>();  // Strings used for interval practice
    private int[] allIntervalStrings = new int[] { 0, 1, 2, 3, 4, 5 };  // All available strings for interval practice

    [Header("Interval Level Presets")]
    public static int[] intervalLevel1 = new int[] {0,7};  // Perfect Unison, Perfect Octave
    public static int[] intervalLevel2 = new int[] { 0, 7, 3, 4 };  // Adding Minor/Major Third
    public static int[] intervalLevel3 = new int[] { 0, 7, 3, 4, 1, 2, 10, 11 };  // Adding Minor/Major Second, Minor/Major Seventh
    public static int[] intervalLevel4 = new int[] { 0, 7, 3, 4, 1, 2, 10, 11, 5, 6, 8, 9 };  // All intervals

    [Header("Note Names")]
    public Dictionary<int, string> notename_sharps = new Dictionary<int, string>()
    {
        {0, "A"},
        {1, "A#"},
        {2, "B"},
        {3, "C"},
        {4, "C#"},
        {5, "D"},
        {6, "D#"},
        {7, "E"},
        {8, "F"},
        {9, "F#"},
        {10, "G"},
        {11, "Ab"}
    };

    [Header("Tuning Settings")]
    public int[] standardTuningNotes = new int[] { 7, 2, 10, 5, 0, 7 }; // E, B, G, D, A, E
    public int[] transposedNotes = new int[6]; // Will store the transposition for each string

    // Events
    public delegate void TuningChangedHandler();
    public event TuningChangedHandler OnTuningChanged;

    public delegate void LearningModeChangedHandler(LearningMode newMode);
    public event LearningModeChangedHandler OnLearningModeChanged;

    public delegate void IntervalLevelChangedHandler(IntervalLevel newLevel, List<int> selectedItems, List<int> strings);
    public event IntervalLevelChangedHandler OnIntervalLevelChanged;

    public delegate void IntervalStringSelectionChangedHandler(List<int> strings);
    public event IntervalStringSelectionChangedHandler OnIntervalStringSelectionChanged;

    private void OnEnable()
    {
        // Initialize transposed notes to 0
        for (int i = 0; i < 6; i++)
        {
           // transposedNotes[i] = 0;
        }
        intervalQuestionList.AddRange(intervalLevel4);
    }

    

    public void SetLearningMode(int modeIndex)
    {
        currentLearningMode = (LearningMode)modeIndex;
        OnLearningModeChanged?.Invoke(currentLearningMode);
    }

    public void SelectIntervalLevel(int level)
    {
        intervalQuestionList.Clear();
        selectedIntervalStrings.Clear();
        
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

    public int GetTransposedNote(int stringNumber)
    {
        int transposedValue = (standardTuningNotes[stringNumber] + transposedNotes[stringNumber]) % 12;
        return transposedValue < 0 ? transposedValue + 12 : transposedValue;
    }

    public void TransposeUp(int stringNumber)
    {
        transposedNotes[stringNumber] = (transposedNotes[stringNumber] + 1) % 12;
        OnTuningChanged?.Invoke();
    }

    public void TransposeDown(int stringNumber)
    {
        transposedNotes[stringNumber] = (transposedNotes[stringNumber] - 1 + 12) % 12;
        OnTuningChanged?.Invoke();
    }

    public string GetNoteName(int noteValue)
    {
        return notename_sharps[noteValue];
    }
} 