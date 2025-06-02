using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class prog_button : MonoBehaviour
{
    private Button button;
    
    [SerializeField]
    private List<GameObject> circleImages = new List<GameObject>();
    public int circleIndex;

    [SerializeField]
    private float scaleDuration = 0.2f; // Duration of the scale animation
    private Vector3 originalScale = Vector3.zero;
    private Vector3 targetScale = Vector3.one ; // Twice the original size
    private Coroutine scaleCoroutine;
    public TMPro.TextMeshProUGUI text;
    private int stringnum;

    public int notevalue;
    string notename_sharp;
    string notename_flat;
    public bool selectedRegion = false; //to check if the button is in the user selected region of the fretboard defined in settings 
    public bool isSelected = false;  // ded for intervals mode
    public int isroot = 0;  // Added for intervals mode
    // Start is called before the first frame update

    public int x_coord;
    public int y_coord; 
    public int buttonNumber;    
    public int fretnum;  // Added for intervals mode

    Dictionary<int, int> notevalue_dict = new Dictionary<int, int>()
    {
        {0,7},
        {1,8},
        {2,9},
        {3,10},
        {4,11},
        {5,0},
        {6,1},
        {7,2},
        {8,3},
        {9,4},
        {10,5},
        {11,6},
        {12,7},

        {13,2},
        {14,3},
        {15,4},
        {16,5},
        {17,6},
        {18,7},
        {19,8},
        {20,9},
        {21,10},
        {22,11},
        {23,0},
        {24,1},
        {25,2},

        {26,10},
        {27,11},
        {28,0},
        {29,1},
        {30,2},
        {31,3},
        {32,4},
        {33,5},
        {34,6},
        {35,7},
        {36,8},
        {37,9},
        {38,10},

         {39,5},
        {40,6},
        {41,7},
        {42,8},
        {43,9},
        {44,10},
        {45,11},
        {46,0},
        {47,1},
        {48,2},
        {49,3},
        {50,4},
        {51,5},

        {52,0},
        {53,1},
        {54,2},
        {55,3},
        {56,4},
        {57,5},
        {58,6},
        {59,7},
        {60,8},
        {61,9},
        {62,10},
        {63,11},
        {64,0},

        {65,7},
        {66,8},
        {67,9},
        {68,10},
        {69,11},
        {70,0},
        {71,1},
        {72,2},
        {73,3},
        {74,4},
        {75,5},
        {76,6},
        {77,7}
    };

    Dictionary<int, int> interval_notevalue_dict = new Dictionary<int, int>()
    {
        {0, 0}, {1, 1}, {2, 2}, {3, 3}, {4, 4}, {5, 5}, {6, 6},
        {7, 7}, {8, 8}, {9, 9}, {10, 10}, {11, 11}, {12, 0}, {13, 1},
        {14, 3}, {15, 4}, {16, 5}, {17, 6}, {18, 7}, {19, 8}, {20, 9},
        {21, 10}, {22, 11}, {23, 0}, {24, 1}, {25, 2}, {26, 3}, {27, 4},
        {28, 5}, {29, 6}, {30, 7}, {31, 8}, {32, 9}, {33, 10}, {34, 11},
        {35, 0}, {36, 1}, {37, 2}, {38, 3}, {39, 4}, {40, 5}, {41, 6}
    };

    Dictionary<int, string> notename_sharps = new Dictionary<int, string>()
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

    Dictionary<int, string> notename_flats = new Dictionary<int, string>()
     {
         {0,"A" },
         {1,"Bb" },
         {2,"B" },
         {3,"C" },
         {4,"Db" },
         {5,"D" },
         {6,"Eb" },
         {7,"E" },
         {8,"F" },
         {9,"Gb" },
         {10,"G" },
         {11,"Ab" }


     };

    

    Dictionary<int, int> transposed_notes_dict = new Dictionary<int, int>() //for alternate tunings
           {
            {0, 0},
            {1, 0},
            {2, 0},
            {3, 0},
            {4, 0},
            {5, 0}

           };  
    
    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Get the Button component
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Button component not found on " + gameObject.name);
            return;
        }

        // Validate circle images
        if (circleImages.Count == 0)
        {
            Debug.LogError("No circle images assigned in the Inspector for " + gameObject.name);
        }

        // Add click event listener that will handle different modes
        button.onClick.AddListener(HandleButtonClick);

        // Initially set all circles to inactive
        foreach (GameObject circle in circleImages)
        {
            if (circle != null)
            {
                circle.SetActive(false);
            }
        }

        // Set text to transparent initially
        if (text != null)
        {
            Color textColor = text.color;
            textColor.a = 0f;
            text.color = textColor;
            text.alpha = 0f;
        }

        // Get transposed notes dictionary - this is universal for all modes
        if (global_settings.instance.transposed_notes_dict != null)
        {
            transposed_notes_dict = global_settings.instance.transposed_notes_dict;
          // Debug.Log("transposed_notes_dict found from global settings singleton");
        }
        else
        {
            Debug.Log("transposed_notes_dict is null");
        }

        // Extract button number from GameObject name
        string buttonName = this.gameObject.name;
        
        if (buttonName == "Prog_button")
        {
            buttonNumber = 0;
        }
        else
        {
            // Extract number from names like "Prog_button (1)", "Prog_button (2)", etc.
            string numberStr = buttonName.Replace("Prog_button (", "").Replace(")", "");
            buttonNumber = int.Parse(numberStr);
        }

        // Initialize based on current learning mode
        switch (global_settings.currentLearningMode)
        {
            case LearningMode.Progressions:
                button_initialisation_progression();
                break;
            case LearningMode.Intervals:
                button_initialisation_intervals();
                break;
            case LearningMode.Notes:
                button_initialisation_notes();
                break;
        }
    }

    private void HandleButtonClick()
    {
        switch (global_settings.currentLearningMode)
        {
            case LearningMode.Progressions:
                ButtonSelected_progressions();
                break;
            case LearningMode.Intervals:
                ButtonSelected_intervals();
                break;
            case LearningMode.Notes:
                ButtonSelected_notes();
                break;
        }
    }

    private void button_initialisation_progression()
    {
        // Calculate coordinates for progressions mode (13 frets, 6 strings)
        this.x_coord = buttonNumber % 13;
        this.y_coord = buttonNumber / 13;
        stringnum = this.y_coord;

        getNoteValue(buttonNumber);
        notename_sharp = notename_sharps[notevalue];
        notename_flat = notename_flats[notevalue];
        text.text = notename_sharp;

        // Set initial circle state to transparent
        circleIndex = 0;
    }

    private void button_initialisation_intervals()
    {
        // Calculate coordinates for intervals mode (7 frets, 6 strings)
        stringnum = buttonNumber / 7;  // 6 strings numbered 0-5
        fretnum = buttonNumber % 7;    // 7 frets numbered 0-6
        this.x_coord = fretnum;
        this.y_coord = stringnum;

        // Get note value for intervals mode
        getNoteValue_intervals(buttonNumber);
        
        // Set text to empty initially
        if (text != null)
        {
            text.text = "";
        }
        
        // Set initial circle state
        circleIndex = 0;
    }

    private void button_initialisation_notes()
    {
        // Calculate coordinates for notes mode (specific to notes mode layout)
        // TODO: Add notes-specific coordinate calculation
       // Debug.Log("Initializing button for notes mode");
    }

    void ButtonSelected_progressions()
    {
        
         if(arpeggio_manager.instance.gameStatus==arpeggio_manager.GameStatus.Next)return;
         if(arpeggio_manager.instance.gameStatus==arpeggio_manager.GameStatus.Gameover)return;
        
        if(selectedRegion == false && arpeggio_manager.instance.gameMode==arpeggio_manager.GameMode.RegionalFretboard)
        {
            Debug.LogError("Button not in selected region");
            return;
        }
        
        Debug.Log("Button selected on " + gameObject.name);
        SetActiveCircle(circleIndex);
        
        // Play the sound for this button
        audioManager.PlayNote(notevalue, x_coord, stringnum, transposed_notes_dict[stringnum]);

        arpeggio_manager.instance.Selected_prog_button(this);
    }

    void ButtonSelected_intervals()
    {
        Scene scene = SceneManager.GetActiveScene();
        this.isSelected = !this.isSelected;

        if (scene.name == "challenge_mode")
        {
            //QuizManager.instance.SelectedButton(this);
        }
        else if (scene.name == "learn_mode")
        {
            //learnmode.instance.SelectedButton_learnmode(this);
        }
    }

    void ButtonSelected_notes()
    {
        // TODO: Add notes-specific button selection logic
        Debug.Log("Button selected in notes mode");
    }

    // Public function that can be called from other scripts
    public void SetActiveCircle(int circleIndex)
    {
       // Debug.Log($"Attempting to set active circle: {circleIndex}");
        
        // Validate the index
        if (circleIndex < 0 || circleIndex >= circleImages.Count)
        {
            Debug.LogWarning($"Invalid circle index: {circleIndex}. Must be between 0 and {circleImages.Count - 1}");
            return;
        }

        this.circleIndex = circleIndex;  // Store the current circle index

        // Deactivate all circles first
        foreach (GameObject circle in circleImages)
        {
            if (circle != null)
            {
                circle.SetActive(false);
                // Reset scale of all circles
                circle.transform.localScale = originalScale;
            }
            else
            {
                Debug.LogWarning("Null reference found in circleImages list");
            }
        }

        // Set text visibility based on circleIndex
        if (text != null)
        {
            Color textColor = text.color;
            //textColor.= circleIndex == 0 ? 0f : 1f; // Transparent if 0, fully visible otherwise
            textColor.a = circleIndex == 0 ? 0f : 1f;
            text.color = textColor;
            text.alpha = circleIndex == 0 ? 0f : 1f;  // Set TextMeshPro's alpha directly
        }

        // Activate the selected circle
        if (circleImages[circleIndex] != null)
        {
            circleImages[circleIndex].SetActive(true);
            // Start scaling animation
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
            }
            scaleCoroutine = StartCoroutine(ScaleCircle(circleImages[circleIndex]));
          //  Debug.Log($"Activated circle at index {circleIndex}");
        }
        else
        {
            Debug.LogError($"Circle at index {circleIndex} is null");
        
    }
    }
/*
    // Public function that can be called from other scripts
    public void SetActiveCircle(int circleIndex)
    {
        Debug.Log($"Attempting to set active circle: {circleIndex}");
        
        // Validate the index
        if (circleIndex < 0 || circleIndex >= circleImages.Count)
        {
            Debug.LogWarning($"Invalid circle index: {circleIndex}. Must be between 0 and {circleImages.Count - 1}");
            return;
        }

        // Deactivate all circles first
        foreach (GameObject circle in circleImages)
        {
            if (circle != null)
            {
                circle.SetActive(false);
                // Reset scale of all circles
                circle.transform.localScale = originalScale;
            }
            else
            {
                Debug.LogWarning("Null reference found in circleImages list");
            }
        }

        // Activate the selected circle
        if (circleImages[circleIndex] != null)
        {
            circleImages[circleIndex].SetActive(true);
            // Start scaling animation
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
            }
            scaleCoroutine = StartCoroutine(ScaleCircle(circleImages[circleIndex]));
            Debug.Log($"Activated circle at index {circleIndex}");
        }
        else
        {
            Debug.LogError($"Circle at index {circleIndex} is null");
        }
    }
    */

    private IEnumerator ScaleCircle(GameObject circle)
    {
        float elapsedTime = 0f;
        Vector3 startScale = originalScale;
        Vector3 endScale = targetScale;

        // Start with text fully transparent
        if (text != null)
        {
            Color textColor = text.color;
            textColor.a = 0f;
            text.color = textColor;
        }

        while (elapsedTime < scaleDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / scaleDuration);
            circle.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            // Scale and fade in the text
            if (text != null)
            {
                text.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                Color textColor = text.color;
                textColor.a = circleIndex == 0 ? 0f : t; // Only fade in if not transparent state
                text.color = textColor;
            }
            yield return null;
        }

        // Ensure we end exactly at the target scale and correct opacity
        circle.transform.localScale = endScale;
        if (text != null)
        {
            text.transform.localScale = endScale;
            Color textColor = text.color;
            textColor.a = circleIndex == 0 ? 0f : 1f; // Keep transparent if circleIndex is 0
            text.color = textColor;
        }
    }
    /*

    void ButtonSelected()
    {
         if(arpeggio_manager.instance.gameStatus==arpeggio_manager.GameStatus.Next)return;
         if(arpeggio_manager.instance.gameStatus==arpeggio_manager.GameStatus.Gameover)return;
       
        if(selectedRegion==false && arpeggio_manager.instance.gameMode==arpeggio_manager.GameMode.RegionalFretboard)
        {
            Debug.Log("Button not in selected region");
            return;
        }
        
        Debug.Log("Button selected on " + gameObject.name);
        SetActiveCircle(circleIndex);
        
        // Play the sound for this button
        audioManager.PlayNote(notevalue, x_coord, stringnum,transposed_notes_dict[stringnum]);

        arpeggio_manager.instance.Selected_prog_button(this);
    }
*/
    void OnDestroy()
    {
        // Remove the event listener when the object is destroyed
        if (button != null)
        {
            button.onClick.RemoveListener(HandleButtonClick);
            Debug.Log("Click listener removed from button");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

       private void getNoteValue(int buttonNumber)
    {
        // Get the base note value from the dictionary
        int baseNoteValue = notevalue_dict[buttonNumber];
        
        // Get the transposition value for this string
        int transpositionValue = 0;
        if (transposed_notes_dict != null && transposed_notes_dict.ContainsKey(stringnum))
        {
            transpositionValue = transposed_notes_dict[stringnum];
          //  Debug.Log($"String {stringnum} is transposed by {transpositionValue}");
        }
        else
        {
            Debug.Log($"String {stringnum} is not transposed (no value in dictionary)");
        }

        // Calculate the final note value with transposition
        int transposedValue = baseNoteValue + transpositionValue;
        
        // Handle negative values by wrapping around
        while (transposedValue < 0)
        {
            transposedValue = 12 + transposedValue;
        }

        // Get the final note value in the 0-11 range
        notevalue = transposedValue % 12;
        
       // Debug.Log($"Button {buttonNumber} on string {stringnum}: Base note {baseNoteValue}, Transposed by {transpositionValue}, Final note {notevalue}");
    }

    private void getNoteValue_intervals(int buttonNumber)
    {
        if (global_settings.instance.transposed_notes_dict != null)
        {
            transposed_notes_dict = global_settings.instance.transposed_notes_dict;
           // Debug.Log("transposed value array at get noteval: " + transposed_notes_dict[0] + " " + transposed_notes_dict[1] + " " + transposed_notes_dict[2] + " " + transposed_notes_dict[3] + " " + transposed_notes_dict[4] + " ");
        }

        // Calculate the transposed value
        int transposed_mathematical_value = interval_notevalue_dict[buttonNumber] + transposed_notes_dict[stringnum];

        // Handle negative numbers
        while (transposed_mathematical_value < 0)
        {
            transposed_mathematical_value = 12 + transposed_mathematical_value;
        }

        // Use the processed transposed value
        notevalue = transposed_mathematical_value % 12;
        
        Debug.Log($"Button {buttonNumber} on string {stringnum}: Base note {interval_notevalue_dict[buttonNumber]}, Transposed by {transposed_notes_dict[stringnum]}, Final note {notevalue}");
    }

    void Start()
    { /*
       if (global_settings.instance.transposed_notes_dict!=null)
        {

            transposed_notes_dict = global_settings.instance.transposed_notes_dict;
             Debug.Log("transposed_notes_dict found from global settings singletonl");
             

        }
        else
        {
            Debug.Log("transposed_notes_dict is null");
        }
        */
    }

}
