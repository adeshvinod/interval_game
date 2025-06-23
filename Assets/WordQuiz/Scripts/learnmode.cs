using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class learnmode : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private IntervalsGameData intervalsGameData;
    [SerializeField] private GameObject OptionNotesPanel;
    [SerializeField] private EventManager eventManager;

    [SerializeField] private GameObject omitStringPanel;
    
    public prog_button[] progbuttons_;  // Array of prog buttons
    public prog_button currentRootProgButton;  // Current root prog button
    public List<int> selectedIntervals = new List<int>();  // List of intervals to show
    public List<int> darkenIntervals = new List<int>();  // List of intervals to darken in learn mode

    public int[] rootoptions = new int[] { 3, 10, 17, 24, 31, 38 };
    public int root_options_index = 3;

    // Mapping of string numbers to their corresponding root button numbers
    private readonly Dictionary<int, int> stringToRootButton = new Dictionary<int, int>()
    {
        {0, 3},   // String 0 (High E) -> Button 3
        {1, 10},  // String 1 (B) -> Button 10
        {2, 17},  // String 2 (G) -> Button 17
        {3, 24},  // String 3 (D) -> Button 24
        {4, 31},  // String 4 (A) -> Button 31
        {5, 38}   // String 5 (Low E) -> Button 38
    };

    private Coroutine currentTransitionCoroutine;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (gameSettings == null)
        {
            Debug.LogError("GameSettings reference not assigned in the Inspector for " + gameObject.name);
            return;
        }
        if (intervalsGameData == null)
        {
            Debug.LogError("IntervalsGameData reference not assigned in the Inspector for " + gameObject.name);
            return;
        }
        if (eventManager == null)
        {
            Debug.LogError("EventManager reference not assigned in the Inspector for " + gameObject.name);
            return;
        }
        eventManager.OnIntervalOptionSelected += HandleIntervalOptionSelected;
    }

    private void OnDestroy()
    {
        if (eventManager != null)
        {
            eventManager.OnIntervalOptionSelected -= HandleIntervalOptionSelected;
        }
    }

    private void HandleIntervalOptionSelected(interval_option option)
    {
        SelectedOption_learnmode(option);
    }

    void Start()
    {
        InitializeProgButtons();
        InitializeIntervalSettings();

        if(intervalsGameData.currentIntervalLevel==IntervalLevel.CUSTOM)
        {
           OptionNotesPanel.SetActive(true);
           omitStringPanel.SetActive(true);
           
        }
        else
        {
           OptionNotesPanel.SetActive(false);
           omitStringPanel.SetActive(false);
        }
    }

    private void InitializeProgButtons()
    {
        GameObject progButtonsObject = GameObject.Find("Prog_buttons");
        if (progButtonsObject != null)
        {
            progbuttons_ = progButtonsObject.GetComponentsInChildren<prog_button>();
        }
        else
        {
            Debug.LogError("Prog_buttons GameObject not found in scene");
            progbuttons_ = new prog_button[0];
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (intervalsGameData != null)
        {
            intervalsGameData.OnIntervalLevelChanged += OnIntervalLevelChanged;
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (intervalsGameData != null)
        {
            intervalsGameData.OnIntervalLevelChanged -= OnIntervalLevelChanged;
        }
    }

    void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
    {
        InitializeProgButtons();
        InitializeIntervalSettings();
    }

    private void OnIntervalLevelChanged(IntervalLevel newLevel, List<int> intervalQuestions, List<int> strings)
    {
        InitializeIntervalSettings();
    }

    private void InitializeIntervalSettings()
    {
        if (progbuttons_ == null || progbuttons_.Length == 0)
        {
            Debug.LogError("Prog buttons not initialized properly");
            return;
        }

        SetSelectedIntervals(intervalsGameData.intervalQuestionList);
        
        switch(intervalsGameData.currentIntervalLevel)
        {
            case IntervalLevel.level1:
                darkenIntervals = new List<int> { };
                break;
            case IntervalLevel.level2:
                darkenIntervals = IntervalsGameData.intervalLevel1.ToList();
                break;
            case IntervalLevel.level3:
                darkenIntervals = IntervalsGameData.intervalLevel2.ToList();
                break;
            case IntervalLevel.level4:  
                darkenIntervals = IntervalsGameData.intervalLevel3.ToList();
                break;
            case IntervalLevel.CUSTOM:
                darkenIntervals = new List<int> { };
                break;
        }
              
        // Use filtered root options for initial root button
        int[] filteredOptions = GetFilteredRootOptions();
        if (filteredOptions.Length > 0)
        {
            // Find a valid root button from filtered options
            int validRootButton = filteredOptions[0]; // Start with first available option
            setrootbutton(validRootButton);
        }
        else
        {
            // Fallback to original method if no filtered options available
            setrootbutton(rootoptions[root_options_index]);
        }
    }

    void setrootbutton(int siblingindex_root, bool Wait=true, bool scalingAnimation=true)
    {
        if (progbuttons_ == null || progbuttons_.Length == 0)
        {
            Debug.LogError("Prog buttons array is null or empty");
            return;
        }

        if (siblingindex_root < 0 || siblingindex_root >= progbuttons_.Length)
        {
            Debug.LogError($"Invalid sibling index: {siblingindex_root}. Array length is {progbuttons_.Length}");
            return;
        }

        // Stop any existing transition
        if (currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine);
        }

        currentRootProgButton = progbuttons_[siblingindex_root];
        
        // Reset all buttons first
        foreach (prog_button progButton in progbuttons_)
        {
            if (progButton != null)
            {
                progButton.SetActiveCircle(0, scalingAnimation);
            }
        }

        // Immediately set the root button
        currentRootProgButton.SetActiveCircle(3, scalingAnimation);
        currentRootProgButton.text.text = "R";

        currentTransitionCoroutine = StartCoroutine(DelayedSetOtherButtons(siblingindex_root, Wait, scalingAnimation));
    }

    private IEnumerator DelayedSetOtherButtons(int siblingindex_root, bool Wait, bool scalingAnimation=true)
    {
        if (progbuttons_ == null || progbuttons_.Length == 0)
        {
            Debug.LogError("Prog buttons array is null or empty in DelayedSetOtherButtons");
            yield break;
        }

        isTransitioning = true;
        
        // Wait for 1 second
        if(Wait==true)
            yield return new WaitForSeconds(1f);

        foreach (prog_button progButton in progbuttons_)
        {
            if (progButton != null && progButton != currentRootProgButton)
            {
                int interval;
                if (progButton.notevalue >= currentRootProgButton.notevalue)
                    interval = progButton.notevalue - currentRootProgButton.notevalue;
                else
                    interval = 12 - currentRootProgButton.notevalue + progButton.notevalue;

                if (selectedIntervals.Contains(interval))
                {
                    progButton.SetActiveCircle(1, scalingAnimation);
                    progButton.text.text = progButton.intervalname[interval];
                }

                if (darkenIntervals.Contains(interval))
                {
                    progButton.SetActiveCircle(2, scalingAnimation);
                    progButton.text.text = progButton.intervalname[interval];
                }
            }
        }
        
        isTransitioning = false;
        currentTransitionCoroutine = null;
    }

    public void shiftroot(int direction)
    {
        int[] filteredOptions = GetFilteredRootOptions();
        
        if (filteredOptions.Length == 0)
        {
            Debug.LogWarning("No root options available - all strings may be omitted");
            return;
        }
        
        int currentFilteredIndex = GetCurrentFilteredIndex();
        
        if(direction == 1)
        {
            currentFilteredIndex = (currentFilteredIndex + 1) % filteredOptions.Length;
            Debug.Log($"Shifted up to filtered index {currentFilteredIndex}");
        }
        else if(direction == -1)
        {
            if (currentFilteredIndex > 0)
                currentFilteredIndex = currentFilteredIndex - 1;
            else
                currentFilteredIndex = filteredOptions.Length - 1;
            Debug.Log($"Shifted down to filtered index {currentFilteredIndex}");
        }
        
        // Update the root_options_index to match the selected filtered option
        int selectedButton = filteredOptions[currentFilteredIndex];
        for (int i = 0; i < rootoptions.Length; i++)
        {
            if (rootoptions[i] == selectedButton)
            {
                root_options_index = i;
                break;
            }
        }
        
        setrootbutton(selectedButton);
    }

    public void SetSelectedIntervals(List<int> intervals)
    {
        selectedIntervals = new List<int>(intervals);
        if (currentRootProgButton != null)
        {
            setrootbutton(rootoptions[root_options_index]);
        }
    }

    public void SelectedOption_learnmode(interval_option value)
    {
        if(value.isSelected==true)
        {
            selectedIntervals.Add(value.intervalValue);
        }
        else
        {
            selectedIntervals.Remove(value.intervalValue);
        }

        // Update GameSettings with the current selected intervals
        if (intervalsGameData.currentIntervalLevel == IntervalLevel.CUSTOM)
        {
            intervalsGameData.intervalQuestionList.Clear();
            intervalsGameData.intervalQuestionList.AddRange(selectedIntervals);
            Debug.Log($"Updated GameSettings with custom intervals: {string.Join(", ", selectedIntervals)}");
        }

        setrootbutton(rootoptions[root_options_index], false, false);
    }

    // Get filtered root options based on selected strings
    private int[] GetFilteredRootOptions()
    {
        if (intervalsGameData == null || intervalsGameData.selectedIntervalStrings == null)
        {
            return rootoptions; // Return all options if no filtering data available
        }

        List<int> filteredOptions = new List<int>();
        
        foreach (int stringNum in intervalsGameData.selectedIntervalStrings)
        {
            if (stringToRootButton.TryGetValue(stringNum, out int buttonNumber))
            {
                filteredOptions.Add(buttonNumber);
            }
        }
        
        Debug.Log($"Filtered root options: {string.Join(", ", filteredOptions)} (from selected strings: {string.Join(", ", intervalsGameData.selectedIntervalStrings)})");
        return filteredOptions.ToArray();
    }

    // Get current filtered root options index
    private int GetCurrentFilteredIndex()
    {
        int[] filteredOptions = GetFilteredRootOptions();
        int currentButton = rootoptions[root_options_index];
        
        for (int i = 0; i < filteredOptions.Length; i++)
        {
            if (filteredOptions[i] == currentButton)
            {
                return i;
            }
        }
        
        // If current button is not in filtered options, return 0
        return 0;
    }

    void Update()
    {
        
    }
}
