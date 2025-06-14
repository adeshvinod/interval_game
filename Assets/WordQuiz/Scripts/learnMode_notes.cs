using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class learnMode_notes : MonoBehaviour
{
    [SerializeField] private NotesGameData gameData;
    [SerializeField] private EventManager eventManager;
    [SerializeField] private GameSettings gameSettings;
    private prog_button[] progbuttons_;

    private Dictionary<int, string> notename_sharps = new Dictionary<int, string>()
    {
        {0,"A" },
        {1,"A#" },
        {2,"B" },
        {3,"C" },
        {4,"C#" },
        {5,"D" },
        {6,"D#" },
        {7,"E" },
        {8,"F" },
        {9,"F#" },
        {10,"G" },
        {11,"G#" }
    };

    private void Awake()
    {
        if (gameData == null)
        {
            Debug.LogError("NotesGameData reference is missing!");
        }

        if (eventManager == null)
        {
            Debug.LogError("EventManager reference is missing!");
        }
    }

    private void OnEnable()
    {
        if (gameData != null)
        {
            gameData.OnLevelChanged += HandleLevelChanged;
        }
    }

    private void OnDisable()
    {
        if (gameData != null)
        {
            gameData.OnLevelChanged -= HandleLevelChanged;
        }
    }

    private void HandleLevelChanged(NotesGameData.NoteLevel newLevel)
    {
        Debug.Log($"Level changed to: {newLevel}");
        RevealSelectedButtons();
    }

    void Start()
    {
        InitializeProgButtons();
        RevealSelectedButtons();
        gameData.ResetGameData();
    }

    private void InitializeProgButtons()
    {
        GameObject progButtonsObject = GameObject.Find("Prog_buttons");
        if (progButtonsObject != null)
        {
            progbuttons_ = progButtonsObject.GetComponentsInChildren<prog_button>();
            Debug.Log($"Found {progbuttons_.Length} prog buttons");
        }
        else
        {
            Debug.LogWarning("Prog_buttons GameObject not found in scene");
        }
    }

    private void RevealSelectedButtons()
    {
        if (progbuttons_ == null || progbuttons_.Length == 0)
        {
            Debug.LogError("Prog buttons not initialized properly");
            return;
        }

        // First reset all buttons to transparent state
        foreach (prog_button button in progbuttons_)
        {
            if (button != null)
            {
                button.SetActiveCircle(0);
                button.text.text = "";
                button.selectedRegion = false;
            }
        }

        // Then reveal the buttons that are in the question list
        foreach ((int, int) question in gameData.questionList)
        {
            if (gameData.Coordinate_system.TryGetValue(question, out int buttonIndex))
            {
                if (buttonIndex >= 0 && buttonIndex < progbuttons_.Length)
                {
                    prog_button button = progbuttons_[buttonIndex];
                    button.selectedRegion = true;
                    button.SetActiveCircle(1);
                    button.text.text = notename_sharps[button.notevalue];

                    // Enable the sprite mask if it exists
                    Transform spriteMask = button.transform.Find("Sprite Mask");
                    if (spriteMask != null)
                    {
                        spriteMask.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
