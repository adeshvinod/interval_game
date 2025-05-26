using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//script for managing the notes mode of the game-A,A#,B,C etc- equivalent of intervals game quiz manager but for notes
public class note_challenge : MonoBehaviour
{
    public static note_challenge instance; //Instance to make is available in other scripts without reference

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);


    }

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
         {11,"G#" },


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
         {11,"Ab" },


     };

    ColorBlock CorrectButton = new ColorBlock();
    ColorBlock RegularButton = new ColorBlock();
    ColorBlock noActionButton = new ColorBlock();

    public Text question_noteval_floating;
    int question_noteval;
    QuestionMode questionmode = QuestionMode.PressTheNote;
    public note_button[] notebuttons_;
    private int questionmode_counter;

    public int score = 0;
    public Text score_text;
    public int lives = 3;
    [SerializeField] public List<Image> lives_image;
    public float timer = 10;
    public int time; //int form of time
    [SerializeField] public Text timer_text;

    private List<note_button> possibleAnswers;

    private List<note_button> Question_button_list = new List<note_button>();

    private GameObject option_notes_panel;
    [SerializeField] public GameObject gameover_panel;
    public GameObject GameRunningPanel;
    private bool gameover_function_flag = false;

    public float[] questionHistory_accuracy = new float[78];   //creates a record of each question answer pair, initializes to -1 but changes to >=0 if that pair is invoked during the game
    public float[] questionHistory_rxntimes = new float[78];
    public float[] questionHistory_Counter = new float[78];
    private float[] avg_rxntimes = new float[78];
    private float[] avg_accuracies = new float[78]; //computed at the end after gameover

    private List<int> missedAnswers = new List<int>();
    public int currentQuestion_Answer_node;

    // Audio sources for correct and wrong answers
    public AudioSource correctanswer_audio;
    public AudioSource wronganswer_audio;
    private bool correctanswer = false;
    private bool togglesound = false;

    public GameStatus gameStatus = GameStatus.Playing;     //to keep track of game status  d
    // Start is called before the first frame update
    void Start()
    {

        CorrectButton = ColorBlock.defaultColorBlock;
        CorrectButton.normalColor = new Color(0, 1, 0, 1);
        //CorrectButton.selectedColor = new Color(0, 1, 0, 1);
        CorrectButton.selectedColor = new Color(0.773f, 0.784f, 0.263f, 1f);  // C5C843, fully visible when selected
        CorrectButton.highlightedColor = new Color(0.773f, 0.784f, 0.263f, 0f);  // C5C843, fully visible when selected

        RegularButton = ColorBlock.defaultColorBlock;
        RegularButton.normalColor = new Color(1, 1, 1, 0f);
        //RegularButton.selectedColor = new Color(1, 1, 0, 1);
        RegularButton.selectedColor = new Color(0.988f, 0.196f, 0.196f, 1f);  // FC3232, fully visible when selected
        RegularButton.highlightedColor = new Color(0.988f, 0.196f, 0.196f, 0f);  // FC3232, fully visible when selected         

        /*
        CorrectButton = ColorBlock.defaultColorBlock;
        CorrectButton.normalColor = new Color(0, 1, 0, 1);
        CorrectButton.selectedColor = new Color(0, 1, 0, 1);

        RegularButton = ColorBlock.defaultColorBlock;
        RegularButton.normalColor = new Color(0, 0, 1, 0);
        RegularButton.selectedColor = new Color(1, 0, 0, 1);
*/
        noActionButton = ColorBlock.defaultColorBlock;     //this is for the guess mode, it prevents the button from showing when you click it  
        noActionButton.normalColor = new Color(0, 0, 1, 0);
        noActionButton.selectedColor = new Color(1, 0, 0, 0);

        InitializeQuestionHistoryArray();

        // Find all note buttons from the 6 strings
        List<note_button> allNoteButtons = new List<note_button>();
        for (int i = 1; i <= 6; i++)
        {
            string stringPath = $"Canvas/fretboard/FRETBOARD_IMAGE/string{i}";
            GameObject stringObj = GameObject.Find(stringPath);
            if (stringObj != null)
            {
                note_button[] stringButtons = stringObj.GetComponentsInChildren<note_button>();
                allNoteButtons.AddRange(stringButtons);
            }
        }
        notebuttons_ = allNoteButtons.ToArray();

        option_notes_panel = GameObject.Find("option_notes");

        possibleAnswers = new List<note_button>();
        questionmode_counter = Random.Range(4, 8);

        Debug.Log(settings_notechallenge.instance.questionList);

        foreach ((int, int) question in settings_notechallenge.instance.questionList)
        {
            Debug.Log($"Processing question coordinates: ({question.Item1}, {question.Item2})");
            int buttonIndex = settings_notechallenge.instance.Coordinate_system[question];
            Debug.Log($"Button index from coordinate system: {buttonIndex}");
            
            Question_button_list.Add(notebuttons_[buttonIndex]);
            notebuttons_[buttonIndex].selectedRegion = true;
            
            Transform spriteMask = notebuttons_[buttonIndex].transform.Find("Sprite Mask");
            if (spriteMask != null)
            {
                Debug.Log($"Found Sprite Mask for button {buttonIndex}, activating it");
                spriteMask.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError($"Sprite Mask not found for button {buttonIndex}");
            }
        }

        Debug.Log("total list:" + Question_button_list.Count + "      radom note val:" + Question_button_list[2].notevalue);


        nextQuestion();


       
    }

    // Update is called once per frame
    void Update()
    {
       //Debug.Log("list of notes"+settings_notechallenge.instance.questionList);
        if (gameStatus == GameStatus.Playing)
        {
            timer -= Time.deltaTime;
            time = (int)timer;
            SetTimer(time);
        }
        else if (gameStatus == GameStatus.Next)
        {
            timer = 10f;
        }

        if (correctanswer == true && togglesound == true)
        {
            Debug.Log("Playing correct answer sound");
            if (correctanswer_audio != null)
            {
                correctanswer_audio.Play();
            }
            else
            {
                Debug.LogError("Correct answer audio source is not assigned!");
            }
            togglesound = false;
        }
        else if (correctanswer == false && togglesound == true)
        {
            Debug.Log("Playing wrong answer sound");
            if (wronganswer_audio != null)
            {
                wronganswer_audio.Play();
            }
            else
            {
                Debug.LogError("Wrong answer audio source is not assigned!");
            }
            togglesound = false;
        }

        if (time <= 0 || lives == 0)
        {
            gameStatus = GameStatus.Gameover;
            if (gameover_function_flag == false)
            {
                gameover_function();
                gameover_function_flag = true;
            }
        }

       

        if (gameStatus == GameStatus.Gameover)
        {
            GameRunningPanel.gameObject.SetActive(false);
            gameover_panel.gameObject.SetActive(true);

            gameover_panel.GetComponent<gameover_notes>().loadGame();

            gameover_panel.GetComponent<gameover_notes>().saveGame();



        }
       
    }

    public void showMissedNotes()               //A function which iterates through the pairs that were either wrong or took to much time to answer
    {
        Debug.Log("number of wrong pairs: " + missedAnswers.Count);

        foreach (int missednote_index in missedAnswers)
        {
            note_button button = notebuttons_[missednote_index];
            SetButtonState(button, true);
            button.colors = CorrectButton;
            button.noteText.text = notename_sharps[button.notevalue];
            button.UpdateTextVisibility(true);
            
        }
    }

    public void gameover_function()
    {
        // First enable all buttons in selected region
        foreach (note_button note_button_temp in notebuttons_)
        {
            if (note_button_temp.selectedRegion)
            {
                SetButtonState(note_button_temp, true);
                note_button_temp.interactable = false;
                note_button_temp.colors = RegularButton;
                note_button_temp.interactable = true;
            }
            else
            {
                SetButtonState(note_button_temp, false);
            }
        }
        
        for (int j = 0; j < 78; j++)
        {
            if (questionHistory_Counter[j] == 0)
                continue;
            if (j == currentQuestion_Answer_node && time <= 0)
                avg_rxntimes[j] = questionHistory_rxntimes[j] / (questionHistory_Counter[j] - 1);
            else
                avg_rxntimes[j] = questionHistory_rxntimes[j] / questionHistory_Counter[j];

            avg_accuracies[j] = questionHistory_accuracy[j] / questionHistory_Counter[j];
        }
        
        for (int j = 0; j < 78; j++)
        {
            if ((avg_accuracies[j] > -1 && avg_accuracies[j] < 1) || avg_rxntimes[j] > 5)
                missedAnswers.Add(j);
        }

        showMissedNotes();
    }

    void InitializeQuestionHistoryArray()
    {
            for (int j = 0; j < questionHistory_accuracy.GetLength(0); j++)
            {
                // Set the value at current row and column to -1 to indicate that pair has not been evoked
                questionHistory_accuracy[j] = -1;
                questionHistory_rxntimes[j] = -1;
                questionHistory_Counter[j] = 0;
                avg_accuracies[j] = -1;
                avg_rxntimes[j] = -1;
            }
        
    }
    private void SetTimer(int value)
    {
        timer_text.text = value.ToString();
    }

    private void nextQuestion()
    {
        if (questionmode_counter == 0)  //once we exhasuted the list of question in a particular mode, switch the mode
        {
            questionmode_counter = Random.Range(4, 8);
            if (questionmode == QuestionMode.PressTheNote)
            {
                questionmode = QuestionMode.GuessTheNote;
                question_noteval_floating.gameObject.SetActive(false);  // Hide text when switching to Guess mode
            }
            else if (questionmode == QuestionMode.GuessTheNote)
            {
                questionmode = QuestionMode.PressTheNote;
                question_noteval_floating.gameObject.SetActive(true);   // Show text when switching to Press mode
            }
        }

        if (questionmode == QuestionMode.PressTheNote)
        {
            option_notes_panel.gameObject.SetActive(false);  //hide the interval options panel
            question_noteval_floating.gameObject.SetActive(true);   // Show text in Press mode
            setquestion_notes();
        }
        else if (questionmode == QuestionMode.GuessTheNote)
        {
            option_notes_panel.gameObject.SetActive(true);    //display interval options panel
            question_noteval_floating.gameObject.SetActive(false);  // Hide text in Guess mode
            setquestion_notes_guessmode();
        }

        questionmode_counter--;
    }

    private void SetButtonState(note_button button, bool enable)
    {
        button.enabled = enable;
        button.GetComponent<Image>().enabled = enable;
        button.UpdateTextVisibility(enable);
    }

    private void setquestion_notes_guessmode()
    {
        gameStatus = GameStatus.Playing;
        question_noteval = Question_button_list[Random.Range(0, Question_button_list.Count)].notevalue;
        question_noteval_floating.text = "Guess the note displayed on the fretboard";

        // First make sure all buttons are visible and enabled
        foreach(note_button note_button in notebuttons_)
        {
            SetButtonState(note_button, true);
        }

        possibleAnswers.Clear();
        foreach (note_button note_button_temp in notebuttons_)
        {
            // Disable buttons that are not in selected region
            if (!note_button_temp.selectedRegion)
            {
                SetButtonState(note_button_temp, false);
                continue;
            }

            note_button_temp.interactable = false; //basically to reset the button from selected state to normal state, we will reactive the interactability at the end of this iteration
            note_button_temp.colors = noActionButton;
            note_button_temp.noteText.text = ""; // Clear all text initially
            note_button_temp.UpdateTextVisibility(false); //this is to make the text transparent
            
            if(note_button_temp.notevalue==question_noteval && note_button_temp.selectedRegion==true)
            {
                possibleAnswers.Add(note_button_temp);
            }

            note_button_temp.interactable = true;
        }
        int b = Random.Range(0, possibleAnswers.Count - 1);
        possibleAnswers[b].colors = CorrectButton;
        possibleAnswers[b].noteText.text = "?"; // Show question mark on the correct button
        possibleAnswers[b].UpdateTextVisibility(true);
        
        // Disable note_button and Image components for all buttons except the one with the question mark
        foreach (note_button note_button_temp in notebuttons_)
        {
            if (note_button_temp != possibleAnswers[b] && note_button_temp.selectedRegion)
            {
                SetButtonState(note_button_temp, false);
            }
        }
        
        string buttonName = possibleAnswers[b].gameObject.name;
        int buttonNumber;
        if (buttonName == "Button")
        {
            buttonNumber = 0;
        }
        else
        {
            string numberStr = buttonName.Replace("Button (", "").Replace(")", "");
            buttonNumber = int.Parse(numberStr);
        }
        currentQuestion_Answer_node = buttonNumber;
        
        possibleAnswers[b].interactable = true;

        if (questionHistory_accuracy[currentQuestion_Answer_node] == -1)
        {
            questionHistory_accuracy[currentQuestion_Answer_node]++;
            questionHistory_rxntimes[currentQuestion_Answer_node]++;
        }
        questionHistory_Counter[currentQuestion_Answer_node]++;
    }
    private void setquestion_notes()
    {
        gameStatus = GameStatus.Playing;
        question_noteval = Question_button_list[Random.Range(0,Question_button_list.Count)].notevalue;
        question_noteval_floating.text = notename_sharps[question_noteval];

        // First make sure all buttons are visible and enabled
        foreach(note_button note_button in notebuttons_)
        {
            SetButtonState(note_button, true);
        }

        foreach(note_button note_button in notebuttons_)
        {
            // Disable buttons that are not in selected region
            if (!note_button.selectedRegion)
            {
                SetButtonState(note_button, false);
                continue;
            }

            note_button.interactable = false; //basically to reset the button from selected state to normal state, we will reactive the interactability at the end of this iteration
            note_button.colors = RegularButton;
            note_button.noteText.color = new Color(note_button.noteText.color.r, note_button.noteText.color.g, note_button.noteText.color.b, 0);
            note_button.interactable = true;
        }
    }

    public void SelectedButton(note_button value)
    {
        if (gameStatus == GameStatus.Next || questionmode == QuestionMode.GuessTheNote || gameStatus == GameStatus.Gameover) return;
        if (value.notevalue == question_noteval && value.selectedRegion == true)
        {
            correctanswer = true;
            togglesound = true;

            if (time >= 7)
                score = score + 10;
            else if (time > 0 && time < 7)
                score = score + 5;
            score_text.text = score.ToString();

            value.colors = CorrectButton;
            gameStatus = GameStatus.Next;
            Invoke("nextQuestion", 0.5f);
        }
        else
        {
            correctanswer = false;
            togglesound = true;

            lives--;
            lives_image[lives].gameObject.SetActive(false);

            if (lives == 0)
                gameStatus = GameStatus.Gameover;
            else
            {
                gameStatus = GameStatus.Next;
                Invoke("nextQuestion", 2.5f);
            }
        }
    }

    public void SelectedOption_guessmode(option_note value)
    {
        if (gameStatus == GameStatus.Next || questionmode == QuestionMode.PressTheNote) return;

        if(value.noteValue==question_noteval)
        {
            correctanswer = true;
            togglesound = true;

            if (time >= 7)
                score = score + 10;
            else if (time > 0 && time < 7)
                score = score + 5;
            score_text.text = score.ToString();

            questionHistory_accuracy[currentQuestion_Answer_node]++;
            questionHistory_rxntimes[currentQuestion_Answer_node] = questionHistory_rxntimes[currentQuestion_Answer_node] + (10f - time);

            // Only update the specific button that was showing the question mark
            foreach (note_button note_button_temp in notebuttons_)
            {
                if (note_button_temp.noteText.text == "?")
                {
                    note_button_temp.noteText.text = notename_sharps[question_noteval];
                    note_button_temp.UpdateTextVisibility(true);
                    break; // Only update the one button that had the question mark
                }
            }

            gameStatus = GameStatus.Next;
            Invoke("nextQuestion", 0.5f);
        }
        else
        {
            correctanswer = false;
            togglesound = true;

            lives--;
            lives_image[lives].gameObject.SetActive(false);

            // Show the wrong answer in red and the correct answer in green
            foreach (note_button note_button_temp in notebuttons_)
            {
                if (note_button_temp.noteText.text == "?") // Only update the button that had the question mark
                {
                    note_button_temp.noteText.text = notename_sharps[question_noteval];
                    note_button_temp.UpdateTextVisibility(true);
                }
            }

            if (lives == 0)
                gameStatus = GameStatus.Gameover;
            else
            {
                gameStatus = GameStatus.Next;
                Invoke("nextQuestion", 2.5f);
            }
        }
    }
    public enum GameStatus
    {
        Next,
        Playing,
        Gameover
    }

    public enum QuestionMode
    {
        GuessTheNote,
        PressTheNote
    }

}
