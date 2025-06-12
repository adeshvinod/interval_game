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
    public static intervalsGameManager instance;

    [SerializeField] private materialController materialController;
    [SerializeField] private GameObject gameComplete;
    [SerializeField] private TextMeshProUGUI questionChordFloating;
    [SerializeField] private GameSettings gameSettings; // Reference to the GameSettings ScriptableObject

    [SerializeField] private interval_option[] optionintervalList;
    private GameObject optionintervalList_parent;

    public GameStatus gameStatus = GameStatus.Playing;
    private QuestionMode questionMode = QuestionMode.PressTheInterval;

    public int intervalquestion_val;
    public prog_button[] progbuttons_;
    public prog_button currentrootnode;
    private prog_button correctnode;
    public int[] rootoptions = new int[] { 3, 10, 17, 24, 31, 38 };
    public SpriteRenderer[] strings;
    private List<prog_button> possibleAnswers;
    private int highlightedstring;

    public int score = 0;
    public TextMeshProUGUI score_text;
    public int lives = 3;
    [SerializeField] public List<Image> lives_image;
    public float timer = 10;
    public int time;
    [SerializeField] public TextMeshProUGUI timer_text;

    public float[] accuracies = new float[12];
    public float[] reactiontimes = new float[12];
    public int[] questioncounter = new int[12];

    public float[,] questionHistory_accuracy = new float[6, 42];
    public float[,] questionHistory_rxntimes = new float[6, 42];
    public float[,] questionHistory_Counter = new float[6, 42];

    private float[,] avg_rxntimes = new float[6, 42];
    private float[,] avg_accuracies = new float[6, 42];

    private List<(int, int)> wrongPairs = new List<(int, int)>();
    private int wrongPairs_index = 0;

    public int currentQuestion_Question_node;
    public int currentQuestion_Answer_node;

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

    public AudioSource correctanswer_audio;
    public AudioSource wronganswer_audio;
    private bool correctanswer = false;
    private bool togglesound = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        // Find GameSettings if not assigned
        if (gameSettings == null)
        {
            gameSettings = Resources.Load<GameSettings>("GameSettings");
            if (gameSettings == null)
            {
                Debug.LogError("GameSettings ScriptableObject not found in Resources folder!");
            }
        }
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
        nextQuestion();
        timer = 10f;
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
        intervalquestion_val = gameSettings.intervalQuestionList[Random.Range(0, gameSettings.intervalQuestionList.Count)];
        Debug.Log("intervalquestion_val: " + intervalquestion_val + "  questionList: " + string.Join(", ", gameSettings.intervalQuestionList));
        questioncounter[intervalquestion_val]++;

        String intervalquestion_text = intervalname[intervalquestion_val];
        questionChordFloating.gameObject.SetActive(true);
        questionChordFloating.text = intervalquestion_text;
        possibleAnswers.Clear();

        materialController.ResetAllStringsToBaseIntensity();

        int a = gameSettings.selectedIntervalStrings[Random.Range(0, gameSettings.selectedIntervalStrings.Count)];
        foreach (prog_button prog_button_ in progbuttons_)
        {
            prog_button_.SetActiveCircle(0); // Reset to transparent

            if (prog_button_.transform.GetSiblingIndex() == rootoptions[a])
            {
                prog_button_.isroot = 1;
                prog_button_.SetActiveCircle(3); // Root button state
                prog_button_.text.text = "R";
                currentrootnode = prog_button_;
            }
            else
            {
                prog_button_.isroot = 0;
                prog_button_.SetActiveCircle(0);
                prog_button_.text.text = "";
            }

            if ((prog_button_.notevalue - progbuttons_[rootoptions[a]].notevalue) == intervalquestion_val || 
                (prog_button_.notevalue - progbuttons_[rootoptions[a]].notevalue) == (intervalquestion_val - 12))
            {
                possibleAnswers.Add(prog_button_);
            }
        }

        int b = Random.Range(0, possibleAnswers.Count);
        highlightedstring = possibleAnswers[b].stringnum;
        correctnode = possibleAnswers[b];

        currentQuestion_Answer_node = correctnode.transform.GetSiblingIndex();
        currentQuestion_Question_node = a;
        if (questionHistory_accuracy[currentQuestion_Question_node, currentQuestion_Answer_node] == -1)
        {
            questionHistory_accuracy[currentQuestion_Question_node, currentQuestion_Answer_node]++;
            questionHistory_rxntimes[currentQuestion_Question_node, currentQuestion_Answer_node]++;
        }
        questionHistory_Counter[currentQuestion_Question_node, currentQuestion_Answer_node]++;

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
        for (int i = 0; i < questionHistory_accuracy.GetLength(0); i++)
        {
            for (int j = 0; j < questionHistory_accuracy.GetLength(1); j++)
            {
                questionHistory_accuracy[i, j] = -1;
                questionHistory_rxntimes[i, j] = -1;
                questionHistory_Counter[i, j] = 0;
                avg_accuracies[i, j] = -1;
                avg_rxntimes[i, j] = -1;
            }
        }
    }

    void SetQuestion_intervals_guessmode()
    {
        questionChordFloating.gameObject.SetActive(false);
        gameStatus = GameStatus.Playing;
        intervalquestion_val = gameSettings.intervalQuestionList[Random.Range(0, gameSettings.intervalQuestionList.Count)];
        questioncounter[intervalquestion_val]++;

        String intervalquestion_text = intervalname[intervalquestion_val];
        questionChordFloating.text = intervalquestion_text;
        possibleAnswers.Clear();

        materialController.ResetAllStringsToBaseIntensity();

        int a = gameSettings.selectedIntervalStrings[Random.Range(0, gameSettings.selectedIntervalStrings.Count)];
        foreach (prog_button prog_button_ in progbuttons_)
        {
            prog_button_.SetActiveCircle(0);

            if (prog_button_.transform.GetSiblingIndex() == rootoptions[a])
            {
                prog_button_.isroot = 1;
                prog_button_.SetActiveCircle(3);
                prog_button_.text.text = "R";
                currentrootnode = prog_button_;
            }
            else
            {
                prog_button_.isroot = 0;
                prog_button_.SetActiveCircle(0);
                prog_button_.text.text = "";
            }

            if ((prog_button_.notevalue - progbuttons_[rootoptions[a]].notevalue) == intervalquestion_val || 
                (prog_button_.notevalue - progbuttons_[rootoptions[a]].notevalue) == (intervalquestion_val - 12))
            {
                possibleAnswers.Add(prog_button_);
            }
        }

        int b = Random.Range(0, possibleAnswers.Count - 1);
        possibleAnswers[b].SetActiveCircle(1);
        correctnode = possibleAnswers[b];

        currentQuestion_Answer_node = correctnode.transform.GetSiblingIndex();
        currentQuestion_Question_node = a;
        if (questionHistory_accuracy[currentQuestion_Question_node, currentQuestion_Answer_node] == -1)
        {
            questionHistory_accuracy[currentQuestion_Question_node, currentQuestion_Answer_node]++;
            questionHistory_rxntimes[currentQuestion_Question_node, currentQuestion_Answer_node]++;
        }
        questionHistory_Counter[currentQuestion_Question_node, currentQuestion_Answer_node]++;
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Canvas touched at position: " + Input.mousePosition);
        }

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
            correctanswer_audio.Play();
            togglesound = false;
        }
        else if (correctanswer == false && togglesound == true)
        {
            wronganswer_audio.Play();
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
    }

    public void showWrongPairs(int direction)
    {
        if (wrongPairs.Count == 0) return;

        foreach (prog_button prog_button_ in progbuttons_)
        {
            prog_button_.SetActiveCircle(0);
            prog_button_.text.text = "";
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

        Debug.Log("item 1:" + wrongPairs[wrongPairs_index].Item1 + "   item2:" + wrongPairs[wrongPairs_index].Item2);

        progbuttons_[rootoptions[wrongPairs[wrongPairs_index].Item1]].SetActiveCircle(3);
        progbuttons_[rootoptions[wrongPairs[wrongPairs_index].Item1]].text.text = "R";
        progbuttons_[wrongPairs[wrongPairs_index].Item2].SetActiveCircle(1);
        int computed_wrongpair_interval = progbuttons_[wrongPairs[wrongPairs_index].Item2].notevalue - 
                                        progbuttons_[rootoptions[wrongPairs[wrongPairs_index].Item1]].notevalue;
        progbuttons_[wrongPairs[wrongPairs_index].Item2].text.text = 
            (computed_wrongpair_interval < 0) ? intervalname[12 + computed_wrongpair_interval] : 
            intervalname[computed_wrongpair_interval];
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
                if (questionHistory_Counter[i, j] == 0)
                    continue;
                if (i == currentQuestion_Question_node && j == currentQuestion_Answer_node && time <= 0)
                    avg_rxntimes[i, j] = questionHistory_rxntimes[i, j] / (questionHistory_Counter[i, j] - 1);
                else
                    avg_rxntimes[i, j] = questionHistory_rxntimes[i, j] / questionHistory_Counter[i, j];

                avg_accuracies[i, j] = questionHistory_accuracy[i, j] / questionHistory_Counter[i, j];
            }
        }

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 42; j++)
            {
                if ((avg_accuracies[i, j] > -1 && avg_accuracies[i, j] < 1) || avg_rxntimes[i, j] > 5)
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
        if (gameStatus == GameStatus.Next || questionMode == QuestionMode.PressTheInterval) return;

        if (value.intervalValue == intervalquestion_val)
        {
            if (time >= 7)
                score = score + 10;
            else if (time > 0 && time < 7)
                score = score + 5;

            correctanswer_audio.Play();
            score_text.text = score.ToString();

            reactiontimes[intervalquestion_val] = reactiontimes[intervalquestion_val] + (10f - time);
            accuracies[intervalquestion_val]++;

            questionHistory_accuracy[currentQuestion_Question_node, currentQuestion_Answer_node]++;
            questionHistory_rxntimes[currentQuestion_Question_node, currentQuestion_Answer_node] = 
                questionHistory_rxntimes[currentQuestion_Question_node, currentQuestion_Answer_node] + (10f - time);

            gameStatus = GameStatus.Next;
            Invoke("nextQuestion", 0.5f);
        }
        else
        {
            wronganswer_audio.Play();
            lives--;
            lives_image[lives].gameObject.SetActive(false);

            if (lives == 0)
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

        int selected_intervalvalue = value.notevalue - currentrootnode.notevalue;
        string interval_displaytext = "a";
        if (selected_intervalvalue < 0)
        {
            interval_displaytext = intervalname[12 + selected_intervalvalue];
        }
        else
        {
            interval_displaytext = intervalname[selected_intervalvalue];
        }

        value.text.text = interval_displaytext;

        if (value.stringnum == highlightedstring)
        {
            if ((value.notevalue - currentrootnode.notevalue) == intervalquestion_val || 
                (value.notevalue - currentrootnode.notevalue) == (intervalquestion_val - 12))
            {
                value.SetActiveCircle(1);
                Debug.Log("Correct Answer");

                if (time >= 7)
                    score = score + 10;
                else if (time > 0 && time < 7)
                    score = score + 5;

                correctanswer = true;
                togglesound = true;
                score_text.text = score.ToString();
                reactiontimes[intervalquestion_val] = reactiontimes[intervalquestion_val] + (10f - time);
                accuracies[intervalquestion_val]++;

                questionHistory_accuracy[currentQuestion_Question_node, currentQuestion_Answer_node]++;
                questionHistory_rxntimes[currentQuestion_Question_node, currentQuestion_Answer_node] = 
                    questionHistory_rxntimes[currentQuestion_Question_node, currentQuestion_Answer_node] + (10f - time);

                gameStatus = GameStatus.Next;
                Invoke("nextQuestion", 0.5f);
            }
            else
            {
                correctanswer = false;
                togglesound = true;

                lives--;
                lives_image[lives].gameObject.SetActive(false);
                correctnode.SetActiveCircle(1);

                if (lives == 0)
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