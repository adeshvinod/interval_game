using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;



//'WORDS' AND 'CHARACTERS' ARE USED INTERCHANGLY IN THESE SCRIPT COMMENTS AND VARIABLEES

public class QuizManager : MonoBehaviour, IPointerClickHandler
{


    public static QuizManager instance; //Instance to make is available in other scripts without reference

    [SerializeField] private materialController materialController; // Reference set in inspector
    [SerializeField] private GameObject gameComplete;
    [SerializeField] private TextMeshProUGUI questionChordFloating;   //the text which shows the question

    [SerializeField] private interval_option[] optionintervalList;    //list of interval options in the game (R,b2,M2,b3 etc)
    private GameObject optionintervalList_parent;

    public GameStatus gameStatus = GameStatus.Playing;     //to keep track of game status  d
    private QuestionMode questionMode = QuestionMode.PressTheInterval;

    //bool coroutine_toggle = true;


    ColorBlock RootButton = new ColorBlock();
    ColorBlock CorrectButton = new ColorBlock();
    ColorBlock RegularButton = new ColorBlock();
    ColorBlock debugButton = new ColorBlock(); //a temporary colour block to help with debugging 

    //public string intervalquestion_text;
    public int intervalquestion_val;  //actual value of the interval 
    // var gameobjects:GameObject[] = GameObject.FindGameObjectsWithTag("node_button");
    //GameObject nullbutton;
    public intervalbutton[] intervalbuttons_;   //array of all the interval buttons on fretboard (everything that you can press)
    public intervalbutton currentrootnode; //stores an instance of the current Root button
    private intervalbutton correctnode;  //records the correct answer to display when you hit wrong answer
    public int[] rootoptions = new int[] { 3, 10, 17, 24, 31, 38 }; //Roots will only occur on middle frets of E,A,D,G,B strings.
    //public GameObject[] strings;
    public SpriteRenderer[] strings;
    private List<intervalbutton> possibleAnswers;
    private int highlightedstring;

    public int score = 0;
    public TextMeshProUGUI score_text;
    public int lives = 3;
    [SerializeField] public List<Image> lives_image;
    public float timer = 10;
    public int time; //int form of time
    [SerializeField] public TextMeshProUGUI timer_text;

    public float[] accuracies = new float[12];
    public float[] reactiontimes = new float[12];
    public int[] questioncounter = new int[12]; //count how many times each interval question has been asked, used to calculate average reaction times and accuracies for individual intervals

    public float[,] questionHistory_accuracy = new float[6, 42];   //creates a record of each question answer pair, initializes to -1 but changes to >=0 if that pair is invoked during the game
    public float[,] questionHistory_rxntimes = new float[6, 42];
    public float[,] questionHistory_Counter = new float[6, 42];



    private float[,] avg_rxntimes = new float[6, 42];
    private float[,] avg_accuracies = new float[6, 42]; //computed at the end after gameover

    private List<(int, int)> wrongPairs = new List<(int, int)>();
    private int wrongPairs_index = 0;

    public int currentQuestion_Question_node; //global variables to keep track of the question snode and answer sibling index so that we can record these pair in questionHistory variables
    public int currentQuestion_Answer_node;


    private int questionmode_counter;  //variable to countdown the number of questions in a particular mode( press interval, guess interval).each mode will have a series of 4-8 questions at a go

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
    public List<int> questionList;
    public GameObject GameoverPanel;  //panel displays highscore and current score
    public GameObject GameRunningPanel;
    public bool gameover_function_flag = false;

    //public gameover gameover_object;

    //set of audio sounds
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


    }

    // Start is called before the first frame update
    void Start()
    {

        RootButton = ColorBlock.defaultColorBlock;
        RootButton.normalColor = new Color(0.976f, 0.459f, 0.294f, 1f);  // F9754B, always visible
        RootButton.selectedColor = new Color(0.976f, 0.459f, 0.294f, 1f);  // Same color when selected
        RootButton.highlightedColor = new Color(0.976f, 0.459f, 0.294f, 0f);  // Same color when selected

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

        debugButton = ColorBlock.defaultColorBlock;
        debugButton.normalColor = new Color(1, 1, 1, 1);



        InitializeQuestionHistoryArray();
        //nullbutton = GameObject.Find("nullbutton");
        GameObject originalGameObject = GameObject.Find("IntervalButtons");
        intervalbuttons_ = originalGameObject.GetComponentsInChildren<intervalbutton>();

        //strings = GameObject.FindGameObjectsWithTag("string");
        GameObject strings_parent = GameObject.Find("strings");
        strings = strings_parent.GetComponentsInChildren<SpriteRenderer>();
        optionintervalList_parent = GameObject.Find("option_buttons");
        optionintervalList = GameObject.Find("option_buttons").GetComponentsInChildren<interval_option>();
        // Remove the initialization loop since it's now handled in interval_option.cs

        //gameover_object=gameObject.GetComponent<gameover>
        questionmode_counter = Random.Range(4, 8);
        //selectedWordsIndex = new List<int>(); //create a new list at start
        possibleAnswers = new List<intervalbutton>();
        nextQuestion();
        timer = 10f;

        //StartCoroutine(CallFunctionEvery5Seconds());

    }


    void SetQuestion_intervals()
    {
        gameStatus = GameStatus.Playing;
        
        if (challenge_settings.instance.questionList != null)
        {
            Debug.Log("challenge_settings or questionList is not null");
            Debug.Log($"challenge_settings.instance: {challenge_settings.instance != null}");
            Debug.Log($"questionList: {challenge_settings.instance.questionList != null}");
            Debug.Log($"questionList count: {challenge_settings.instance.questionList.Count}");
        }
        else
        {   
            Debug.LogWarning("challenge_settings or questionList is null");
            Debug.LogWarning($"challenge_settings.instance: {challenge_settings.instance != null}");
            if (challenge_settings.instance != null)
            {
                Debug.LogWarning($"questionList: {challenge_settings.instance.questionList != null}");
                if (challenge_settings.instance.questionList != null)
                {
                    Debug.LogWarning("question list: "+ string.Join(", ", challenge_settings.instance.questionList));
                }
            }
        }
        intervalquestion_val = challenge_settings.instance.questionList[Random.Range(0, challenge_settings.instance.questionList.Count)];
        Debug.Log("intervalquestion_val: " + intervalquestion_val + "  questionList: " + string.Join(", ", challenge_settings.instance.questionList));
        questioncounter[intervalquestion_val]++;

        String intervalquestion_text = intervalname[intervalquestion_val];
        questionChordFloating.gameObject.SetActive(true);
        questionChordFloating.text = intervalquestion_text;
        possibleAnswers.Clear();

        // Reset all strings to base intensity
        materialController.ResetAllStringsToBaseIntensity();

        int a = challenge_settings.instance.stringList[Random.Range(0, challenge_settings.instance.stringList.Count)];
        foreach (intervalbutton intervalbutton_ in intervalbuttons_)
        {
            intervalbutton_.interactable = false; //basically to reset the button from selected state to normal state, we will reactive the interactability at the end of this iteration

            if (intervalbutton_.transform.GetSiblingIndex() == rootoptions[a])
            {
                intervalbutton_.isroot = 1;
                intervalbutton_.colors = RootButton;
                intervalbutton_.intervalText.text = "R";
                currentrootnode = intervalbutton_;
            }

            else
            {
                intervalbutton_.isroot = 0;
                intervalbutton_.colors = RegularButton;
                intervalbutton_.intervalText.text = "\0";
            }

            if ((intervalbutton_.notevalue - intervalbuttons_[rootoptions[a]].notevalue) == intervalquestion_val || (intervalbutton_.notevalue - intervalbuttons_[rootoptions[a]].notevalue) == (intervalquestion_val - 12))
            {
                possibleAnswers.Add(intervalbutton_);
            }
            intervalbutton_.interactable = true;
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
                // Set the value at current row and column to -1 to indicate that pair has not been evoked
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
        // intervalquestion_val = Random.Range(0, 11);
        intervalquestion_val = challenge_settings.instance.questionList[Random.Range(0, challenge_settings.instance.questionList.Count)]; //choses which intervals to ask depending on the settings
        questioncounter[intervalquestion_val]++; //records the number of times this interval has been asked

        String intervalquestion_text = intervalname[intervalquestion_val];
        questionChordFloating.text = intervalquestion_text;
        possibleAnswers.Clear();

         materialController.ResetAllStringsToBaseIntensity();

         /*

        //resets the color of the strings to blue
        foreach (SpriteRenderer string_ in strings)
        {
            string_.GetComponent<SpriteRenderer>().color = new Color(0.498f, 0.780f, 0.8235f, 1);
        }
*/
        // int a = Random.Range(0, 5);
        int a = challenge_settings.instance.stringList[Random.Range(0, challenge_settings.instance.stringList.Count)]; //chooses which string to put the root on depending on the settings
        foreach (intervalbutton intervalbutton_ in intervalbuttons_)
        {

            intervalbutton_.interactable = false; //basically to reset the button from selected state to normal state, we will reactive the interactability at the end of this iteration

            if (intervalbutton_.transform.GetSiblingIndex() == rootoptions[a])
            {
                intervalbutton_.isroot = 1;
                intervalbutton_.colors = RootButton;
                intervalbutton_.intervalText.text = "R";
                currentrootnode = intervalbutton_;
               // intervalbutton_.interactable = true;  //this ensures Root node color is seen

            }
            else
            {
                intervalbutton_.isroot = 0;
                intervalbutton_.colors = RegularButton;
                intervalbutton_.intervalText.text = "\0";
            }

            if ((intervalbutton_.notevalue - intervalbuttons_[rootoptions[a]].notevalue) == intervalquestion_val || (intervalbutton_.notevalue - intervalbuttons_[rootoptions[a]].notevalue) == (intervalquestion_val - 12))
            {
                possibleAnswers.Add(intervalbutton_);
            }


            intervalbutton_.interactable = true;  
        }

        int b = Random.Range(0, possibleAnswers.Count - 1);
        possibleAnswers[b].colors = CorrectButton;
        possibleAnswers[b].interactable = true;    //this ensures blue color is seen

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
    int temp_highlight = 0;
    public void Update()
    {
        // Add touch detection at the start of Update
        if (Input.GetMouseButtonDown(0))  // This detects both mouse clicks and touch
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
        if (gameStatus == GameStatus.Gameover)
        {
/*
            GameRunningPanel.gameObject.SetActive(false);
            GameoverPanel.gameObject.SetActive(true);

            GameoverPanel.GetComponent<gameover>().loadGame();

            GameoverPanel.GetComponent<gameover>().saveGame();
            */
           
        }
    }


    public void showWrongPairs(int direction)               //A function which iterates through the pairs that were either wrong or took to much time to answer
    {
        if (wrongPairs.Count == 0) return;  // Guard against empty list

        foreach (intervalbutton intervalbutton_ in intervalbuttons_)
        {
            intervalbutton_.interactable = false;
            intervalbutton_.colors = RegularButton;
            intervalbutton_.intervalText.text = "\0";
            intervalbutton_.interactable = true;
        }

        Debug.Log("number of wrong pairs: " + wrongPairs.Count);

        // Update index based on direction, handling wrapping
        if (direction > 0)
        {
            wrongPairs_index = (wrongPairs_index + 1) % wrongPairs.Count;
        }
        else if (direction < 0)
        {
            wrongPairs_index = (wrongPairs_index - 1 + wrongPairs.Count) % wrongPairs.Count;
        }

        Debug.Log("item 1:" + wrongPairs[wrongPairs_index].Item1 + "   item2:" + wrongPairs[wrongPairs_index].Item2);

        intervalbuttons_[rootoptions[wrongPairs[wrongPairs_index].Item1]].colors = RootButton;
        intervalbuttons_[rootoptions[wrongPairs[wrongPairs_index].Item1]].intervalText.text = "R";
        intervalbuttons_[wrongPairs[wrongPairs_index].Item2].colors = CorrectButton;
        int computed_wrongpair_interval = intervalbuttons_[wrongPairs[wrongPairs_index].Item2].notevalue - intervalbuttons_[rootoptions[wrongPairs[wrongPairs_index].Item1]].notevalue;
        intervalbuttons_[wrongPairs[wrongPairs_index].Item2].intervalText.text = (computed_wrongpair_interval < 0) ? intervalname[12 + computed_wrongpair_interval] : intervalname[computed_wrongpair_interval];
    }

    public void gameover_function()
    {/*
        foreach (SpriteRenderer string_ in strings)
        {
            string_.GetComponent<SpriteRenderer>().color = new Color(0.498f, 0.780f, 0.8235f, 1);
        }
        */
         materialController.ResetAllStringsToBaseIntensity();



        foreach (intervalbutton intervalbutton_ in intervalbuttons_)
        {
            intervalbutton_.interactable = false;
            intervalbutton_.colors = RegularButton;
            intervalbutton_.interactable = true;
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
                // if (avg_rxntimes[i, j] > 5)
                //   wrongPairs.Add((i, j));

                if ((avg_accuracies[i, j] > -1 && avg_accuracies[i, j] < 1) || avg_rxntimes[i, j] > 5)
                    wrongPairs.Add((i, j));
            }
        }
        Debug.Log("number of wrong pairs: " + wrongPairs.Count);

        
            GameRunningPanel.gameObject.SetActive(false);
            GameoverPanel.gameObject.SetActive(true);

           // GameoverPanel.GetComponent<gameover>().loadGame();

          //  GameoverPanel.GetComponent<gameover>().saveGame();
    }
    IEnumerator CallFunctionEvery5Seconds()
    {
        while (true)
        {
            //coroutine_toggle = false;
            // Call the function
            //Debug.Log("highlighted string is (UPDATE)" + highlightedstring);
            if (questionMode == QuestionMode.PressTheInterval)
            {
               debughighlightedstring();
                temp_highlight = highlightedstring;
            }
            // Wait for 5 seconds before calling the function again
            yield return new WaitForSeconds(5.0f);
        }
    }

    void debughighlightedstring()
    {
//Debug.Log("highlighted string is (UPDATE)" + highlightedstring);
    }
    /// <summary>
    /// When we click on any options button this method is called
    /// </summary>
    /// <param name="value"></param>
    /// 
    /// 
    /// 
    public void nextQuestion()
    {
        if (questionmode_counter == 0)  //once we exhasuted the list of question in a particular mode, switch the mode
        {
            questionmode_counter = Random.Range(4, 8);
            if (questionMode == QuestionMode.PressTheInterval)
                questionMode = QuestionMode.GuessTheInterval;
            else if (questionMode == QuestionMode.GuessTheInterval)
                questionMode = QuestionMode.PressTheInterval;

        }

        if (questionMode == QuestionMode.PressTheInterval)
        {
            optionintervalList_parent.gameObject.SetActive(false);  //hide the interval options panel
            SetQuestion_intervals();


        }
        else if (questionMode == QuestionMode.GuessTheInterval)
        {
            optionintervalList_parent.gameObject.SetActive(true);    //display interval options panel
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
            questionHistory_rxntimes[currentQuestion_Question_node, currentQuestion_Answer_node] = questionHistory_rxntimes[currentQuestion_Question_node, currentQuestion_Answer_node] + (10f - time);

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

    public void SelectedButton(intervalbutton value)
    {
        if (gameStatus == GameStatus.Next || questionMode == QuestionMode.GuessTheInterval) return;
        
        // Add debug logging for clicked element
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
        string interval_displaytext="a";
        if (selected_intervalvalue < 0)
        {
            interval_displaytext = intervalname[12 + selected_intervalvalue];
        }
        else
        {
            interval_displaytext = intervalname[selected_intervalvalue];
        }

        value.intervalText.text = interval_displaytext;

        if (value.stringnum == highlightedstring)

        {
            if ((value.notevalue - currentrootnode.notevalue) == intervalquestion_val || (value.notevalue - currentrootnode.notevalue) == (intervalquestion_val - 12))
            {

                value.colors = CorrectButton;
                Debug.Log("Correct Answer");

                if (time >= 7)
                    score = score + 10;
                else if (time > 0 && time < 7)
                    score = score + 5;

                correctanswer = true;  //for playing audio
                togglesound = true;
                score_text.text = score.ToString();
                reactiontimes[intervalquestion_val] = reactiontimes[intervalquestion_val] + (10f - time);
                accuracies[intervalquestion_val]++;

                questionHistory_accuracy[currentQuestion_Question_node, currentQuestion_Answer_node]++;
                questionHistory_rxntimes[currentQuestion_Question_node, currentQuestion_Answer_node] = questionHistory_rxntimes[currentQuestion_Question_node, currentQuestion_Answer_node] + (10f - time);

                gameStatus = GameStatus.Next;
                Invoke("nextQuestion", 0.5f);
            }
            else
            {
                correctanswer = false;
                togglesound = true;

                lives--;
                lives_image[lives].gameObject.SetActive(false);
                correctnode.colors = CorrectButton;

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
            return;   //does nothing to score if you select option in a different string


    }
    /*
    public void ResetLastWord()
    {
        if (selectedWordsIndex.Count > 0)
        {
            int index = selectedWordsIndex[selectedWordsIndex.Count - 1];
            optionsWordList[index].gameObject.SetActive(true);
            selectedWordsIndex.RemoveAt(selectedWordsIndex.Count - 1);
            
           
            currentAnswerIndex--;
            answerWordList2[currentAnswerIndex].SetWord2(-1);
        }
    }
  */

    /*
    public void SelectedOption(WordData value)
    {
        //if gameStatus is next or currentAnswerIndex is more or equal to answerWord length
        if (gameStatus == GameStatus.Next || currentAnswerIndex >= answerWord2.Length) return;

        selectedWordsIndex.Add(value.transform.GetSiblingIndex()); //add the child index to selectedWordsIndex list, this is necesarry for resetting the char when we undo
        value.gameObject.SetActive(false); //deactivate options object
       
        answerWordList2[currentAnswerIndex].SetWord2(value.wordValue2); //set the answer word list

        currentAnswerIndex++;   //increase currentAnswerIndex

       

        if (currentAnswerIndex == answerWord2.Length)
        {
            correctAnswer = true;   //default value
            //loop through answerWordList
            for (int i = 0; i < answerWord2.Length; i++)
            {
                if (correctAnswer == false)
                    break;

                for(int j=0; j < answerWord2.Length; j++)
                {
                    if(answerWordList2[i].wordValue2 == answerWord2[j])
                    {
                        break;
                    }
                    if (j == answerWord2.Length-1)
                        correctAnswer = false;
                }
                
            }

            //if correctAnswer is true
            if (correctAnswer)
            {
                Debug.Log("Correct Answer");
                gameStatus = GameStatus.Next; //set the game status
                currentQuestionIndex++; //increase currentQuestionIndex

                //if currentQuestionIndex is less that total available questions
                if (currentQuestionIndex < questionDataScriptable.questions.Count)
                {
                    Invoke("SetQuestion", 0.5f); //go to next question
                }
                else
                {
                    Debug.Log("Game Complete"); //else game is complete
                    gameComplete.SetActive(true);
                }
            }
       
        }
    }
*/
    //Method called on Reset Button click and on new question
    /*
    public void ResetQuestion()
    {
        for (int i = 0; i < answerWordList2.Length; i++)
        {
            answerWordList2[i].gameObject.SetActive(true);
            answerWordList2[i].SetWord2(-1);
        }

        //Now deactivate the unwanted answerWordList gameobject (object more than answer string length)
        for (int i = answerWord2.Length; i < answerWordList2.Length; i++)
        {
            answerWordList2[i].gameObject.SetActive(false);
        }

        //activate all the optionsWordList objects
        for (int i = 0; i < optionsWordList.Length; i++)
        {
            optionsWordList[i].gameObject.SetActive(true);
        }
        currentAnswerIndex = 0;
    }

    */

    /*

   void SetQuestion()
   {
       gameStatus = GameStatus.Playing;                //set GameStatus to playing 
       answerWord2 = questionDataScriptable.questions[currentQuestionIndex].answer2;

       //set the image of question
       questionChordFloating.text= questionDataScriptable.questions[currentQuestionIndex].questionchord;
       ResetQuestion();                               //reset the answers and options value to orignal     

       selectedWordsIndex.Clear();                     //clear the list for new question

       //set the options words Text value
       for (int k = 0; k < optionsWordList.Length; k++)
       {         
          optionsWordList[k].SetWord2(k);         
       }

   }
   */

    public void OnPointerClick(PointerEventData eventData)
    {
        // Debug the raw event data
        Debug.Log($"Click detected at position: {eventData.position}");
        
        // Get the GameObject that was clicked
        GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
        
        if (clickedObject == null)
        {
            Debug.Log("User clicked on empty space or no collider detected");
            return;
        }

        // Log the full hierarchy of the clicked object
        Debug.Log($"Clicked object hierarchy: {GetGameObjectPath(clickedObject)}");

        // Check what type of UI element was clicked
        if (clickedObject.GetComponent<intervalbutton>() != null)
        {
            Debug.Log($"User clicked on fretboard button - String: {clickedObject.GetComponent<intervalbutton>().stringnum}, Fret: {clickedObject.GetComponent<intervalbutton>().fretnum}");
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
            // Check if it's a life image
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

    // Helper method to get the full hierarchy path of a GameObject
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

[System.Serializable]
public class QuestionData
{
    public string questionchord;
    public string answer;

    public int[] answer2;
}

public enum GameStatus
{
    Next,
    Playing,
    Gameover
}

public enum QuestionMode
{
    PressTheInterval,    //in this mode, player will have to click on the correction position on the fretboard  
    GuessTheInterval     //in this mode,player will have to click the correct option on the panel below looking at the the 2 circles on the fretboard
}