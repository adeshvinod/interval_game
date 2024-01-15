using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

//script for describing each interval button on fretboard


public class note_button : Button
{
    //[SerializeField] private Text;
    [SerializeField] public Text noteText;
    [SerializeField] public int bio;
    public Scene scene;

    public int x_coord=0;
    public int y_coord=0;
    public int note_siblingindex;
    public int stringnum;

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
         {11,"G#" }
         

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
         {11,"Ab" }


     };

    

    Dictionary<int, int> transposed_notes_dict = new Dictionary<int, int>() //for alternate tunings
           {
            {0, 0},
            {1, 0},
            {2, 0},
            {3, 0},
            {4, 0},
            {5, 0}

           };
    


    public Button buttonComponent;
    public int notevalue;
    string notename_sharp;
    string notename_flat;
    public bool selectedRegion = false; //to check if the button is in the user selected region of the fretboard defined in settings 
    // Start is called before the first frame update
    protected override void Awake()
    {

        buttonComponent = GetComponent<note_button>();
        if (buttonComponent)
        {
            buttonComponent.onClick.AddListener(() => buttonselected());

        }

        stringnum = this.transform.GetSiblingIndex() / 13; //there are 13 nodes on 1 string      


        noteText = GetComponentInChildren<Text>();
        getNoteValue(this.transform.GetSiblingIndex());
        //notevalue = notevalue_dict[this.transform.GetSiblingIndex()];
        notename_sharp = notename_sharps[notevalue];
        notename_flat = notename_flats[notevalue];
        note_siblingindex = this.transform.GetSiblingIndex();


       //   noteText.text = notename_sharp;
        noteText.text = note_siblingindex.ToString()+"/"+notevalue.ToString();
        noteText.color = new Color(noteText.color.r, noteText.color.g, noteText.color.b, 1);

        scene = SceneManager.GetActiveScene();

        
        
            this.x_coord = this.transform.GetSiblingIndex() % 13;
            this.y_coord = this.transform.GetSiblingIndex() / 13;
        

    }

    private void getNoteValue(int sibling_index)
    {

        if (global_settings.instance.transposed_notes_dict != null)  //to check if we entered global settings and created an instance of the modified  global transposed array. if not we just gonna use the transposed arrays created in this script
        {
            transposed_notes_dict = global_settings.instance.transposed_notes_dict;

            //Debug.Log("transposed value array: " + global_settings.instance.transposed_notes_dict[0] + " " + global_settings.instance.transposed_notes_dict[1] + " " + global_settings.instance.transposed_notes_dict[2] + " " + global_settings.instance.transposed_notes_dict[3] + " " + global_settings.instance.transposed_notes_dict[4] + " ");
            Debug.Log("transposed value array at get noteval: " + transposed_notes_dict[0] + " " + transposed_notes_dict[1] + " " + transposed_notes_dict[2] + " " + transposed_notes_dict[3] + " " + transposed_notes_dict[4] + " ");
        }

        int transposed_mathematical_value; //value after transposing, includes negative numbers

        transposed_mathematical_value = notevalue_dict[this.transform.GetSiblingIndex()] + transposed_notes_dict[stringnum];

        //turn the negative numbers into positive number mapped to the corresponding note
        while (transposed_mathematical_value < 0)
        {
            transposed_mathematical_value = 11 + transposed_mathematical_value;
        }


        notevalue = (notevalue_dict[this.transform.GetSiblingIndex()] + transposed_notes_dict[stringnum]) % 12;



    }

    public void buttonselected()
    {

        if (scene.name == "notes_challenge")
            note_challenge.instance.SelectedButton(this);
        else if (scene.name == "arpeggio_game")
        {
            arpeggio_manager.instance.Selected_button(this);
           
        }
            noteText.color = new Color(noteText.color.r, noteText.color.g, noteText.color.b, 1);

    }

    protected override void Start()
    {
       if (global_settings.instance.transposed_notes_dict!=null)
        {
            transposed_notes_dict = global_settings.instance.transposed_notes_dict;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
