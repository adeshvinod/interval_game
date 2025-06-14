using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class intervalsGameManager : MonoBehaviour, IPointerClickHandler
{
    

    [SerializeField] private materialController materialController;
   
    [SerializeField] private TextMeshProUGUI questionChordFloating;
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private IntervalsGameData gameData;

    [SerializeField] private interval_option[] optionintervalList;
    private GameObject optionintervalList_parent;

    public GameStatus gameStatus = GameStatus.Playing;
    private QuestionMode questionMode = QuestionMode.PressTheInterval;

    public prog_button[] progbuttons_;
    public prog_button currentrootnode;
    private prog_button correctnode;
    public int[] rootoptions = new int[] { 3, 10, 17, 24, 31, 38 };
    public SpriteRenderer[] strings;
    private List<prog_button> possibleAnswers;
    private int highlightedstring;

    public TextMeshProUGUI score_text;
    [SerializeField] public List<Image> lives_image;
    [SerializeField] public TextMeshProUGUI timer_text;

    private List<(int, int)> wrongPairs = new List<(int, int)>();
    private int wrongPairs_index = 0;

    private int questionmode_counter;

    public Dictionary<int, string> intervalname = new Dictionary<int, string>()
    {
        {0, "R"},
        {1, "b2"},
        {2, "M2"},
        {3, "b3"},
        {4, "M3"},
        {5, "P4"},
        {6, "b5"},
        {7, "P5"},
        {8, "b6"},
        {9, "M6"},
        {10, "b7"},
        {11, "M7"},
    };

    public GameObject GameoverPanel;
    public GameObject GameRunningPanel;
    public bool gameover_function_flag = false;
    [SerializeField] private EventManager eventManager;

    public AudioSource correctanswer_audio;
    public AudioSource wronganswer_audio;
    private bool correctanswer = false;
    private bool togglesound = false;

    private void Awake()
    {
        // Find GameSettings if not assigned
        if (gameSettings == null)
        {
            gameSettings = Resources.Load<GameSettings>("GameSettings");
            if (gameSettings == null)
            {
                Debug.LogError("GameSettings ScriptableObject not found in Resources folder!");
            }
        }

        // Find IntervalsGameData if not assigned
        if (gameData == null)
        {
            gameData = Resources.Load<IntervalsGameData>("IntervalsGameData");
            if (gameData == null)
            {
                Debug.LogError("IntervalsGameData ScriptableObject not found in Resources folder!");
            }
        }
    }

    private void OnEnable()
    {
        eventManager.OnIntervalButtonSelected += HandleIntervalButtonSelected;
        eventManager.OnIntervalOptionSelected += HandleIntervalOptionSelected;
    }
    private void OnDisable()
    {
        eventManager.OnIntervalButtonSelected -= HandleIntervalButtonSelected;
        eventManager.OnIntervalOptionSelected -= HandleIntervalOptionSelected;
    }

    private void HandleIntervalButtonSelected(prog_button button)
    {
        SelectedButton(button);
    }

    private void HandleIntervalOptionSelected(interval_option option)
    {
        SelectedOption_guessmode(option);
    }

    void Start()
    {
        InitializeQuestionHistoryArray();
        GameObject originalGameObject = GameObject.Find("Prog_buttons");
        progbuttons_ = originalGameObject.GetComponentsInChildren<prog_button>();

        GameObject strings_parent = GameObject.Find("strings");
        strings = strings_parent.GetComponentsInChildren<SpriteRenderer>();
        optionintervalList_parent = GameObject.Find("option_buttons");
        optionintervalList = GameObject.Find("option_buttons").GetComponentsInChildren<interval_option>();

        questionmode_counter = Random.Range(4, 8);
        possibleAnswers = new List<prog_button>();
        gameData.ResetGameData();
        nextQuestion();
    }

    void SetQuestion_intervals()
    {
        gameStatus = GameStatus.Playing;
        
        if (gameSettings.intervalQuestionList != null && gameSettings.intervalQuestionList.Count > 0)
        {
            Debug.Log("GameSettings intervalQuestionList is not null");
            Debug.Log($"intervalQuestionList count: {gameSettings.intervalQuestionList.Count}");
        }
        else
        {   
            Debug.LogWarning("GameSettings intervalQuestionList is null or empty");
            Debug.LogWarning($"GameSettings: {gameSettings != null}");
            if (gameSettings != null)
            {
                Debug.LogWarning($"intervalQuestionList: {gameSettings.intervalQuestionList != null}");
                if (gameSettings.intervalQuestionList != null)
                {
                    Debug.LogWarning("interval question list: "+ string.Join(", ", gameSettings.intervalQuestionList));
                }
            }
        }
        gameData.intervalquestion_val = gameSettings.intervalQuestionList[Random.Range(0, gameSettings.intervalQuestionList.Count)];
        Debug.Log("intervalquestion_val: " + gameData.intervalquestion_val + "  questionList: " + string.Join(", ", gameSettings.intervalQuestionList));
        gameData.questioncounter[gameData.intervalquestion_val]++;

        String intervalquestion_text = intervalname[gameData.intervalquestion_val];
        questionChordFloating.gameObject.SetActive(true);
        questionChordFloating.text = intervalquestion_text;
        possibleAnswers.Clear();

        materialController.ResetAllStringsToBaseIntensity();

        int a = gameSettings.selectedIntervalStrings[Random.Range(0, gameSettings.selectedIntervalStrings.Count)];
        
        // First pass: Set root node
        foreach (prog_button prog_button_ in progbuttons_)
        {
            prog_button_.SetActiveCircle(0); // Reset to transparent
            prog_button_.isroot = 0;
            prog_button_.text.text = "";

            if (prog_button_.buttonNumber == rootoptions[a])
            {
                prog_button_.isroot = 1;
                prog_button_.SetActiveCircle(3); // Root button state
                prog_button_.text.text = "R";
                currentrootnode = prog_button_;
                Debug.Log($"Setting root node - Button: {prog_button_.buttonNumber}, Note Value: {prog_button_.notevalue}, " +
                         $"String: {prog_button_.stringnum}, Fret: {prog_button_.fretnum}");
            }
        }

        // Second pass: Find possible answers
        HashSet<int> processedButtons = new HashSet<int>(); // To prevent duplicates
        foreach (prog_button prog_button_ in progbuttons_)
        {
            if (prog_button_ == currentrootnode) continue;
            if (!processedButtons.Add(prog_button_.buttonNumber)) continue; // Skip if we've already processed this button

            // Calculate interval using the same logic as learnmode.cs
            int interval;
            if (prog_button_.notevalue >= currentrootnode.notevalue)
                interval = prog_button_.notevalue - currentrootnode.notevalue;
            else
                interval = 12 - currentrootnode.notevalue + prog_button_.notevalue;

            if (interval == gameData.intervalquestion_val)
            {
                Debug.Log($"Found possible answer - Button: {prog_button_.buttonNumber}, Note Value: {prog_button_.notevalue}, " +
                         $"Root Note: {currentrootnode.notevalue}, Interval: {interval}, String: {prog_button_.stringnum}, " +
                         $"Fret: {prog_button_.fretnum}, Position: {prog_button_.transform.position}");
                possibleAnswers.Add(prog_button_);
            }
        }

        if (possibleAnswers.Count == 0)
        {
            Debug.LogError($"No possible answers found for interval {gameData.intervalquestion_val} from root note {currentrootnode.notevalue}");
            return;
        }

        int b = Random.Range(0, possibleAnswers.Count);
        highlightedstring = possibleAnswers[b].stringnum;
        correctnode = possibleAnswers[b];

        Debug.Log($"Selected correct answer - Button: {correctnode.buttonNumber}, Note Value: {correctnode.notevalue}, " +
                 $"String: {correctnode.stringnum}, Fret: {correctnode.fretnum}, Position: {correctnode.transform.position}");

        gameData.currentQuestion_Answer_node = correctnode.buttonNumber;
        gameData.currentQuestion_Question_node = a;
        if (gameData.questionHistory_accuracy[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node] == -1)
        {
            gameData.questionHistory_accuracy[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node]++;
            gameData.questionHistory_rxntimes[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node]++;
        }
        gameData.questionHistory_Counter[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node]++;

        Debug.Log($"Final setup - Root: {currentrootnode.buttonNumber}, Question Node: {a}, " +
                 $"Possible Answers: {string.Join(", ", possibleAnswers.Select(p => p.buttonNumber))}");
        Debug.Log($"Interval: {gameData.intervalquestion_val}, Answer Node: {gameData.currentQuestion_Answer_node}");

        StartCoroutine(highlightedstringcoroutine());
    }

    IEnumerator highlightedstringcoroutine()
    {
        while (gameStatus == GameStatus.Playing)
        {
            materialController.PulseStringIntensity(highlightedstring);
            yield return null;
        }
    }

    public void InitializeQuestionHistoryArray()
    {
        for (int i = 0; i < gameData.questionHistory_accuracy.GetLength(0); i++)
        {
            for (int j = 0; j < gameData.questionHistory_accuracy.GetLength(1); j++)
            {
                gameData.questionHistory_accuracy[i, j] = -1;
                gameData.questionHistory_rxntimes[i, j] = -1;
                gameData.questionHistory_Counter[i, j] = 0;
            }
        }
    }

    void SetQuestion_intervals_guessmode()
    {
        Debug.Log("=== Starting SetQuestion_intervals_guessmode ===");
        questionChordFloating.gameObject.SetActive(false);
        gameStatus = GameStatus.Playing;
        gameData.intervalquestion_val = gameSettings.intervalQuestionList[Random.Range(0, gameSettings.intervalQuestionList.Count)];
        Debug.Log($"Selected interval question value: {gameData.intervalquestion_val}");
        gameData.questioncounter[gameData.intervalquestion_val]++;

        String intervalquestion_text = intervalname[gameData.intervalquestion_val];
        questionChordFloating.text = intervalquestion_text;
        possibleAnswers.Clear();

        materialController.ResetAllStringsToBaseIntensity();

        int a = gameSettings.selectedIntervalStrings[Random.Range(0, gameSettings.selectedIntervalStrings.Count)];
        Debug.Log($"Selected string index: {a}, Root button number will be: {rootoptions[a]}");
        
        // First pass: Set root node
        foreach (prog_button prog_button_ in progbuttons_)
        {
            prog_button_.SetActiveCircle(0); // Reset to transparent
            prog_button_.isroot = 0;
            prog_button_.text.text = "";

            if (prog_button_.buttonNumber == rootoptions[a])
            {
                prog_button_.isroot = 1;
                prog_button_.SetActiveCircle(3); // Root button state
                prog_button_.text.text = "R";
                currentrootnode = prog_button_;
                Debug.Log($"Setting root node - Button: {prog_button_.buttonNumber}, Note Value: {prog_button_.notevalue}, " +
                         $"String: {prog_button_.stringnum}, Fret: {prog_button_.fretnum}");
            }
        }

        // Second pass: Find possible answers
        HashSet<int> processedButtons = new HashSet<int>(); // To prevent duplicates
        foreach (prog_button prog_button_ in progbuttons_)
        {
            if (prog_button_ == currentrootnode) continue;
            if (!processedButtons.Add(prog_button_.buttonNumber)) continue; // Skip if we've already processed this button

            // Calculate interval using the same logic as learnmode.cs
            int interval;
            if (prog_button_.notevalue >= currentrootnode.notevalue)
                interval = prog_button_.notevalue - currentrootnode.notevalue;
            else
                interval = 12 - currentrootnode.notevalue + prog_button_.notevalue;

            if (interval == gameData.intervalquestion_val)
            {
                Debug.Log($"Found possible answer - Button: {prog_button_.buttonNumber}, Note Value: {prog_button_.notevalue}, " +
                         $"Root Note: {currentrootnode.notevalue}, Interval: {interval}, String: {prog_button_.stringnum}, " +
                         $"Fret: {prog_button_.fretnum}, Position: {prog_button_.transform.position}");
                possibleAnswers.Add(prog_button_);
            }
        }

        if (possibleAnswers.Count == 0)
        {
            Debug.LogError($"No possible answers found for interval {gameData.intervalquestion_val} from root note {currentrootnode.notevalue}");
            return;
        }

        int b = Random.Range(0, possibleAnswers.Count);
        possibleAnswers[b].SetActiveCircle(1);
        correctnode = possibleAnswers[b];

        Debug.Log($"Selected correct answer - Button: {correctnode.buttonNumber}, Note Value: {correctnode.notevalue}, " +
                 $"String: {correctnode.stringnum}, Fret: {correctnode.fretnum}, Position: {correctnode.transform.position}");

        gameData.currentQuestion_Answer_node = correctnode.buttonNumber;
        gameData.currentQuestion_Question_node = a;
        if (gameData.questionHistory_accuracy[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node] == -1)
        {
            gameData.questionHistory_accuracy[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node]++;
            gameData.questionHistory_rxntimes[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node]++;
        }
        gameData.questionHistory_Counter[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node]++;

        Debug.Log($"Final setup - Root: {currentrootnode.buttonNumber}, Question Node: {a}, " +
                 $"Possible Answers: {string.Join(", ", possibleAnswers.Select(p => p.buttonNumber))}");
        Debug.Log($"Interval: {gameData.intervalquestion_val}, Answer Node: {gameData.currentQuestion_Answer_node}");
        Debug.Log("=== Finished SetQuestion_intervals_guessmode ===");
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Canvas touched at position: " + Input.mousePosition);
        }

        if (gameStatus == GameStatus.Playing)
        {
            gameData.timer -= Time.deltaTime;
            SetTimer((int)gameData.timer);
        }
        else if (gameStatus == GameStatus.Next)
        {
            gameData.timer = 10f;
        }

        if (correctanswer == true && togglesound == true)
        {
            correctanswer_audio.Play();
            togglesound = false;
        }
        else if (correctanswer == false && togglesound == true)
        {
            wronganswer_audio.Play();
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
    }

    public void showWrongPairs(int direction)
    {
        if (wrongPairs.Count == 0) return;

        // Reset all buttons first
        foreach (prog_button prog_button_ in progbuttons_)
        {
            prog_button_.SetActiveCircle(0);
            prog_button_.text.text = "";
            prog_button_.isroot = 0;
        }

        Debug.Log("number of wrong pairs: " + wrongPairs.Count);

        if (direction > 0)
        {
            wrongPairs_index = (wrongPairs_index + 1) % wrongPairs.Count;
        }
        else if (direction < 0)
        {
            wrongPairs_index = (wrongPairs_index - 1 + wrongPairs.Count) % wrongPairs.Count;
        }

        int rootStringIndex = wrongPairs[wrongPairs_index].Item1;
        int rootButtonNumber = rootoptions[rootStringIndex];
        
        Debug.Log($"Root string index: {rootStringIndex}, Root button number: {rootButtonNumber}");
        Debug.Log($"Root options array: {string.Join(", ", rootoptions)}");
        
        // Find root button by buttonNumber
        prog_button rootButton = null;
        prog_button wrongPairButton = null;
        
        foreach (prog_button button in progbuttons_)
        {
            if (button.buttonNumber == rootButtonNumber)
            {
                rootButton = button;
            }
            if (button.buttonNumber == wrongPairs[wrongPairs_index].Item2)
            {
                wrongPairButton = button;
            }
        }

        if (rootButton == null || wrongPairButton == null)
        {
            Debug.LogError($"Could not find buttons - Root: {rootButtonNumber}, Wrong Pair: {wrongPairs[wrongPairs_index].Item2}");
            return;
        }

        // Set root node
        rootButton.SetActiveCircle(3);
        rootButton.text.text = "R";
        rootButton.isroot = 1;

        // Set wrong pair node
        wrongPairButton.SetActiveCircle(1);
        
        // Calculate interval
        int rootNoteValue = rootButton.notevalue;
        int wrongPairNoteValue = wrongPairButton.notevalue;
        int computed_wrongpair_interval;
        
        if (wrongPairNoteValue >= rootNoteValue)
        {
            computed_wrongpair_interval = wrongPairNoteValue - rootNoteValue;
        }
        else
        {
            computed_wrongpair_interval = 12 - rootNoteValue + wrongPairNoteValue;
        }
        
        Debug.Log($"Root note value: {rootNoteValue}, Wrong pair note value: {wrongPairNoteValue}, Computed interval: {computed_wrongpair_interval}");
        Debug.Log($"Root button: {rootButton.buttonNumber}, Wrong pair button: {wrongPairButton.buttonNumber}");
        
        wrongPairButton.text.text = intervalname[computed_wrongpair_interval];
    }

    public void gameover_function()
    {
        materialController.ResetAllStringsToBaseIntensity();

        foreach (prog_button prog_button_ in progbuttons_)
        {
            prog_button_.SetActiveCircle(0);
            prog_button_.text.text = "";
        }

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 42; j++)
            {
                if (gameData.questionHistory_Counter[i, j] == 0)
                    continue;
                if (i == gameData.currentQuestion_Question_node && j == gameData.currentQuestion_Answer_node && gameData.timer <= 0)
                    gameData.questionHistory_rxntimes[i, j] = gameData.questionHistory_rxntimes[i, j] / (gameData.questionHistory_Counter[i, j] - 1);
                else
                    gameData.questionHistory_rxntimes[i, j] = gameData.questionHistory_rxntimes[i, j] / gameData.questionHistory_Counter[i, j];

                gameData.questionHistory_accuracy[i, j] = gameData.questionHistory_accuracy[i, j] / gameData.questionHistory_Counter[i, j];
            }
        }

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 42; j++)
            {
                if ((gameData.questionHistory_accuracy[i, j] > -1 && gameData.questionHistory_accuracy[i, j] < 1) || 
                    gameData.questionHistory_rxntimes[i, j] > 5)
                    wrongPairs.Add((i, j));
            }
        }
        Debug.Log("number of wrong pairs: " + wrongPairs.Count);

        GameRunningPanel.gameObject.SetActive(false);
        GameoverPanel.gameObject.SetActive(true);
    }

    public void nextQuestion()
    {
        if (questionmode_counter == 0)
        {
            questionmode_counter = Random.Range(4, 8);
            if (questionMode == QuestionMode.PressTheInterval)
                questionMode = QuestionMode.GuessTheInterval;
            else if (questionMode == QuestionMode.GuessTheInterval)
                questionMode = QuestionMode.PressTheInterval;
        }

        //questionMode = QuestionMode.GuessTheInterval; //for testing

        if (questionMode == QuestionMode.PressTheInterval)
        {
            optionintervalList_parent.gameObject.SetActive(false);
            SetQuestion_intervals();
        }
        else if (questionMode == QuestionMode.GuessTheInterval)
        {
            optionintervalList_parent.gameObject.SetActive(true);
            SetQuestion_intervals_guessmode();
        }

        questionmode_counter--;
    }

    public void SelectedOption_guessmode(interval_option value)
    {
        Debug.Log("=== Starting SelectedOption_guessmode ===");
        Debug.Log($"Selected option - Interval Value: {value.intervalValue}, Expected: {gameData.intervalquestion_val}");
        Debug.Log($"Current game status: {gameStatus}, Question mode: {questionMode}");

        if (gameStatus == GameStatus.Next || questionMode == QuestionMode.PressTheInterval) 
        {
            Debug.Log("Returning early - Game status is Next or not in guess mode");
            return;
        }

        if (value.intervalValue == gameData.intervalquestion_val)
        {
            Debug.Log("Correct answer selected!");
            if (gameData.timer >= 7)
                gameData.score = gameData.score + 10;
            else if (gameData.timer > 0 && gameData.timer < 7)
                gameData.score = gameData.score + 5;

            correctanswer_audio.Play();
            score_text.text = gameData.score.ToString();

            gameData.reactiontimes[gameData.intervalquestion_val] = gameData.reactiontimes[gameData.intervalquestion_val] + (10f - gameData.timer);
            gameData.accuracies[gameData.intervalquestion_val]++;

            gameData.questionHistory_accuracy[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node]++;
            gameData.questionHistory_rxntimes[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node] = 
                gameData.questionHistory_rxntimes[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node] + (10f - gameData.timer);

            gameStatus = GameStatus.Next;
            Invoke("nextQuestion", 0.5f);
        }
        else
        {
            Debug.Log($"Wrong answer - Selected: {value.intervalValue}, Expected: {gameData.intervalquestion_val}");
            wronganswer_audio.Play();
            gameData.lives--;
            lives_image[gameData.lives].gameObject.SetActive(false);

            if (gameData.lives == 0)
            {
                Debug.Log("Game over - No lives remaining");
                gameStatus = GameStatus.Gameover;
            }
            else
            {
                Debug.Log($"Wrong answer - Lives remaining: {gameData.lives}");
                gameStatus = GameStatus.Next;
                Invoke("nextQuestion", 2.5f);
            }
        }
        Debug.Log("=== Finished SelectedOption_guessmode ===");
    }

    private void SetTimer(int value)
    {
        timer_text.text = value.ToString();
    }

    public void SelectedButton(prog_button value)
    {
        if (gameStatus == GameStatus.Next || questionMode == QuestionMode.GuessTheInterval) return;
        
        if (value != null)
        {
            Debug.Log($"Element clicked - String: {value.stringnum}, Fret: {value.fretnum}, Note Value: {value.notevalue}");
        }
        else
        {
            Debug.Log("No particular element touched");
        }

        Debug.Log("the string value is: " + value.stringnum + "  HL:" + highlightedstring);
        
        // Calculate interval using the same logic as learnmode.cs
        int interval;
        if (value.notevalue >= currentrootnode.notevalue)
            interval = value.notevalue - currentrootnode.notevalue;
        else
            interval = 12 - currentrootnode.notevalue + value.notevalue;

        value.text.text = intervalname[interval];

        Debug.Log("button note value is:"+value.notevalue+"    root node note value is:"+currentrootnode.notevalue+"  current root node number:"+currentrootnode.buttonNumber+"    interval is:"+interval+"button number is:"+value.buttonNumber);

        if (value.stringnum == highlightedstring)
        {
            if (interval == gameData.intervalquestion_val)
            {
                value.SetActiveCircle(1);
                Debug.Log("Correct Answer");

                if (gameData.timer >= 7)
                    gameData.score = gameData.score + 10;
                else if (gameData.timer > 0 && gameData.timer < 7)
                    gameData.score = gameData.score + 5;

                correctanswer = true;
                togglesound = true;
                score_text.text = gameData.score.ToString();
                gameData.reactiontimes[gameData.intervalquestion_val] = gameData.reactiontimes[gameData.intervalquestion_val] + (10f - gameData.timer);
                gameData.accuracies[gameData.intervalquestion_val]++;

                gameData.questionHistory_accuracy[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node]++;
                gameData.questionHistory_rxntimes[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node] = 
                    gameData.questionHistory_rxntimes[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node] + (10f - gameData.timer);

                gameStatus = GameStatus.Next;
                Invoke("nextQuestion", 0.5f);
            }
            else
            {  
                Debug.Log("wrong answer    selected interval is:"+interval+"    correct interval is:"+gameData.intervalquestion_val);
                
                correctanswer = false;
                togglesound = true;

                gameData.lives--;
                lives_image[gameData.lives].gameObject.SetActive(false);
                correctnode.SetActiveCircle(1);

                if (gameData.lives == 0)
                {
                    gameStatus = GameStatus.Gameover;
                }
                else
                {
                    gameStatus = GameStatus.Next;
                    Invoke("nextQuestion", 2.5f);
                }
            }
        }
        else
            return;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Click detected at position: {eventData.position}");
        
        GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
        
        if (clickedObject == null)
        {
            Debug.Log("User clicked on empty space or no collider detected");
            return;
        }

        Debug.Log($"Clicked object hierarchy: {GetGameObjectPath(clickedObject)}");

        if (clickedObject.GetComponent<prog_button>() != null)
        {
            Debug.Log($"User clicked on fretboard button - String: {clickedObject.GetComponent<prog_button>().stringnum}, " +
                     $"Fret: {clickedObject.GetComponent<prog_button>().fretnum}");
        }
        else if (clickedObject.GetComponent<TextMeshProUGUI>() != null)
        {
            if (clickedObject.GetComponent<TextMeshProUGUI>() == score_text)
            {
                Debug.Log("User clicked on score text");
            }
            else if (clickedObject.GetComponent<TextMeshProUGUI>() == timer_text)
            {
                Debug.Log("User clicked on timer text");
            }
            else if (clickedObject.GetComponent<TextMeshProUGUI>() == questionChordFloating)
            {
                Debug.Log("User clicked on question text");
            }
            else
            {
                Debug.Log($"User clicked on text element: {clickedObject.name}");
            }
        }
        else if (clickedObject.GetComponent<Image>() != null)
        {
            if (lives_image.Contains(clickedObject.GetComponent<Image>()))
            {
                Debug.Log("User clicked on life element");
            }
            else
            {
                Debug.Log($"User clicked on image element: {clickedObject.name}");
            }
        }
        else
        {
            Debug.Log($"User clicked on: {clickedObject.name}");
        }
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
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
    PressTheInterval,
    GuessTheInterval
} 