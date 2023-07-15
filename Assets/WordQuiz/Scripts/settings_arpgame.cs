using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;



public class settings_arpgame : MonoBehaviour
{
    public static settings_arpgame instance;
    private int dropmenu_note;
    private int dropmenu_chordtype;
    public TMPro.TMP_Dropdown dropval_note;
    public TMPro.TMP_Dropdown dropval_chordtype;
    public TMPro.TMP_Dropdown dropmenu_loaded_progressions;
    public InputField progressionNameInput;



    public GameObject chord_prog_display;
    public GameObject chord_prog_container;
    public TextMeshProUGUI chord_name;
    int chord_count = 0;

    public savedData modifiedData;

    

    public GameObject askRandom_Panel;
    public Dictionary<int, string> chord_name_list = new Dictionary<int, string>()
    {
        {0,"maj" },
        {1,"min" },
        {2,"dim" },
        {3,"aug" },
        {4,"maj7" },
        {5,"min7" },
        {6,"dom7" },
        {7,"min7b5"},
        {8,"majScale" },
        {9,"harmonicMin"},
        {10,"melodicMin" }
    };

    public class Pair
    {
        public int note;
        public int chordtype;
    }

   
    public List<Pair> myProgression = new List<Pair>();  //a list of the chords in the current progression created by user
    public List<int> QuestionList_chordtypes = new List<int>();

    public class chord_Progression
    {
        public List<Pair> myProgression;
        public string name;

        public chord_Progression(List<Pair> _myprogression,string _name)
        {
            myProgression = _myprogression;
            name = _name;

        }

    }

    public List<chord_Progression> List_of_progressions= new List<chord_Progression>(); //a list of all the labelled chord progressions the user saved before




    // Start is called before the first frame update
    void Start()
    {

        progressionNameInput.onEndEdit.AddListener(save_progression);

        

         load_data();
         PopulateDropdown();
        Debug.Log("reached  here");
       


    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);


    }
    public void addChord()
    {
        Pair pair1 = new Pair();
        pair1.note = dropmenu_note;
        pair1.chordtype = dropmenu_chordtype;

        myProgression.Add(pair1);

        GameObject newBox = Instantiate(chord_prog_display, chord_prog_container.transform);
        newBox.transform.localPosition = new Vector3(chord_count * 2, 0, 0);

        RectTransform rectTransform = newBox.GetComponent<RectTransform>();

        // Get the size of the new box
        Vector2 size = rectTransform.sizeDelta;
        float width = size.x;
        float height = size.y;

        newBox.transform.localPosition = new Vector3(((chord_count % 4) * width) - 300, (height * (chord_count / 4) * -1) - 70, 0);
        // chord_name.text= dropmenu_note.ToString()+" "+dropval_chordtype.ToString();



        // Access the TextMeshPro component of the new box
        TextMeshProUGUI textMesh = newBox.GetComponentInChildren<TextMeshProUGUI>();

        if (textMesh != null)
        {
            // Access and modify properties of the TextMeshPro component
            textMesh.text = dropval_note.options[dropval_note.value].text.ToString() + " " + dropval_chordtype.options[dropval_chordtype.value].text.ToString();
            textMesh.fontSize = 20;
            // ... other operations with the textMesh
        }



        chord_count++;

    }

    public void reset_chordprogression()
    {
        int childCount = chord_prog_container.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = chord_prog_container.transform.GetChild(i).gameObject;
            // Destroy the child object
            Destroy(child);
        }
        myProgression.Clear();
        chord_count = 0;
    }

    public void askrandom()
    {
        myProgression.Clear();
        
        askRandom_Panel.SetActive(true);

    }

    public void Handle_notes()
    {
        
        dropmenu_note = dropval_note.value;
        Debug.Log("new note is: " + dropmenu_note);
    }



    public void Handle_chordtype()
    {
        dropmenu_chordtype = dropval_chordtype.value;
        Debug.Log("new chord type is: " + dropmenu_chordtype);

    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void load_data()
    {

        string path = Application.persistentDataPath + "/player.fun";

        if (File.Exists(path))
        {
            savedData Data = SaveSystem.Loaddata();
            modifiedData = Data;
            if(Data.savedProgressions!=null)
            List_of_progressions.AddRange(Data.savedProgressions);
            
            
            Pair newPair = new Pair();
            newPair.note = 3;
            newPair.chordtype = 1;

            myProgression.Add(newPair);
            chord_Progression savedProgression = new chord_Progression(myProgression, "hey babe");
            List_of_progressions.Add(savedProgression);

        }
     else
        {
            Debug.Log("file not found");
        }


        
        Debug.Log(path);
    }

    private void PopulateDropdown()
    {
        dropmenu_loaded_progressions.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        if (List_of_progressions != null)
        {
            foreach (chord_Progression chord_prog_temp in List_of_progressions)
            {
                options.Add(new TMP_Dropdown.OptionData(chord_prog_temp.name));
            }

            dropmenu_loaded_progressions.AddOptions(options);
        }
        else
            Debug.Log("list is null");
     }

    public void save_progression(string value)
    {

        if (string.IsNullOrEmpty(value))
        {
            // Display an error message in the debug console
            Debug.LogError("Input field is empty!");

            // Clear the input field
            progressionNameInput.text = string.Empty;
        }
        else
        {
            // Handle input submission

            chord_Progression savedProgression = new chord_Progression(myProgression, value);
            List_of_progressions.Add(savedProgression);
            //Debug.Log(savedProgression.name);
            if(modifiedData.savedProgressions==null)
            {
                Debug.Log("dest list is null");
            }
            else
            modifiedData.savedProgressions.AddRange(List_of_progressions);
            
            SaveSystem.SavePlayer(modifiedData);


            // Clear the input field
            progressionNameInput.text = string.Empty;
        }
    }

}
