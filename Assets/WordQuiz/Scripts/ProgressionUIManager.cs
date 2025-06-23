using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class ProgressionUIManager : MonoBehaviour
{
    [Header("References")]
    public ProgressionsGameData gameData;
    public GameSettings gameSettings;
    public ChordButton[] chordButtons = new ChordButton[16];
    public GameObject loadProgEditPanel;
    public GameObject newProgEditPanel;
    public TMP_Dropdown progressionDropdown;
    public Button loadButton;
    public Button newProgButton;
    public Button transposeButton;
    public Button deleteButton;
    public Button saveButton;
    public TMP_InputField progressionNameInput;
    public Button confirmNewProgButton;

    //reference for progression title
    public TMP_Text progressionTitle;

    private void Start()
    {
        // Load saved data from file first
        gameData.LoadFromFile();

        // Set button indices and ensure gameData is assigned to all chord buttons
        for (int i = 0; i < chordButtons.Length; i++)
        {
            chordButtons[i].SetButtonIndex(i);
            // Ensure gameData is assigned to each chord button
            if (chordButtons[i].gameData == null)
            {
                chordButtons[i].gameData = gameData;
            }
        }

        progressionTitle.text = gameData.currentProgression.name;

        // Wait a frame to ensure all components are properly initialized
        StartCoroutine(InitializeAfterDelay());
    }

    private System.Collections.IEnumerator InitializeAfterDelay()
    {
        // Wait for one frame to ensure all components are initialized
        yield return null;

        // Load the current progression from gameData
        if (gameData.currentProgression != null)
        {
            RefreshUI();
        }
        else
        {
            // If no current progression exists, create a new one
            gameData.CreateNewProgression();
            RefreshUI();
        }

        // Initialize UI
        InitializeUI();

        // Subscribe to progression changes
        gameData.onProgressionChanged += OnProgressionChanged;
    }

    private void OnDestroy()
    {
        // Unsubscribe from progression changes
        if (gameData != null)
        {
            gameData.onProgressionChanged -= OnProgressionChanged;
        }
    }

    private void OnProgressionChanged()
    {
        RefreshUI();
    }

    private void InitializeUI()
    {
        // Setup button listeners
        loadButton.onClick.AddListener(OnLoadButtonClicked);
        newProgButton.onClick.AddListener(OnNewProgButtonClicked);
        transposeButton.onClick.AddListener(OnTransposeButtonClicked);
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        confirmNewProgButton.onClick.AddListener(OnConfirmNewProgClicked);

        // Setup dropdown listener
        progressionDropdown.onValueChanged.AddListener((value) => OnProgressionDropdownValueChanged());

        // Initialize progression dropdown
        RefreshProgressionDropdown();

        // Hide panels initially
        loadProgEditPanel.SetActive(false);
        newProgEditPanel.SetActive(false);
    }

    private void RefreshProgressionDropdown()
    {
        progressionDropdown.ClearOptions();
        
        // Add placeholder option
        List<string> options = new List<string>();
        options.Add("Choose a progression");
        
        // Add saved progression names
        foreach (var progression in gameData.savedProgressions)
        {
            options.Add(progression.name);
        }
        
        progressionDropdown.AddOptions(options);
        
        // Reset dropdown value
        progressionDropdown.value = 0;
    }

    private void RefreshUI()
    {
        Debug.Log("=== RefreshUI called ===");
        
        // Reset all buttons
        foreach (var button in chordButtons)
        {
            button.ClearChord();
            button.SetInteractable(false);
        }

        // Update buttons with current progression
        if (gameData.currentProgression != null)        
        {
            progressionTitle.text = gameData.currentProgression.name;
            int actualProgressionSize = gameData.currentProgression.progression.Count;
            Debug.Log($"Current progression has {actualProgressionSize} total elements");
            
            // Set up buttons based on progression size
            for (int i = 0; i < chordButtons.Length; i++)
            {
                if (i < actualProgressionSize)
                {
                    // This button should have a chord
                    var chord = gameData.currentProgression.progression[i];
                    
                    // Only set the chord if it has valid data (not -1)
                    if (chord.note >= 0 && chord.chordtype >= 0)
                    {
                        chordButtons[i].SetChord(chord.note, chord.chordtype);
                        chordButtons[i].SetInteractable(true);
                        Debug.Log($"Button {i}: Set chord {chord.note} {chord.chordtype}");
                    }
                    else
                    {
                        chordButtons[i].ClearChord();
                        chordButtons[i].SetInteractable(true);
                        chordButtons[i].SetChordText("(click to add)");
                        Debug.Log($"Button {i}: Invalid chord data ({chord.note}, {chord.chordtype}), cleared");
                    }
                }
                else if (i == actualProgressionSize)
                {
                    // This button should be empty but interactive
                    chordButtons[i].ClearChord();
                    chordButtons[i].SetInteractable(true);
                    chordButtons[i].SetChordText("(click to add)");
                    Debug.Log($"Button {i}: Empty slot, made interactive and added click to add");
                }
                else if (i > actualProgressionSize)
                {
                    // This button should be empty but interactive
                    chordButtons[i].ClearChord();
                    chordButtons[i].SetInteractable(false);
                    chordButtons[i].SetChordText("");
                    Debug.Log($"Button {i}: Empty slot, made non interactive");
                }
            }
        }
        else
        {
            Debug.Log("No current progression, all buttons cleared");
        }
        
        Debug.Log("=== RefreshUI complete ===");
        // Update button states
        bool hasProgression = gameData.currentProgression != null;
        transposeButton.interactable = hasProgression;
        deleteButton.interactable = hasProgression;
        saveButton.interactable = hasProgression;
    }

    private void OnNewProgButtonClicked()
    {
        // Show the new progression panel
        newProgEditPanel.SetActive(true);
        progressionNameInput.text = ""; // Clear any existing text
    }

    private void OnConfirmNewProgClicked()
    {
        if (string.IsNullOrEmpty(progressionNameInput.text))
        {
            Debug.LogWarning("Please enter a name for the progression");
            return;
        }

        // Create a new empty progression
        gameData.CreateNewProgression();
        gameData.currentProgression.name = progressionNameInput.text;
        
        // Hide the panel
        newProgEditPanel.SetActive(false);

        loadProgEditPanel.SetActive(false);
        
        // Refresh the UI to show empty state (this will clear all buttons)
        RefreshUI();
    }

    private void OnLoadButtonClicked()
    {
        // Show the edit panel
        loadProgEditPanel.SetActive(true);
        
        // Refresh the dropdown to ensure it has the latest data
        RefreshProgressionDropdown();
    }

    // Add this new method to handle dropdown value changes
    public void OnProgressionDropdownValueChanged()
    {
        int selectedIndex = progressionDropdown.value;
        Debug.Log($"Dropdown value changed to: {selectedIndex}");
        
        // Skip the placeholder option (index 0)
        if (selectedIndex > 0)
        {
            // Convert dropdown index to saved progressions index (subtract 1 for placeholder)
            int progressionIndex = selectedIndex - 1;
            Debug.Log($"Loading progression at index: {progressionIndex}");
            
            if (progressionIndex >= 0 && progressionIndex < gameData.savedProgressions.Count)
            {
                var progressionToLoad = gameData.savedProgressions[progressionIndex];
                Debug.Log($"Loading progression: '{progressionToLoad.name}' with {progressionToLoad.progression.Count} chords");
                
                gameData.LoadProgression(progressionIndex);
                RefreshUI();
                loadProgEditPanel.SetActive(false);
                
                Debug.Log($"Successfully loaded progression: {gameData.savedProgressions[progressionIndex].name}");
            }
            else
            {
                Debug.LogError($"Invalid progression index: {progressionIndex}, total progressions: {gameData.savedProgressions.Count}");
            }
        }
        else
        {
            Debug.Log("Placeholder option selected, doing nothing");
        }
    }

    private void OnTransposeButtonClicked()
    {
        gameData.TransposeProgression(1);
        RefreshUI();
    }

    private void OnDeleteButtonClicked()
    {
        if (gameData.currentProgressionIndex >= 0)
        {
            gameData.DeleteCurrentProgression();
            RefreshUI();
            RefreshProgressionDropdown();
        }
    }

    private void OnSaveButtonClicked()
    {
        if (string.IsNullOrEmpty(progressionNameInput.text))
        {
            Debug.LogWarning("Please enter a name for the progression");
            return;
        }

        // Create new progression from current button states
        List<ProgressionsGameData.Pair> progression = new List<ProgressionsGameData.Pair>();
        foreach (var button in chordButtons)
        {
            if (!button.IsEmpty())
            {
                progression.Add(button.GetChordData());
            }
        }

        // Save the progression
        gameData.currentProgression = new ProgressionsGameData.ChordProgression(progression, progressionNameInput.text);
        gameData.SaveCurrentProgression(progressionNameInput.text);
        
        // Refresh UI and dropdown
        RefreshProgressionDropdown();
        progressionNameInput.text = "";
        
        Debug.Log($"Saved progression: {progressionNameInput.text} with {progression.Count} chords");
    }
} 