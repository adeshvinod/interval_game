using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class intervalbutton : Button
{
    //[SerializeField] private Text wordText;
    [SerializeField] public Text intervalText;
    Scene scene; //this needs to be in game settings static instance


    //lets imagine the fretboard starts from the 5th fret of the guitar and accordingle assign notes to each button. the notes A to G# is indexed from 0 to 11
    
    Dictionary<int, int> notevalue_dict = new Dictionary<int, int>()
         {
            {0, 0},
            {1, 1},
            {2, 2},
            {3, 3},
            {4, 4},
            {5, 5},
            {6, 6},

            {7, 7},
            {8, 8},
            {9, 9},
            {10, 10},
            {11, 11},
            {12, 0},
            {13, 1},

            {14, 3},
            {15, 4},
            {16, 5},
            {17, 6},
            {18, 7},
            {19, 8},
            {20, 9},

            {21, 10},
            {22, 11},
            {23, 0},
            {24, 1},
            {25, 2},
            {26, 3},
            {27, 4},

            {28, 5},
            {29, 6},
            {30, 7},
            {31, 8},
            {32, 9},
            {33, 10},
            {34, 11},

            {35, 0},
            {36, 1},
            {37, 2},
            {38, 3},
            {39, 4},
            {40, 5},
            {41, 6},
           
          };
    
   

    Dictionary<int, int> transposed_notes_dict = new Dictionary<int, int>() //for alternate tunings
           {
            {0, 1},
            {1, 1},
            {2, 0},
            {3, 0},
            {4, 0},
            {5, 0}
            
           };


    [HideInInspector]

    public int notevalue;
    public int stringnum;  //guitar string
    public int fretnum;
    public bool isSelected = false;
    public int isroot = 0; //flag for root note

    private Button buttonComponent;

    protected  override void Awake()
    {
       
        buttonComponent = GetComponent<intervalbutton>();
         if (buttonComponent)
         {
             buttonComponent.onClick.AddListener(() => buttonselected());
            
         }
      
         stringnum = this.transform.GetSiblingIndex()/ 7; //the 6 strings are numbered from 0 to 5       
       

        Debug.Log("sibling indec" + this.transform.GetSiblingIndex() + "   note value:" + notevalue);
        getNoteValue(this.transform.GetSiblingIndex());
        intervalText.text = notevalue.ToString();
        
        fretnum = this.transform.GetSiblingIndex() % 7;  //the 7 frets are numbered from 0 to 6
        

    }
    /*

    private void initialize_notevalue_dict()
    {
        

        for (int i = 0; i < 42; i++)
        {
            notevalue_dict[i] = i % 12;  // Looping from 0 to 11
        }
    }
    */

    private void getNoteValue(int sibling_index)
    {
        int transposed_mathematical_value; //value after transposing, includes negative numbers

        transposed_mathematical_value = notevalue_dict[this.transform.GetSiblingIndex()] + transposed_notes_dict[stringnum];

        //turn the negative numbers into positive number mapped to the corresponding note
        while (transposed_mathematical_value<0)
        {
            transposed_mathematical_value = 11 + transposed_mathematical_value;
        }


        notevalue = (notevalue_dict[this.transform.GetSiblingIndex()] + transposed_notes_dict[stringnum]) % 12;



    }

  

    private void buttonselected()
    {
        // Debug.Log("HIYA BITCH!");
        this.isSelected = !this.isSelected;
        scene = SceneManager.GetActiveScene();
       // if (scene.buildIndex == 3)
         if(scene.name=="challenge_mode")
        QuizManager.instance.SelectedButton(this);
        //else if (scene.buildIndex == 1)
        else if(scene.name=="learn_mode")
        {
            //Debug.Log("HIYA BITCH! -1");
            learnmode.instance.SelectedButton_learnmode(this);
        }
        
       // Debug.Log(this.stringnum + "   " + this.fretnum);
    }

    protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
    {
        
        base.DoStateTransition(state, instant);
        //base.Reset();
        //this.gameObject.GetComponent<intervalbutton>().enabled = false;
        //this.gameObject.GetComponent<intervalbutton>().enabled = true;

        // this.gameObject.GetComponent<Button>().enabled = true;

        //this.gameObject.SetActive(true);
        //Debug.Log(this.currentSelectionState);
        // Do what you need here.
    }

    public void DeselectButton()
    {
        this.DoStateTransition(Selectable.SelectionState.Normal, false);
        //base.DoStateTransition(Selectable.SelectionState.Pressed, true);

        //Debug.Log(this.currentSelectionState);
    }
    /*public void SetWord2(int value)
    {
        if (value == -1)
            wordText.text = "_";
        else
            wordText.text = intervalname[value];

        wordValue2 = value;


    }

    private void WordSelected()
    {
       // QuizManager.instance.SelectedOption(this);
    }
    */
}