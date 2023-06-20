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

    public GameObject metronome_object;

    private GameObject shadedregion_instance;
    public int progression_index=0;

    public GameStatus gameStatus = GameStatus.Playing;     //to keep track of game status  d
    public GameMode gameMode = GameMode.RegionalFretboard;
    public QuestionMode questionMode = QuestionMode.RandomQuestions;

    private int RegionalFretboard_rows = 4;
    private int RegionalFretboard_columns=7;  //decodes how big the box should be for the restricted fretboard region
    private int TotalCorrectAnswers_count_RFB = 0;
    private int CurrentCorrectAnswers_count_RFB = 0; //keeps track of how many any right answers you selected 


    private int x_coord_startpoint;
    private int y_coord_startpoint;

    public List<int> answer;                //list of notevalues present in the chord (calculated wrt Root note according to chord formula
    public List<int> answer_restrictedFB; //list of the sibling index values when we play in restrictedfretboard mode
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
        shadedregion_instance = GameObject.Find("shaded_region");
        GameObject originalGameObject = GameObject.Find("notebuttons");
        notebuttons_ = originalGameObject.GetComponentsInChildren<note_button>();
        

        CorrectButton = ColorBlock.defaultColorBlock;
        CorrectButton.normalColor = new Color(0, 255, 0, 255);
        CorrectButton.selectedColor = new Color(0, 255, 0,255);
        


        RegularButton = ColorBlock.defaultColorBlock;
        RegularButton.normalColor = new Color(0,0,0,0);
        RegularButton.selectedColor = new Color(255, 0, 0, 255 );

        chord_formula_index_list = new Dictionary<int, int[]>();
        answer = new List<int>();

        // Add arrays to the dictionary
        chord_formula_index_list.Add(0,maj7);
        chord_formula_index_list.Add(1,min7);
        chord_formula_index_list.Add(2,dom7);
        chord_formula_index_list.Add(3,min7b5);
        if(settings_arpgame.instance.myProgression.Count>0)
        {
            questionMode = QuestionMode.ChordProgressionQuestions;
            Debug.Log("mode is:chord prog");
        }
        else
        {
            questionMode = QuestionMode.RandomQuestions;
            Debug.Log("mode is random Qs");
        }
        next_question();
       

    }

    public void next_question()
    {
        gameStatus = GameStatus.Playing;
        // question_chordtype = Random.Range(0, 4);
        //question_notevalue = Random.Range(0, 12);
        if (questionMode == QuestionMode.RandomQuestions)
        {
            question_chordtype = settings_arpgame.instance.QuestionList_chordtypes[Random.Range(0, settings_arpgame.instance.QuestionList_chordtypes.Count)];
            question_notevalue = Random.Range(0, 12);
        }
        else if (questionMode == QuestionMode.ChordProgressionQuestions)
        {
            question_chordtype = settings_arpgame.instance.myProgression[progression_index].chordtype;
            question_notevalue = settings_arpgame.instance.myProgression[progression_index].note;
        }


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

            //this is only for gameMode restrictedFretboard
            note_button_temp.transform.Find("Sprite Mask").gameObject.SetActive(false);
            note_button_temp.selectedRegion = false;


            note_button_temp.interactable = true;

        }

        if (gameMode == GameMode.RegionalFretboard)
        {
            TotalCorrectAnswers_count_RFB = 0;
            CurrentCorrectAnswers_count_RFB = 0;
            x_coord_startpoint = Random.Range(0, 13 - RegionalFretboard_columns);
            y_coord_startpoint = Random.Range(0, 6 - RegionalFretboard_rows);
            Debug.Log("xcoord:" + x_coord_startpoint + "    ycoord: " + y_coord_startpoint);


            foreach (note_button note_button_temp in notebuttons_)
            {
                
                if(note_button_temp.x_coord>=x_coord_startpoint && note_button_temp.x_coord<x_coord_startpoint+RegionalFretboard_columns && note_button_temp.y_coord >= y_coord_startpoint && note_button_temp.y_coord < y_coord_startpoint + RegionalFretboard_rows)
                {
                    note_button_temp.transform.Find("Sprite Mask").gameObject.SetActive(true);
                    note_button_temp.selectedRegion = true;
                    
                    foreach(int element in answer)
                    {
                        if(note_button_temp.notevalue==element)
                        {
                            answer_restrictedFB.Add(note_button_temp.transform.GetSiblingIndex());
                            TotalCorrectAnswers_count_RFB++;
                            break;

                        }

                    }
                    //if note_button_temp.notevalue
                    
                }

            }
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
                 // value.interactable = false; //basically to reset the button from selected state to normal state, we will reactive the interactability at the end of this iteration

                  value.colors = CorrectButton;

                 // value.interactable = true;

                if(gameMode==GameMode.TwoOctaveArpeggio)
                  evaluate_fixedoctaves(value.notevalue,i,2);
                else if(gameMode==GameMode.OneOctaveArpeggio)
                {
                    evaluate_fixedoctaves(value.notevalue, i, 1);
                }
                else if(gameMode==GameMode.RegionalFretboard)
                        {
                    evaluate_RestrictedFretboard(value.transform.GetSiblingIndex());
                }
                break;
              }
          }
          if(i==answer.Count)
          {
              //value.interactable = false; //basically to reset the button from selected state to normal state, we will reactive the interactability at the end of this iteration

              value.colors = RegularButton;

             // value.interactable = true;
          }
        
        Debug.Log("The button row and columns is :"+value.x_coord+"  "+value.y_coord);

    }

    public void evaluate_fixedoctaves(int notevalue_,int index,int no_of_octaves)
    {
        answerchecklist[index]++;
        int j;
        for( j = 0; j < answer.Count;j++)
        {
            if (answerchecklist[j] < no_of_octaves)
                break;
        }
        string arrayString = string.Join(", ",answerchecklist);

        // Display the array in the console
        Debug.Log("Array: " + arrayString);
        

        if(j==answer.Count)
        {
           
            gameStatus = GameStatus.Next;
            Debug.Log("CORRECT ANSWER!");
            if (questionMode == QuestionMode.ChordProgressionQuestions)
                progression_index = (progression_index + 1) % settings_arpgame.instance.myProgression.Count;
            Invoke("next_question", 3f);
        }
    }

    public void evaluate_RestrictedFretboard(int selected_siblingindex)
    {
        int elementToRemove=1000;  //assign to some nuber outside the fretboard range scope

        foreach(int element in answer_restrictedFB)
        {
            if(element==selected_siblingindex)
            {
                elementToRemove = selected_siblingindex;
            }
        }
        if(elementToRemove<1000)
        answer_restrictedFB.Remove(elementToRemove);
        Debug.Log("no of elements left: " + answer_restrictedFB.Count);

        if (answer_restrictedFB.Count==0)
        {
            gameStatus = GameStatus.Next;
            Debug.Log("CORRECT ANSWER!");
            if(questionMode==QuestionMode.ChordProgressionQuestions)
            progression_index = (progression_index + 1) % settings_arpgame.instance.myProgression.Count;
            Invoke("next_question", 3f);
        }

        Debug.Log("answer_restrictedFB=" + answer_restrictedFB);
    }
    // Update is called once per frame

    public void SetGameMode(int gameMode_index)
    {
        switch(gameMode_index)
        {
            case 0:
                             shadedregion_instance.SetActive(false);
                            metronome_object.GetComponent<Metronome>().initialize();               
                            gameMode = GameMode.OpenFretboard;
                            break;
            case 1: gameMode =  GameMode.RegionalFretboard;
                shadedregion_instance.SetActive(true);
                next_question();
                                break;
           
            case 2:              shadedregion_instance.SetActive(false);
                gameMode = GameMode.OneOctaveArpeggio;
               // shadedregion_instance.SetActive(true);
                next_question();
                                break;
            
            case 3:         shadedregion_instance.SetActive(false);
                            gameMode =GameMode.TwoOctaveArpeggio;
              //  shadedregion_instance.SetActive(true);
                next_question();
                                break;

            default:break;
        }
    }
    void Update()
    {
      
        
    }

    public enum GameStatus
    {
        Next,
        Playing,
        Gameover
    }

    public enum GameMode
    {
        OpenFretboard,
        RegionalFretboard,
        TwoOctaveArpeggio,
        OneOctaveArpeggio
    }

    public enum QuestionMode
    {
        RandomQuestions,            //ask from the set of chord types user selected in pre game menu. the root notes can be anywhere
        ChordProgressionQuestions  //ask from a specific chord progression user entered, Root notes and order are fixed 
    }
}
