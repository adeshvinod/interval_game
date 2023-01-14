using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public Text question_noteval_floating;
    int question_noteval;
    QuestionMode questionmode = QuestionMode.PressTheNote;
    public note_button[] notebuttons_;
    private int questionmode_counter;

    private List<note_button> possibleAnswers;

    private GameObject option_notes_panel;

    public GameStatus gameStatus = GameStatus.Playing;     //to keep track of game status  d
    // Start is called before the first frame update
    void Start()
    {
        CorrectButton = ColorBlock.defaultColorBlock;
        CorrectButton.normalColor = new Color(0, 1, 0, 1);
        CorrectButton.selectedColor = new Color(0, 1, 0, 1);

        RegularButton = ColorBlock.defaultColorBlock;
        RegularButton.normalColor = new Color(0, 0, 1, 0);
        RegularButton.selectedColor = new Color(1, 0, 0, 1);

     

        GameObject originalGameObject = GameObject.Find("notebuttons");
        notebuttons_ = originalGameObject.GetComponentsInChildren<note_button>();

        option_notes_panel = GameObject.Find("option_notes");

        possibleAnswers = new List<note_button>();
        questionmode_counter = Random.Range(4, 8);
        nextQuestion();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void nextQuestion()
    {
        if (questionmode_counter == 0)  //once we exhasuted the list of question in a particular mode, switch the mode
        {
            questionmode_counter = Random.Range(4, 8);
            if (questionmode == QuestionMode.PressTheNote)
                questionmode = QuestionMode.GuessTheNote;
            else if (questionmode == QuestionMode.GuessTheNote)
                questionmode = QuestionMode.PressTheNote;

        }
        //setquestion_notes();

        if (questionmode == QuestionMode.PressTheNote)
        {
           option_notes_panel.gameObject.SetActive(false);  //hide the interval options panel
            setquestion_notes();


        }
        else if (questionmode == QuestionMode.GuessTheNote)
        {
           option_notes_panel.gameObject.SetActive(true);    //display interval options panel
           setquestion_notes_guessmode();

        }

        questionmode_counter--;

    }

    private void setquestion_notes_guessmode()
    {
        gameStatus = GameStatus.Playing;
        question_noteval = Random.Range(0, 11);
        question_noteval_floating.text = "Guess the note displayed on the fretboard";

        possibleAnswers.Clear();
        foreach (note_button note_button_temp in notebuttons_)
        {
            note_button_temp.interactable = false; //basically to reset the button from selected state to normal state, we will reactive the interactability at the end of this iteration

           
  
            note_button_temp.colors = RegularButton;
            note_button_temp.noteText.color = new Color(note_button_temp.noteText.color.r, note_button_temp.noteText.color.g, note_button_temp.noteText.color.b, 0);
            
            if(note_button_temp.notevalue==question_noteval)
            {
                possibleAnswers.Add(note_button_temp);
            }

            note_button_temp.interactable = true;

        }
        int b = Random.Range(0, possibleAnswers.Count - 1);
        possibleAnswers[b].colors = CorrectButton;
        possibleAnswers[b].interactable = true;    //this ensures blue color is seen
    }
    private void setquestion_notes()
    {
        gameStatus = GameStatus.Playing;
        question_noteval = Random.Range(0, 11);
        question_noteval_floating.text = notename_sharps[question_noteval];

        foreach(note_button note_button in notebuttons_)
        {
            note_button.interactable = false; //basically to reset the button from selected state to normal state, we will reactive the interactability at the end of this iteration

            note_button.colors = RegularButton;
            note_button.noteText.color = new Color(note_button.noteText.color.r, note_button.noteText.color.g, note_button.noteText.color.b, 0);
            note_button.interactable = true;

        }


    }

    public void SelectedButton(note_button value)
    {
        if (gameStatus == GameStatus.Next || questionmode == QuestionMode.GuessTheNote) return;
        if (value.notevalue==question_noteval)
        {
            value.colors = CorrectButton;
            gameStatus = GameStatus.Next;
            Invoke("nextQuestion", 0.5f);
        }


    }

    public void SelectedOption_guessmode(option_note value)
    {
        if (gameStatus == GameStatus.Next || questionmode == QuestionMode.PressTheNote) return;

        if(value.noteValue==question_noteval)
        {
           gameStatus = GameStatus.Next;
           Invoke("nextQuestion", 0.5f);
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
