using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class ChordButton : MonoBehaviour
{
    [Header("UI References")]
    public Button button;
    public TMP_Text chordText;
    public GameObject editPanel;
    public TMP_Dropdown noteDropdown;
    public TMP_Dropdown chordTypeDropdown;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Data")]
    public ProgressionsGameData gameData;

    private int buttonIndex;
    private bool dropdownsInitialized = false;

    // Use -1 to represent no chord instead of boolean
    private int currentNote = -1;
    private int currentChordType = -1;

    private ProgressionUIManager uiManager;

    private void Awake()
    {
        // Get components
        button = GetComponent<Button>();
        chordText = GetComponentInChildren<TMP_Text>();

        // Get references
        uiManager = FindObjectOfType<ProgressionUIManager>();

        // Setup button listeners
        button.onClick.AddListener(OnButtonClicked);
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);

        // Hide edit panel initially
        editPanel.SetActive(false);
    }

    private void Start()
    {
        // Initialize dropdowns after gameData is set
        if (gameData != null && !dropdownsInitialized)
        {
            InitializeDropdowns();
            dropdownsInitialized = true;
        }
    }

    public void SetButtonIndex(int index)
    {
        buttonIndex = index;
    }

    private void InitializeDropdowns()
    {
        if (gameData == null)
        {
            Debug.LogWarning("gameData is null in ChordButton. Cannot initialize dropdowns.");
            return;
        }

        // Initialize note dropdown
        noteDropdown.ClearOptions();
        List<string> noteOptions = new List<string> { "A","A#","B","C","C#","D","D#","E","F","F#","G","G#"};
        noteDropdown.AddOptions(noteOptions);

        // Initialize chord type dropdown
        chordTypeDropdown.ClearOptions();
        List<string> chordTypeOptions = new List<string>();
        foreach (var chord in gameData.chordNameList)
        {
            chordTypeOptions.Add(chord.Value);
        }
        chordTypeDropdown.AddOptions(chordTypeOptions);
    }

    private void OnButtonClicked()
    {
        // Ensure dropdowns are initialized before showing edit panel
        if (!dropdownsInitialized && gameData != null)
        {
            InitializeDropdowns();
            dropdownsInitialized = true;
        }
        
        editPanel.SetActive(true);
    }

    private void OnConfirmClicked()
    {
        if (gameData == null)
        {
            Debug.LogError("gameData is null. Cannot save chord.");
            return;
        }

        // Update current values from dropdowns
        currentNote = noteDropdown.value;
        currentChordType = chordTypeDropdown.value;
        UpdateDisplay();

        // Update the current progression
        if (gameData.currentProgression == null)
        {
            gameData.CreateNewProgression();
        }

        // Add or update the chord at this position
        gameData.AddChordToCurrentProgression(currentNote, currentChordType, buttonIndex);

        editPanel.SetActive(false);
    }

    private void OnCancelClicked()
    {
        editPanel.SetActive(false);
    }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }

    public void SetChordText(string text)
    {
        chordText.text = text;
    }

    public void SetChord(int noteIndex, int chordTypeIndex)
    {
        Debug.Log($"ChordButton {buttonIndex}: SetChord called with note={noteIndex}, chordType={chordTypeIndex}");
        
        // Ensure dropdowns are initialized
        if (!dropdownsInitialized && gameData != null)
        {
            Debug.Log($"ChordButton {buttonIndex}: Initializing dropdowns");
            InitializeDropdowns();
            dropdownsInitialized = true;
        }

        // Only proceed if dropdowns are ready
        if (!dropdownsInitialized || noteDropdown.options.Count == 0 || chordTypeDropdown.options.Count == 0)
        {
            Debug.LogWarning($"ChordButton {buttonIndex}: Dropdowns not ready, cannot set chord");
            return;
        }

        // Validate indices
        if (noteIndex >= 0 && noteIndex < noteDropdown.options.Count &&
            chordTypeIndex >= 0 && chordTypeIndex < chordTypeDropdown.options.Count)
        {
            Debug.Log($"ChordButton {buttonIndex}: Setting dropdown values - note={noteIndex}, chordType={chordTypeIndex}");
            
            // Update current values
            currentNote = noteIndex;
            currentChordType = chordTypeIndex;
            
            // Update dropdowns
            noteDropdown.value = noteIndex;
            chordTypeDropdown.value = chordTypeIndex;
            
            UpdateDisplay();
            Debug.Log($"ChordButton {buttonIndex}: Chord set successfully");
        }
        else
        {
            Debug.LogWarning($"ChordButton {buttonIndex}: Invalid chord indices: note={noteIndex}, chordType={chordTypeIndex}");
            ClearChord();
        }
    }

    public void ClearChord()
    {
        // Set to -1 to indicate no chord
        currentNote = -1;
        currentChordType = -1;
        SetChordText("");
        
        // Don't reset dropdown values when clearing - this was causing empty slots to show as "A maj"
        // The dropdowns should keep their current values, only the display should change
    }

    public bool IsEmpty()
    {
        return currentNote == -1 || currentChordType == -1;
    }

    public ProgressionsGameData.Pair GetChordData()
    {
        // Return current stored values instead of dropdown values
        return new ProgressionsGameData.Pair { note = currentNote, chordtype = currentChordType };
    }

    private void UpdateDisplay()
    {
        if (IsEmpty())
        {
            SetChordText("");
        }
        else
        {
            if (gameData != null && gameData.chordNameList.ContainsKey(currentChordType))
            {
                string noteName = noteDropdown.options[currentNote].text;
                string chordType = gameData.chordNameList[currentChordType];
                SetChordText($"{noteName} {chordType}");
            }
            else
            {
                SetChordText("Invalid");
            }
        }
    }
} 