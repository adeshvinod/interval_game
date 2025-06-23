using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class global_settings : MonoBehaviour
{
    public static global_settings instance;
    public static LearningMode currentLearningMode = LearningMode.Intervals;

    public void SetLearningMode(int modeIndex)
    {
        currentLearningMode = (LearningMode)modeIndex;
        Debug.Log("Learning mode set to: " + currentLearningMode);
    }

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
         {11,"Ab" }
     };

   public  Dictionary<int, int> transposed_notes_dict = new Dictionary<int, int>() //for alternate tunings
           {
            {0, 0},
            {1, 0},
            {2, 0},
            {3, 0},
            {4, 0},
            {5, 0}
           };

    private Text[] open_string_notes_text;
    private int [] open_string_notes_values=new int[] {7,2,10,5,0,7};
    private int[] standard_tuning_notes_values = new int[] { 7, 2, 10, 5, 0, 7 }; //this is the base default values with reference to which we will add our transpose calculations
    private Button[] increase_buttons;
    private Button[] decrease_buttons;

    public GameObject settingsPanel;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
    }

    void Start()
    {
       Debug.Log("-4 % 12: " + (-4 % 12));
       
        for (int i = 0; i <= 5; i++)
        {
            open_string_notes_values[i] = (standard_tuning_notes_values[i] + transposed_notes_dict[i])%12;
        }
    }

    public void toggle_settingsPanel(bool show)
    {
        settingsPanel.SetActive(show);

        GameObject originalGameObject = GameObject.Find("string_notes_text");
        open_string_notes_text = originalGameObject.GetComponentsInChildren<Text>();

        for (int i = 0; i <= 5; i++)
        {
            open_string_notes_values[i] = (standard_tuning_notes_values[i] + transposed_notes_dict[i]) % 12;
            open_string_notes_text[i].text = notename_sharps[open_string_notes_values[i]];
        }
    }

    public void transpose_up(int string_num)
    {
        transposed_notes_dict[string_num] = transposed_notes_dict[string_num]+1;

        if (transposed_notes_dict[string_num] > 11)
        {
            transposed_notes_dict[string_num] = 0;
        }

        for (int i = 0; i <= 5; i++)
        {
            open_string_notes_values[i] = (standard_tuning_notes_values[i] + transposed_notes_dict[i])%12;
            open_string_notes_text[i].text = notename_sharps[open_string_notes_values[i]];
        }

        Debug.Log(transposed_notes_dict[string_num]+"     " + open_string_notes_values[string_num]+ notename_sharps[11]);
    }

    public void transpose_down(int string_num)
    {
        transposed_notes_dict[string_num] = transposed_notes_dict[string_num] -1;

        if(transposed_notes_dict[string_num]<0)
        {
           // transposed_notes_dict[string_num] = 11;
        }

        for (int i = 0; i <= 5; i++)
        {
            int usable_transposed_notes_dict=transposed_notes_dict[i]; //this is the value that will be used to calculate the note vvalue text on display
           while(usable_transposed_notes_dict<0)
           {
            usable_transposed_notes_dict=12+usable_transposed_notes_dict;
           }        
           open_string_notes_values[i] = (standard_tuning_notes_values[i] + usable_transposed_notes_dict)%12;
           open_string_notes_text[i].text = notename_sharps[open_string_notes_values[i]];
        }
    }

    void Update()
    {
    }
}
