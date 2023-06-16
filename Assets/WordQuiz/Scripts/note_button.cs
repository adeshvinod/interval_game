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

        noteText = GetComponentInChildren<Text>();
        notevalue = notevalue_dict[this.transform.GetSiblingIndex()];
        notename_sharp = notename_sharps[notevalue];
        notename_flat = notename_flats[notevalue];
        
        noteText.text = notename_sharp;
        noteText.color = new Color(noteText.color.r, noteText.color.g, noteText.color.b, 0);

        scene = SceneManager.GetActiveScene();

        
        
            this.x_coord = this.transform.GetSiblingIndex() % 13;
            this.y_coord = this.transform.GetSiblingIndex() / 13;
        

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
