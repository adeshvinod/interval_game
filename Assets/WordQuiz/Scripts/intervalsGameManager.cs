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
    [SerializeField] public TextMeshProUGUI healthText;
    [SerializeField] public TextMeshProUGUI timer_text;
    private Coroutine healthAnimCoroutine;
    private Coroutine scoreAnimCoroutine;

    private List<(int rootNode, int answerNode)> wrongPairs = new List<(int rootNode, int answerNode)>();
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
        UpdateHealthDisplay();
        nextQuestion();
    }

    void SetQuestion_intervals()
    {
        gameStatus = GameStatus.Playing;
        
        if (gameData.intervalQuestionList != null && gameData.intervalQuestionList.Count > 0)
        {
            Debug.Log("IntervalsGameData intervalQuestionList is not null");
            Debug.Log($"intervalQuestionList count: {gameData.intervalQuestionList.Count}");
        }
        else
        {   
            Debug.LogWarning("IntervalsGameData intervalQuestionList is null or empty");
            Debug.LogWarning($"IntervalsGameData: {gameData != null}");
            if (gameData != null)
            {
                Debug.LogWarning($"intervalQuestionList: {gameData.intervalQuestionList != null}");
                if (gameData.intervalQuestionList != null)
                {
                    Debug.LogWarning("interval question list: "+ string.Join(", ", gameData.intervalQuestionList));
                }
            }
        }
        gameData.intervalquestion_val = gameData.intervalQuestionList[Random.Range(0, gameData.intervalQuestionList.Count)];
        Debug.Log("intervalquestion_val: " + gameData.intervalquestion_val + "  questionList: " + string.Join(", ", gameData.intervalQuestionList));
        gameData.questioncounter[gameData.intervalquestion_val]++;

        String intervalquestion_text = intervalname[gameData.intervalquestion_val];
        questionChordFloating.gameObject.SetActive(true);
        questionChordFloating.text = intervalquestion_text;
        possibleAnswers.Clear();

        materialController.ResetAllStringsToBaseIntensity();

        int a = gameData.selectedIntervalStrings[Random.Range(0, gameData.selectedIntervalStrings.Count)];
        
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

        StartCoroutine(PlayIntervalAudio(currentrootnode, correctnode));
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
        gameData.intervalquestion_val = gameData.intervalQuestionList[Random.Range(0, gameData.intervalQuestionList.Count)];
        Debug.Log($"Selected interval question value: {gameData.intervalquestion_val}");
        gameData.questioncounter[gameData.intervalquestion_val]++;

        String intervalquestion_text = intervalname[gameData.intervalquestion_val];
        questionChordFloating.text = intervalquestion_text;
        possibleAnswers.Clear();

        materialController.ResetAllStringsToBaseIntensity();

        int a = gameData.selectedIntervalStrings[Random.Range(0, gameData.selectedIntervalStrings.Count)];
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
        #if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Canvas touched at position: " + Input.mousePosition);
        }
        #endif

        // Only update timer and game logic if game is still playing
        if (gameStatus == GameStatus.Playing)
        {
            gameData.timer -= Time.deltaTime;
            SetTimer((int)gameData.timer);
        }
        else if (gameStatus == GameStatus.Next)
        {
            gameData.timer = 10f;
        }
        // Don't update timer when game is over

        // Handle sound effects
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

        // Timer expired: lose 34% health, play wrong sound, advance to next question
        if (gameData.timer <= 0 && gameStatus == GameStatus.Playing)
        {
            TakeDamage(34f);
            gameData.timer = 10f;
            if (gameData.health > 0)
            {
                wronganswer_audio.Play();
                gameStatus = GameStatus.Next;
                Invoke("nextQuestion", 2.5f);
            }
        }

        // Check for game over
        if (gameData.health <= 0)
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
        if (wrongPairs.Count == 0) 
        {
            Debug.Log("No wrong pairs to show");
            return;
        }

        // Ensure game is in game over state
        if (gameStatus != GameStatus.Gameover)
        {
            Debug.LogWarning("showWrongPairs called but game is not in game over state");
            return;
        }

        // Reset all buttons first and make them non-interactive
        foreach (prog_button prog_button_ in progbuttons_)
        {
            prog_button_.SetActiveCircle(0);
            prog_button_.text.text = "";
            prog_button_.isroot = 0;
            // Make sure buttons are not interactive in review mode
            prog_button_.GetComponent<Button>().interactable = false;
        }

        // Navigate through wrong pairs
        if (direction > 0)
        {
            wrongPairs_index = (wrongPairs_index + 1) % wrongPairs.Count;
        }
        else if (direction < 0)
        {
            wrongPairs_index = (wrongPairs_index - 1 + wrongPairs.Count) % wrongPairs.Count;
        }

        // Get current wrong pair
        var currentWrongPair = wrongPairs[wrongPairs_index];
        int rootNode = currentWrongPair.rootNode;
        int answerNode = currentWrongPair.answerNode;

        Debug.Log($"Showing wrong pair {wrongPairs_index + 1}/{wrongPairs.Count}: Root={rootNode}, Answer={answerNode}");

        // Find and display the root button
        prog_button rootButton = FindButtonByNumber(rootoptions[rootNode]);
        if (rootButton != null)
        {
            rootButton.SetActiveCircle(3); // Root state
            rootButton.text.text = "R";
            rootButton.isroot = 1;
        }

        // Find and display the answer button
        prog_button answerButton = FindButtonByNumber(answerNode);
        if (answerButton != null)
        {
            answerButton.SetActiveCircle(1); // Answer state
            
            // Calculate and display the interval
            int interval = CalculateInterval(rootButton.notevalue, answerButton.notevalue);
            answerButton.text.text = intervalname[interval];
        }
    }

    private prog_button FindButtonByNumber(int buttonNumber)
    {
        foreach (prog_button button in progbuttons_)
        {
            if (button.buttonNumber == buttonNumber)
            {
                return button;
            }
        }
        Debug.LogError($"Button with number {buttonNumber} not found");
        return null;
    }

    private int CalculateInterval(int rootNote, int answerNote)
    {
        if (answerNote >= rootNote)
            return answerNote - rootNote;
        else
            return 12 - rootNote + answerNote;
    }

    public void gameover_function()
    {
        // Stop the highlighted string coroutine
        StopAllCoroutines();
        
        materialController.ResetAllStringsToBaseIntensity();

        // Reset all buttons and make them non-interactive
        foreach (prog_button prog_button_ in progbuttons_)
        {
            prog_button_.SetActiveCircle(0);
            prog_button_.text.text = "";
            prog_button_.isroot = 0;
            // Disable button interaction
            prog_button_.GetComponent<Button>().interactable = false;
        }

        // Clear previous wrong pairs
        wrongPairs.Clear();

        // Identify wrong pairs based on performance data
        for (int rootNode = 0; rootNode < 6; rootNode++)
        {
            for (int answerNode = 0; answerNode < 42; answerNode++)
            {
                if (gameData.questionHistory_Counter[rootNode, answerNode] == 0)
                    continue;

                // Calculate average accuracy and reaction time
                float avgAccuracy = gameData.questionHistory_accuracy[rootNode, answerNode] / gameData.questionHistory_Counter[rootNode, answerNode];
                float avgReactionTime = gameData.questionHistory_rxntimes[rootNode, answerNode] / gameData.questionHistory_Counter[rootNode, answerNode];

                // Add to wrong pairs if:
                // 1. Accuracy is less than 100% (wrong answers)
                // 2. Average reaction time is more than 6 seconds (slow responses)
                if (avgAccuracy < 1.0f || avgReactionTime > 6.0f)
                {
                    wrongPairs.Add((rootNode, answerNode));
                    Debug.Log($"Added wrong pair: Root={rootNode}, Answer={answerNode}, " +
                             $"Accuracy={avgAccuracy:F2}, AvgTime={avgReactionTime:F2}s");
                }
            }
        }

        Debug.Log($"Total wrong pairs identified: {wrongPairs.Count}");
        wrongPairs_index = 0; // Reset index for next review session

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

        // Prevent interaction if game is over or in wrong state
        if (gameStatus == GameStatus.Gameover || gameStatus == GameStatus.Next || questionMode == QuestionMode.PressTheInterval) 
        {
            Debug.Log($"Option interaction blocked - GameStatus: {gameStatus}, QuestionMode: {questionMode}");
            return;
        }

        if (value.intervalValue == gameData.intervalquestion_val)
        {
            Debug.Log("Correct answer selected!");

            // Award points based on speed
            int oldScore_guess = gameData.score;
            if (gameData.timer >= 7)
                gameData.score = gameData.score + 10;
            else if (gameData.timer > 0 && gameData.timer < 7)
                gameData.score = gameData.score + 5;

            correctanswer_audio.Play();
            AnimateScoreIncrease(oldScore_guess, gameData.score);

            // Update performance data
            UpdatePerformanceData(true);

            gameStatus = GameStatus.Next;
            Invoke("nextQuestion", 0.5f);
        }
        else
        {
            Debug.Log($"Wrong answer - Selected: {value.intervalValue}, Expected: {gameData.intervalquestion_val}");
            wronganswer_audio.Play();
            TakeDamage(25f);

            // Update performance data for wrong answer
            UpdatePerformanceData(false);

            if (gameData.health <= 0)
            {
                Debug.Log("Game over - Health depleted");
                gameStatus = GameStatus.Gameover;
            }
            else
            {
                Debug.Log($"Wrong answer - Health remaining: {gameData.health}");
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
        // Prevent interaction if game is over or in wrong state
        if (gameStatus == GameStatus.Gameover || gameStatus == GameStatus.Next || questionMode == QuestionMode.GuessTheInterval) 
        {
            Debug.Log($"Button interaction blocked - GameStatus: {gameStatus}, QuestionMode: {questionMode}");
            return;
        }
        
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
        int interval = CalculateInterval(currentrootnode.notevalue, value.notevalue);
        value.text.text = intervalname[interval];

        Debug.Log("button note value is:"+value.notevalue+"    root node note value is:"+currentrootnode.notevalue+"  current root node number:"+currentrootnode.buttonNumber+"    interval is:"+interval+"button number is:"+value.buttonNumber);

        if (value.stringnum == highlightedstring)
        {
            if (interval == gameData.intervalquestion_val)
            {
                value.SetActiveCircle(1);
                Debug.Log("Correct Answer");

                // Award points based on speed
                int oldScore_press = gameData.score;
                if (gameData.timer >= 7)
                    gameData.score = gameData.score + 10;
                else if (gameData.timer > 0 && gameData.timer < 7)
                    gameData.score = gameData.score + 5;

                correctanswer = true;
                togglesound = true;
                AnimateScoreIncrease(oldScore_press, gameData.score);

                // Update performance data
                UpdatePerformanceData(true);

                gameStatus = GameStatus.Next;
                Invoke("nextQuestion", 0.5f);
            }
            else
            {  
                Debug.Log("wrong answer    selected interval is:"+interval+"    correct interval is:"+gameData.intervalquestion_val);
                
                correctanswer = false;
                togglesound = true;

                TakeDamage(25f);
                correctnode.SetActiveCircle(1);

                // Update performance data for wrong answer
                UpdatePerformanceData(false);

                if (gameData.health <= 0)
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

    private IEnumerator PlayIntervalAudio(prog_button rootButton, prog_button answerButton)
    {
        audioManager.PlayNote(rootButton.notevalue, rootButton.x_coord, rootButton.stringnum, gameSettings.transposedNotes_audio[rootButton.stringnum]);
        yield return new WaitForSeconds(0.3f);
        audioManager.PlayNote(answerButton.notevalue, answerButton.x_coord, answerButton.stringnum, gameSettings.transposedNotes_audio[answerButton.stringnum]);
    }

    private void UpdatePerformanceData(bool isCorrect)
    {
        // Update reaction time and accuracy for current question
        gameData.reactiontimes[gameData.intervalquestion_val] = gameData.reactiontimes[gameData.intervalquestion_val] + (10f - gameData.timer);
        
        if (isCorrect)
        {
            gameData.accuracies[gameData.intervalquestion_val]++;
            gameData.questionHistory_accuracy[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node]++;
        }
        
        gameData.questionHistory_rxntimes[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node] = 
            gameData.questionHistory_rxntimes[gameData.currentQuestion_Question_node, gameData.currentQuestion_Answer_node] + (10f - gameData.timer);
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
            Debug.Log($"User clicked on image element: {clickedObject.name}");
        }
        else
        {
            Debug.Log($"User clicked on: {clickedObject.name}");
        }
    }

    private void AnimateScoreIncrease(int fromScore, int toScore)
    {
        if (scoreAnimCoroutine != null) StopCoroutine(scoreAnimCoroutine);
        scoreAnimCoroutine = StartCoroutine(AnimateScore(fromScore, toScore));
    }

    private IEnumerator AnimateScore(int fromScore, int toScore)
    {
        float duration = 0.5f;
        float elapsed  = 0f;
        Color normal   = Color.white;
        Color glow     = new Color(1f, 0.92f, 0.016f); // golden yellow
        Vector3 normSc = Vector3.one;
        Vector3 bigSc  = new Vector3(1.5f, 1.5f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float p = Mathf.Sin(t * Mathf.PI);

            score_text.text                     = Mathf.RoundToInt(Mathf.Lerp(fromScore, toScore, t)).ToString();
            score_text.transform.localScale     = Vector3.Lerp(normSc, bigSc, p);
            score_text.color                    = Color.Lerp(normal, glow, p);
            yield return null;
        }

        score_text.text                 = toScore.ToString();
        score_text.transform.localScale = normSc;
        score_text.color                = normal;
        scoreAnimCoroutine              = null;
    }

    private void TakeDamage(float amount)
    {
        float fromHealth = gameData.health;
        gameData.health = Mathf.Max(0f, gameData.health - amount);
        if (healthAnimCoroutine != null) StopCoroutine(healthAnimCoroutine);
        healthAnimCoroutine = StartCoroutine(AnimateHealth(fromHealth, gameData.health));
    }

    private void UpdateHealthDisplay()
    {
        if (healthText != null)
            healthText.text = Mathf.RoundToInt(gameData.health) + "%";
    }

    private IEnumerator AnimateHealth(float fromHealth, float toHealth)
    {
        float duration = 0.55f;
        float elapsed = 0f;
        Color normalColor = Color.white;
        Color damageColor = new Color(1f, 0.2f, 0.2f);
        Vector3 normalScale = Vector3.one;
        Vector3 peakScale = new Vector3(1.4f, 1.4f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float pulse = Mathf.Sin(t * Mathf.PI); // arc: 0 → 1 → 0

            healthText.text = Mathf.RoundToInt(Mathf.Lerp(fromHealth, toHealth, t)) + "%";
            healthText.transform.localScale = Vector3.Lerp(normalScale, peakScale, pulse);
            healthText.color = Color.Lerp(normalColor, damageColor, pulse);
            yield return null;
        }

        healthText.text = Mathf.RoundToInt(toHealth) + "%";
        healthText.transform.localScale = normalScale;
        healthText.color = normalColor;
        healthAnimCoroutine = null;
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