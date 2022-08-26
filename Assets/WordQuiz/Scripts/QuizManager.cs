using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


//'WORDS' AND 'CHARACTERS' ARE USED INTERCHANGABLY IN THESE SCRIPT COMMENTS AND VARIABLEES

public class QuizManager : MonoBehaviour
{
    public static QuizManager instance; //Instance to make is available in other scripts without reference

    [SerializeField] private GameObject gameComplete;
    //Scriptable data which store our questions data
    [SerializeField] private QuizDataScriptable questionDataScriptable;
   // [SerializeField] private Image questionImage;           //image element to show the image
    [SerializeField] private Text questionChordFloating;
    [SerializeField] private WordData[] answerWordList2;
    [SerializeField] private WordData[] optionsWordList;    //list of options word in the game


    private GameStatus gameStatus = GameStatus.Playing;     //to keep track of game status
                                                       
    private List<int> selectedWordsIndex;                   //list which keep track of option word index w.r.t answer word index
    private List<QuestionData> shuffledquestions;
    private int currentAnswerIndex = 0, currentQuestionIndex = 0;   //index to keep track of current answer and current question
    private bool correctAnswer = true;                      //bool to decide if answer is correct or not
    private int[] answerWord2;                           //string to store answer of current question, it has to be the right answer

    ColorBlock RootButton = new ColorBlock();
    ColorBlock CorrectButton = new ColorBlock();
    ColorBlock RegularButton = new ColorBlock();

    public string intervalquestion_text;
    public int intervalquestion_val;
    // var gameobjects:GameObject[] = GameObject.FindGameObjectsWithTag("node_button");
    GameObject nullbutton;
    public intervalbutton[] intervalbuttons_;   //array of objects of class intervalbutton
    public intervalbutton currentrootnode; //stores an instance of the current Root button

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
            {8, "m6"},
            {9, "M6"},
            {10, "b7"},
            {11, "M7"},
          };

    

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
        RootButton.normalColor = new Color(1, 0, 0, 1);

        CorrectButton = ColorBlock.defaultColorBlock;
        CorrectButton.normalColor = new Color(0, 0, 1, 1);
        CorrectButton.selectedColor = new Color(0, 1, 0, 1);


        RegularButton = ColorBlock.defaultColorBlock;
        RegularButton.normalColor = new Color(0, 0, 1, 1);
        RegularButton.selectedColor = new Color(1, 1, 0, 1);



        nullbutton = GameObject.Find("nullbutton");
        GameObject originalGameObject = GameObject.Find("IntervalButtons");
        //Debug.Log(originalGameObject.name);
        //GameObject child = originalGameObject.transform.GetChild(0).gameObject;

        intervalbuttons_ = originalGameObject.GetComponentsInChildren<intervalbutton>();
      


          selectedWordsIndex = new List<int>();   //create a new list at start
                                               
        //shuffledquestions = new List<QuestionData>();
        //shuffledquestions = questionDataScriptable.questions.ToList<QuestionData>();
        //shuffledquestions = ShuffleList.ShuffleListItems<QuestionData>(shuffledquestions);
       // questionDataScriptable.questions = shuffledquestions;

        //SetQuestion();                                  //set question
        SetQuestion_intervals();




    }

    void SetQuestion()
    {
        gameStatus = GameStatus.Playing;                //set GameStatus to playing 

        //set the answerWord string variable
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

    void SetQuestion_intervals()
    {
        gameStatus = GameStatus.Playing;

       // nullbutton.GetComponent<Button>().Select();
        int a = Random.Range(0, 41);
        
        foreach (intervalbutton intervalbutton_ in intervalbuttons_)
        {
            //Debug.Log("hi bitch");
            if (intervalbutton_.transform.GetSiblingIndex() == a)
            {
                intervalbutton_.isroot = 1;
                // ColorBlock colorBlock = new ColorBlock();
                // colorBlock = ColorBlock.defaultColorBlock;
                // colorBlock.normalColor = new Color(1,0,0,1);

                intervalbutton_.GetComponent<Button>().colors = RootButton;
                currentrootnode = intervalbutton_;
               // intervalbutton_.GetComponent<Button>().interactable = false;
            }
            else
            {
                intervalbutton_.isroot = 0;
                intervalbutton_.GetComponent<Button>().colors = RegularButton;
                intervalbutton_.DeselectButton();
                //intervalbutton_.DoStateTransition
               // intervalbutton_.Do

            }

            //Debug.Log(intervalbutton_.isroot);
        }
        intervalquestion_val =Random.Range(0, 11);
        intervalquestion_text = intervalname[intervalquestion_val];
        //intervalquestion = "hi biiiittch";
         questionChordFloating.text = intervalquestion_text;
        //questionChordFloating.text = "Hi Bitch";




    }

    //Method called on Reset Button click and on new question
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

    /// <summary>
    /// When we click on any options button this method is called
    /// </summary>
    /// <param name="value"></param>
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

    public void SelectedButton(intervalbutton value)
    {
        if (gameStatus == GameStatus.Next) return;

        if((value.notevalue - currentrootnode.notevalue)==intervalquestion_val || (value.notevalue - currentrootnode.notevalue) == (intervalquestion_val - 12))
        {
            value.GetComponent<Button>().colors = CorrectButton;
            Debug.Log("Correct Answer");
            gameStatus = GameStatus.Next;
            Invoke("SetQuestion_intervals", 0.5f);
        }


    }
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

}

[System.Serializable]
public class QuestionData
{
    //public Sprite questionImage;
    public string questionchord;
    public string answer;

    public int[] answer2;
}

public enum GameStatus
{
   Next,
   Playing
}
