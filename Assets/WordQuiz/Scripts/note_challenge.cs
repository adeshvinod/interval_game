using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//script for managing the notes mode of the game-A,A#,B,C etc- equivalent of intervals game quiz manager but for notes
public class note_challenge : MonoBehaviour
{
  //  public static note_challenge instance; //Instance to make is available in other scripts without reference

    [SerializeField] private NotesGameData gameData;
    [SerializeField] private EventManager eventManager;
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private Text score_text;
    [SerializeField] private List<Image> lives_image;
    [SerializeField] private Text timer_text;
    [SerializeField] private GameObject gameover_panel;
    [SerializeField] private GameObject GameRunningPanel;
    [SerializeField] private AudioSource correctanswer_audio;
    [SerializeField] private AudioSource wronganswer_audio;

    private void Awake()
    {/*
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject)
            */
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
        eventManager.OnNoteButtonSelected += HandleNoteButtonSelected;
        eventManager.OnNoteOptionSelected += HandleNoteOptionSelected;
    }

    private void OnDisable()
    {
        eventManager.OnNoteButtonSelected -= HandleNoteButtonSelected;
        eventManager.OnNoteOptionSelected -= HandleNoteOptionSelected;
    }

    private void HandleNoteButtonSelected(prog_button button)
    {
        SelectedButton(button);
    }

    private void HandleNoteOptionSelected(option_note option)
    {
        SelectedOption_guessmode(option);
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
    public prog_button[] progbuttons_;
    private int questionmode_counter;

    private List<prog_button> possibleAnswers;
    private prog_button currentQuestionButton;
    private List<prog_button> Question_button_list = new List<prog_button>();
    private GameObject option_notes_panel;
    private bool gameover_function_flag = false;
    private List<int> missedAnswers = new List<int>();
    private bool correctanswer = false;
    private bool togglesound = false;
    public GameStatus gameStatus = GameStatus.Playing;

    // Start is called before the first frame update
    void Start()
    {
        InitializeQuestionHistoryArray();

        // Initialize prog buttons
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

        option_notes_panel = GameObject.Find("option_notes");

        possibleAnswers = new List<prog_button>();
        questionmode_counter = Random.Range(4, 8);

        Debug.Log(gameData.questionList);

        foreach ((int, int) question in gameData.questionList)
        {
            Debug.Log($"Processing question coordinates: ({question.Item1}, {question.Item2})");
            int buttonIndex = gameData.Coordinate_system[question];
            Debug.Log($"Button index from coordinate system: {buttonIndex}");
            
            Question_button_list.Add(progbuttons_[buttonIndex]);
            progbuttons_[buttonIndex].selectedRegion = true;
            
            Transform spriteMask = progbuttons_[buttonIndex].transform.Find("Sprite Mask");
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

        Debug.Log("total list:" + Question_button_list.Count);

        nextQuestion();

    }

    // Update is called once per frame
    void Update()
    {
       //Debug.Log("list of notes"+settings_notechallenge.instance.questionList);
        if (gameStatus == GameStatus.Playing)
        {
            gameData.timer -= Time.deltaTime;
            int time = (int)gameData.timer;
            SetTimer(time);
        }
        else if (gameStatus == GameStatus.Next)
        {
            gameData.timer = 10f;
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

        if (gameData.timer <= 0 || gameData.lives == 0)
        {
            gameStatus = GameStatus.Gameover;
            if (gameover_function_flag == false)
            {
                gameover_function();
                gameover_function_flag = true;
            }
        }

       

        if (gameStatus == GameStatus.Gameover)
        {/*
            GameRunningPanel.gameObject.SetActive(false);
            gameover_panel.gameObject.SetActive(true);

            gameover_panel.GetComponent<gameover_notes>().loadGame();

            gameover_panel.GetComponent<gameover_notes>().saveGame();
            */



        }
       
    }

    public void showMissedNotes()               //A function which iterates through the pairs that were either wrong or took to much time to answer
    {
        Debug.Log("number of wrong pairs: " + missedAnswers.Count);

        foreach (int missednote_index in missedAnswers)
        {
            prog_button button = progbuttons_[missednote_index];
            button.SetActiveCircle(1); // Show correct answer
            button.text.text = notename_sharps[button.notevalue];
        }
    }

    public void gameover_function()
    {
        // First enable all buttons in selected region
        foreach (prog_button button in progbuttons_)
        {
            if (button.selectedRegion)
            {
                button.SetActiveCircle(0); // Reset to transparent
                button.text.text = "";
            }
        }
        
        for (int j = 0; j < 78; j++)
        {
            if (gameData.questioncounter[j] == 0)
                continue;
            if (j == gameData.currentQuestion_Answer_node && gameData.timer <= 0)
                gameData.reactiontimes[j] = gameData.reactiontimes[j] / (gameData.questioncounter[j] - 1);
            else
                gameData.reactiontimes[j] = gameData.reactiontimes[j] / gameData.questioncounter[j];

            gameData.accuracies[j] = gameData.accuracies[j] / gameData.questioncounter[j];
        }
        
        for (int j = 0; j < 78; j++)
        {
            if ((gameData.accuracies[j] > -1 && gameData.accuracies[j] < 1) || gameData.reactiontimes[j] > 5)
                missedAnswers.Add(j);
        }
         GameRunningPanel.gameObject.SetActive(false);
            gameover_panel.gameObject.SetActive(true);

            //gameover_panel.GetComponent<gameover_notes>().loadGame();

          // gameover_panel.GetComponent<gameover_notes>().saveGame();

        showMissedNotes();

    }

    void InitializeQuestionHistoryArray()
    {
        for (int j = 0; j < 78; j++)
        {
            gameData.accuracies[j] = -1;
            gameData.reactiontimes[j] = -1;
            gameData.questioncounter[j] = 0;
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

    private void setquestion_notes_guessmode()
    {
        Debug.Log("=== Starting GuessTheNote Mode ===");
        gameStatus = GameStatus.Playing;
        question_noteval = Question_button_list[Random.Range(0, Question_button_list.Count)].notevalue;
        Debug.Log($"Selected question note value: {question_noteval} ({notename_sharps[question_noteval]})");
        question_noteval_floating.text = "Guess the note displayed on the fretboard";

        possibleAnswers.Clear(); // Clear previous answers
        Debug.Log("Clearing and finding possible answers...");

        foreach(prog_button button in progbuttons_)
        {
            button.SetActiveCircle(0); // Reset to transparent
            button.text.text = "";
            
            if (!button.selectedRegion)
            {
                continue;
            }

            if(button.notevalue == question_noteval && button.selectedRegion)
            {
                possibleAnswers.Add(button);
                Debug.Log($"Added possible answer: Button {button.buttonNumber} with note {notename_sharps[button.notevalue]}");
            }
        }

        Debug.Log($"Found {possibleAnswers.Count} possible answers");
        int b = Random.Range(0, possibleAnswers.Count);
        currentQuestionButton = possibleAnswers[b];
        Debug.Log($"Selected question button: Button {currentQuestionButton.buttonNumber} with note {notename_sharps[currentQuestionButton.notevalue]}");
        
        currentQuestionButton.SetActiveCircle(1);
        currentQuestionButton.text.text = "?";
        
        foreach (prog_button button in progbuttons_)
        {
            if (button != currentQuestionButton && button.selectedRegion)
            {
                button.SetActiveCircle(0);
            }
        }
        
        gameData.currentQuestion_Answer_node = currentQuestionButton.buttonNumber;
        Debug.Log($"Set currentQuestion_Answer_node to: {gameData.currentQuestion_Answer_node}");
        
        if (gameData.accuracies[gameData.currentQuestion_Answer_node] == -1)
        {
            gameData.accuracies[gameData.currentQuestion_Answer_node]++;
            gameData.reactiontimes[gameData.currentQuestion_Answer_node]++;
        }
        gameData.questioncounter[gameData.currentQuestion_Answer_node]++;
        Debug.Log("=== GuessTheNote Mode Setup Complete ===");
    }
    private void setquestion_notes()
    {
        gameStatus = GameStatus.Playing;
        question_noteval = Question_button_list[Random.Range(0, Question_button_list.Count)].notevalue;
        question_noteval_floating.text = notename_sharps[question_noteval];

        foreach(prog_button button in progbuttons_)
        {
            button.SetActiveCircle(0); // Reset to transparent
            button.text.text = "";
            
            if (!button.selectedRegion)
            {
                continue;
            }
        }
    }

    public void SelectedButton(prog_button value)
    {
        if (gameStatus == GameStatus.Next || questionmode == QuestionMode.GuessTheNote || gameStatus == GameStatus.Gameover) return;
        
        if (value.notevalue == question_noteval && value.selectedRegion)
        {
            correctanswer = true;
            togglesound = true;

            if (gameData.timer >= 7)
                gameData.score = gameData.score + 10;
            else if (gameData.timer > 0 && gameData.timer < 7)
                gameData.score = gameData.score + 5;
            score_text.text = gameData.score.ToString();

            value.SetActiveCircle(1); // Show correct answer
            value.text.text = notename_sharps[value.notevalue];
            
            gameStatus = GameStatus.Next;
            Invoke("nextQuestion", 0.5f);
        }
        else
        {
            correctanswer = false;
            togglesound = true;

            gameData.lives--;
            lives_image[gameData.lives].gameObject.SetActive(false);

            if (gameData.lives == 0)
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
        Debug.Log("=== Option Selected in GuessTheNote Mode ===");
        Debug.Log($"Current game status: {gameStatus}, Question mode: {questionmode}");
        Debug.Log($"Selected option note value: {value.noteValue} ({notename_sharps[value.noteValue]})");
        Debug.Log($"Expected answer: {question_noteval} ({notename_sharps[question_noteval]})");
        Debug.Log($"Current question button: Button {currentQuestionButton?.buttonNumber} with note {currentQuestionButton?.notevalue}");

        if (gameStatus == GameStatus.Next || questionmode == QuestionMode.PressTheNote || currentQuestionButton == null)
        {
            Debug.Log("Selection ignored - Invalid game state or missing question button");
            return;
        }

        if(value.noteValue == question_noteval)
        {
            Debug.Log("Correct answer selected!");
            correctanswer = true;
            togglesound = true;

            if (gameData.timer >= 7)
                gameData.score = gameData.score + 10;
            else if (gameData.timer > 0 && gameData.timer < 7)
                gameData.score = gameData.score + 5;
            score_text.text = gameData.score.ToString();

            gameData.accuracies[gameData.currentQuestion_Answer_node]++;
            gameData.reactiontimes[gameData.currentQuestion_Answer_node] = gameData.reactiontimes[gameData.currentQuestion_Answer_node] + (10f - gameData.timer);

            // Use the stored reference to update the correct button
            currentQuestionButton.text.text = notename_sharps[question_noteval];
            currentQuestionButton.SetActiveCircle(1);
            Debug.Log($"Updated question button text to: {notename_sharps[question_noteval]}");

            gameStatus = GameStatus.Next;
            Invoke("nextQuestion", 0.5f);
        }
        else
        {
            Debug.Log("Wrong answer selected!");
            correctanswer = false;
            togglesound = true;

            gameData.lives--;
            lives_image[gameData.lives].gameObject.SetActive(false);

            currentQuestionButton.text.text = notename_sharps[question_noteval];
            currentQuestionButton.SetActiveCircle(1);
            Debug.Log($"Updated question button text to: {notename_sharps[question_noteval]}");

            if (gameData.lives == 0)
            {
                Debug.Log("Game Over - No lives remaining");
                gameStatus = GameStatus.Gameover;
            }
            else
            {
                gameStatus = GameStatus.Next;
                Invoke("nextQuestion", 2.5f);
            }
        }
        Debug.Log("=== Option Selection Complete ===");
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


