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
    [SerializeField] private EventManager eventManager;
    public prog_button[] progbuttons_;  // Array of prog buttons
    public prog_button currentRootProgButton;  // Current root prog button
    public List<int> selectedIntervals = new List<int>();  // List of intervals to show
    public List<int> darkenIntervals = new List<int>();  // List of intervals to darken in learn mode

    public int[] rootoptions = new int[] { 3, 10, 17, 24, 31, 38 };
    public int root_options_index = 3;

    private Coroutine currentTransitionCoroutine;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (gameSettings == null)
        {
            Debug.LogError("GameSettings reference not assigned in the Inspector for " + gameObject.name);
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
        if (gameSettings != null)
        {
            gameSettings.OnIntervalLevelChanged += OnIntervalLevelChanged;
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (gameSettings != null)
        {
            gameSettings.OnIntervalLevelChanged -= OnIntervalLevelChanged;
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

        SetSelectedIntervals(gameSettings.intervalQuestionList);
        
        switch(gameSettings.currentIntervalLevel)
        {
            case IntervalLevel.level1:
                darkenIntervals = new List<int> { };
                break;
            case IntervalLevel.level2:
                darkenIntervals = GameSettings.intervalLevel1.ToList();
                break;
            case IntervalLevel.level3:
                darkenIntervals = GameSettings.intervalLevel2.ToList();
                break;
            case IntervalLevel.level4:  
                darkenIntervals = GameSettings.intervalLevel3.ToList();
                break;
            case IntervalLevel.CUSTOM:
                darkenIntervals = new List<int> { };
                break;
        }
              
        setrootbutton(rootoptions[root_options_index]);
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
        if(direction==1)
        {
            root_options_index = (root_options_index + 1)%6;
            Debug.Log("Shifted up");
        }
        else if(direction==-1)
        {
            if (root_options_index > 0)
                root_options_index = root_options_index - 1;
            else
                root_options_index = 5;
        }
        setrootbutton(rootoptions[root_options_index]);
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
        setrootbutton(rootoptions[root_options_index], false, false);
    }

    void Update()
    {
        
    }
}
