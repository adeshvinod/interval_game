using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class intervalbutton : Button
{
    //[SerializeField] private Text wordText;
    [SerializeField] private Text intervalText;

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


    [HideInInspector]

    public int notevalue;
    public int stringnum;  //guitar string
    public int isroot = 0; //flag for root note

    private Button buttonComponent;

    protected  override void Awake()
    {
         buttonComponent = GetComponent<Button>();
         if (buttonComponent)
         {
             buttonComponent.onClick.AddListener(() => buttonselected());
            
         }
        
        notevalue = notevalue_dict[this.transform.GetSiblingIndex()];
        intervalText.text = notevalue.ToString();
        stringnum = this.transform.GetSiblingIndex();// 7;
        

    }

  

    private void buttonselected()
    {
        QuizManager.instance.SelectedButton(this);
    }

    public void DeselectButton()
    {
       // base.DoStateTransition(SelectionState.Pressed, true);
        // Do what you need here.
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