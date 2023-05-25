using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Random = UnityEngine.Random;

public class arpeggio_manager : MonoBehaviour
{
    public static arpeggio_manager instance;

    [SerializeField] private Text questionChordFloating_chordtype;   //the text which shows the question
    [SerializeField] private Text questionChordFloating_note;   //the text which shows the question

    public int[] maj7 = { 0, 4, 7, 11 };
    public int[] min7 = { 0, 3, 7, 10 };
    public int[] dom7 = { 0, 4, 7, 10 };
    public int[] min7b5 = { 0, 3, 6, 10 };
    public int[] answerchecklist={0,0,0,0,0,0,0,0};

    public GameStatus gameStatus = GameStatus.Playing;     //to keep track of game status  d

    public List<int> answer;

    public Dictionary<int, int[]> chord_formula_index_list = new Dictionary<int, int[]>()
    {
        
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

    public Dictionary<int, string> chord_name_list = new Dictionary<int, string>()
    {
        {0,"maj7" },
        {1,"min7" },
        {2,"dom7" },
        {3,"min7b5"}
    };

    public int question_chordtype;
    public int question_notevalue;

    ColorBlock CorrectButton = new ColorBlock();
    ColorBlock RegularButton = new ColorBlock();

    public note_button[] notebuttons_;
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
        GameObject originalGameObject = GameObject.Find("notebuttons");
        notebuttons_ = originalGameObject.GetComponentsInChildren<note_button>();

        CorrectButton = ColorBlock.defaultColorBlock;
        CorrectButton.normalColor = new Color(1, 1, 1, 1);
        CorrectButton.selectedColor = new Color(1, 1, 1, 1);

        RegularButton = ColorBlock.defaultColorBlock;
        RegularButton.normalColor = new Color(0, 0, 1, 0);
        RegularButton.selectedColor = new Color(1, 0, 0, 1);

        chord_formula_index_list = new Dictionary<int, int[]>();
        answer = new List<int>();

        // Add arrays to the dictionary
        chord_formula_index_list.Add(0,maj7);
        chord_formula_index_list.Add(1,min7);
        chord_formula_index_list.Add(2,dom7);
        chord_formula_index_list.Add(3,min7b5);

        next_question();


    }

    public void next_question()
    {
        gameStatus = GameStatus.Playing;
        question_chordtype = Random.Range(0, 4);
        question_notevalue = Random.Range(0, 12);

        questionChordFloating_chordtype.text = chord_name_list[question_chordtype];
        questionChordFloating_note.text = notename_flats[question_notevalue];

        answer.Clear();
        Array.Clear(answerchecklist, 0, answerchecklist.Length);
        for(int i=0;i<chord_formula_index_list[question_chordtype].Length;i++)
        {
            answer.Add((question_notevalue + chord_formula_index_list[question_chordtype][i])%12);
        }

        foreach (note_button note_button_temp in notebuttons_)
        {
            note_button_temp.interactable = false; //basically to reset the button from selected state to normal state, we will reactive the interactability at the end of this iteration



            note_button_temp.colors = RegularButton;
           // note_button_temp.noteText.color = new Color(note_button_temp.noteText.color.r, note_button_temp.noteText.color.g, note_button_temp.noteText.color.b, 0);

           
            note_button_temp.interactable = true;

        }
    }

    public void Selected_button(note_button value)
    {
        if (gameStatus == GameStatus.Next) return;
          int i;

          for( i=0;i<answer.Count;i++)
          {
              if (value.notevalue == answer[i])
              {
                //  value.interactable = false; //basically to reset the button from selected state to normal state, we will reactive the interactability at the end of this iteration

                  value.colors = CorrectButton;

                 // value.interactable = true;

                  evaluate(value.notevalue,i);
              }
          }
          if(i==answer.Count)
          {
             // value.interactable = false; //basically to reset the button from selected state to normal state, we will reactive the interactability at the end of this iteration

              value.colors = RegularButton;

             // value.interactable = true;
          }
        
       // Debug.Log("THE BUTTON IS PRESSED");

    }

    public void evaluate(int notevalue_,int index)
    {
        answerchecklist[index]++;
        int j;
        for( j = 0; j < answer.Count;j++)
        {
            if (answerchecklist[j] < 2)
                break;
        }
        string arrayString = string.Join(", ",answerchecklist);

        // Display the array in the console
        Debug.Log("Array: " + arrayString);
        

        if(j==answer.Count)
        {
           
            gameStatus = GameStatus.Next;
            Debug.Log("CORRECT ANSWER!");
            Invoke("next_question", 3f);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public enum GameStatus
    {
        Next,
        Playing,
        Gameover
    }
}
