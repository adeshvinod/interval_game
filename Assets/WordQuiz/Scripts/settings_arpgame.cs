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
    public TMPro.TMP_Dropdown dropval_chordtype_askrandom; //dropdown menu when user selects random mode, still has the same values as progression chordtypes dropdown
    public TMPro.TMP_Dropdown dropmenu_loaded_progressions;
    public TMPro.TMP_Dropdown progressionDropdown;
    public InputField progressionNameInput;


    public GameObject layout1;  //layout to choose between asking random chords vs asking custom progression
    public GameObject layout2;  //layout when user clicks custom progression, gives choice to create new progression vs use existing progression
    public GameObject layout3;  //layout when user creates new custom progression
    public GameObject layout4;  //layout when user edits old progression- contains just the edit tools
    public GameObject layout5;  //layout when user saves chord progression
    public GameObject layout6; //layout when user selects random chordtypes button
    public GameObject layout7; //layout when user inputs new custom chord type


    public GameObject chord_prog_display;
    public GameObject chord_prog_container;
    public GameObject RandomQuestions_list_container;
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
    public List<int> QuestionList_chordtypes = new List<int>();  //a list of the indexe referenceing the chordtypes you want to be asked, NOT FOR CUSTOM CHORDS
    public List<int> QuestionList_customchords = new List<int>(); //a list of indices of of thecustom chords


    [SerializeField] private interval_option[] optionintervalList;    //list of interval options in the game (R,b2,M2,b3 etc)
    private GameObject optionintervalList_parent;

    public class chord_Progression
    {
        public List<Pair> Progression;
        public string name;

        public chord_Progression(List<Pair> _myprogression,string _name)
        {
            Progression = new List<Pair>();
            Progression.AddRange(_myprogression);
            name = _name;

        }

    }

    public class CustomChordType
    {
        public int chordType_index;  //an int value used to reference this particular chord 
        public List<int> intervals;
        public string name;

        public CustomChordType(List<int> _intervals,string _name)
        {
            intervals = new List<int>();
            intervals.AddRange(_intervals);
            name = _name;
        }
    }

    public List<chord_Progression> List_of_progressions= new List<chord_Progression>(); //a list of all the labelled chord progressions the user saved before
    public int current_index_List_of_progressions=-1;  //index tht passes through list of progressions when you're dealing with editing/deleting/displaying particular progression from list of progressions

    public List<CustomChordType> List_of_CustomChords = new List<CustomChordType>();
    public int current_index_list_of_customchords=-1;
    public CustomChordType myCustomChord;
    public List<int> myCustomChord_intervalList=new List<int>();
    public InputField CustomChordInput;
    public TMPro.TMP_Dropdown customChordDropdown;


    // Start is called before the first frame update
    void Start()
    {
        optionintervalList_parent = GameObject.Find("option_buttons");
        optionintervalList = GameObject.Find("option_buttons").GetComponentsInChildren<interval_option>();
        for (int k = 0; k < optionintervalList.Length; k++)
        {
            optionintervalList[k].SetValue(k);
        }

        layoutSwitcher(1);


        // progressionNameInput.onEndEdit.AddListener(save_progression);

        // Assign an event listener using a lambda expression
     
      
        // Add an empty option to the Dropdown.

   
        progressionDropdown.onValueChanged.AddListener((value) => HandleProgressionDropdown(value));
        customChordDropdown.onValueChanged.AddListener((value) => HandleCCdropdown(value));


        load_data();
         PopulateDropdown();
        Debug.Log("reached  here");
        //myProgression.AddRange(List_of_progressions[0].Progression);



    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);


    }

    public void HandleProgressionDropdown(int value)
    {
        if (value == 0) return;
        layoutSwitcher(3);
        reset_chordprogression();
        current_index_List_of_progressions = value-1;
        myProgression.Clear();
        Debug.Log("im here "+(value-1)+" "+List_of_progressions[value-1].Progression.Count);
        myProgression.AddRange(List_of_progressions[value-1].Progression);
        // Debug.Log("after selecting dropdown " + myProgression[0].chordtype + " " + myProgression[1].chordtype + " "+myProgression[2].chordtype);
        Debug.Log("on clicking drop, listofprog.count " + List_of_progressions.Count);
        chord_name.text = List_of_progressions[current_index_List_of_progressions].name;
        display_progression();
        



    }

    public void HandleCCdropdown(int value)
    {
        if (value == 0) return;
        dropmenu_chordtype = customChordDropdown.value - 1;
        
        QuestionList_customchords.Add(dropmenu_chordtype);

        GameObject newBox = Instantiate(chord_prog_display, RandomQuestions_list_container.transform);
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
            textMesh.text = customChordDropdown.options[customChordDropdown.value].text.ToString();
            textMesh.fontSize = 20;
            // ... other operations with the textMesh
        }



        chord_count++;
    }

    public void display_progression()
    {
        chord_count = 0;
        for (int i = 0; i < myProgression.Count; i++)
        {
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
                textMesh.text = dropval_note.options[myProgression[i].note].text.ToString() + " " + dropval_chordtype.options[myProgression[i].chordtype].text.ToString();
                textMesh.fontSize = 20;
                // ... other operations with the textMesh
            }



            chord_count++;

        }
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

    public void askRandom_AddChordType()
    {
        //this function handles the dropdown menu values in the askRandom chords panel
        dropmenu_chordtype = dropval_chordtype_askrandom.value;
        QuestionList_chordtypes.Add(dropmenu_chordtype);

        GameObject newBox = Instantiate(chord_prog_display, RandomQuestions_list_container.transform);
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
            textMesh.text = dropval_chordtype_askrandom.options[dropval_chordtype_askrandom.value].text.ToString();
            textMesh.fontSize = 20;
            // ... other operations with the textMesh
        }



        chord_count++;



    }

    public void deleteLastChord()
    {
        int index = myProgression.Count - 1;
        myProgression.RemoveAt(index);

        int childCount = chord_prog_container.transform.childCount;
       
            GameObject child = chord_prog_container.transform.GetChild(childCount-1).gameObject;
            // Destroy the child object
            Destroy(child);
        chord_count--;

    }

    public void deleteLastChord_askRandomPanel()
    {
        int index = QuestionList_chordtypes.Count - 1;
        QuestionList_chordtypes.RemoveAt(index);

        int childCount = RandomQuestions_list_container.transform.childCount;

        GameObject child = RandomQuestions_list_container.transform.GetChild(childCount - 1).gameObject;
        // Destroy the child object
        Destroy(child);
        chord_count--;
    }
    public void transpose_progression(int a)
    {
        List<Pair> transposedProgression = new List<Pair>();

        //clear the progression dispay booxes with old values
        int childCount = chord_prog_container.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = chord_prog_container.transform.GetChild(i).gameObject;
            // Destroy the child object
            Destroy(child);
        }

        if (a == 1)
        {
            for (int i = 0; i < myProgression.Count; i++)
            {
                Pair newPair = new Pair();
                newPair.note = (myProgression[i].note + 1) % 12;
                newPair.chordtype = myProgression[i].chordtype;
                transposedProgression.Add(newPair);
            
            }

            myProgression.Clear();
            myProgression = transposedProgression;
            display_progression();


        }
        else
            return;
    }


    public void removeWholeProgression()
    {
        //function to delete the current progression from the list of progressions in saved data database
        List_of_progressions.RemoveAt(current_index_List_of_progressions);
        ConvertProgressionintoSerializable();
        SaveSystem.SavePlayer(modifiedData);
        Debug.Log("after removing list_of_prog.count= " + List_of_progressions.Count);
        reset_chordprogression();
        PopulateDropdown();

    }

    public void saveEditedProgression()
    {
        List_of_progressions[current_index_List_of_progressions].Progression = myProgression;
        ConvertProgressionintoSerializable();
        SaveSystem.SavePlayer(modifiedData);

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
        current_index_List_of_progressions = -1;
        chord_count = 0;   
        progressionDropdown.value = 0;
        QuestionList_chordtypes.Clear();
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
           // savedData Data = SaveSystem.Loaddata();
            modifiedData = SaveSystem.Loaddata();
            //if(Data.savedProgressions!=null)
            // List_of_progressions.AddRange(Data.savedProgressions);

            // if (modifiedData.l2_intervals_highscore == 0)
            //  Debug.Log("data.savedprogchordtypes is not null");
          //  Debug.Log("l3 highscore is :"+modifiedData.savedProgressions_chordtypes[0,0]);
          if(modifiedData.savedProgressions_tonics==null)
            {
                modifiedData.savedProgressions_tonics = new int[15, 40];
                Debug.Log("modifieddata.savedprogressions_tonics was null");
            }
            if (modifiedData.savedProgressions_chordtypes == null)
            {
                modifiedData.savedProgressions_chordtypes = new int[15, 40];
                Debug.Log("modifieddata.savedprogressions_chordtypes was null");
            }
            if (modifiedData.savedProgression_names == null)
            {
                modifiedData.savedProgression_names = new string[15];
                Debug.Log("modifieddata.savedprogressions_names was null");
            }
            if(modifiedData.savedCustomChordTypes == null)
            {
                modifiedData.savedCustomChordTypes = new int[20,12];
                Debug.Log("modifieddata.savedcustomchordtypes was null");
            }
            if(modifiedData.savedCustomChord_names==null)
            {
                modifiedData.savedCustomChord_names = new string[20];
                Debug.Log("modifieddata.savedcustomchordnames was null");
            }
            
            
            Debug.Log("modified data info: " + modifiedData.savedProgressions_chordtypes[0,1]+" "+ modifiedData.savedProgressions_chordtypes[0, 2]+" "+ modifiedData.savedProgressions_chordtypes[1, 1]+" "+ modifiedData.savedProgressions_chordtypes[1, 2]+" "+ modifiedData.savedProgressions_chordtypes[2, 1]+" "+ modifiedData.savedProgressions_chordtypes[2, 2]);

            /*
            Pair newPair = new Pair();
            newPair.note = 3;
            newPair.chordtype = 1;

            myProgression.Add(newPair);
            chord_Progression savedProgression = new chord_Progression(myProgression, "hey babe");
            List_of_progressions.Add(savedProgression);
            */
            ConvertSerializedCustomChordsIntoUsableData();
            ConvertSerializedProgressionToUsableDatatype();  //converts the binary serial data into List_of_progressions data type
           
        }
     else
        {
            Debug.Log("file not found");
        }


        
        Debug.Log(path);
    }

    private void PopulateDropdown()
    {
        //lets just handle custom chordtypes first
        customChordDropdown.ClearOptions();
        customChordDropdown.value = -1;
        List<TMP_Dropdown.OptionData> CC_options = new List<TMP_Dropdown.OptionData>();
        TMP_Dropdown.OptionData CC_placeholderOption = new TMP_Dropdown.OptionData("Choose an option"); //CC stands for custom chord
        customChordDropdown.options.Insert(0, CC_placeholderOption);
        if(List_of_CustomChords!=null)
        {
            foreach(CustomChordType CC_temp in List_of_CustomChords)
            {
                CC_options.Add(new TMP_Dropdown.OptionData(CC_temp.name));
            }
            customChordDropdown.AddOptions(CC_options);
        }
        else
        {
            Debug.Log("Custom chord list os null");

        }

        dropmenu_loaded_progressions.ClearOptions();
        // Debug.Log("im here populatedopdown  " + List_of_progressions[0].myProgression[0].chordtype);
        progressionDropdown.value = -1;
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        // Add an empty option to the Dropdown.

        // Create a new TMP_Dropdown.OptionData instance for the placeholder option.
        TMP_Dropdown.OptionData placeholderOption = new TMP_Dropdown.OptionData("Choose an option");

        // Insert the placeholder option at the beginning of the dropdown options.
        progressionDropdown.options.Insert(0, placeholderOption);
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

    public void save_progression()
    {
        string value = progressionNameInput.text;
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
            Debug.Log("reached save progression");
            ConvertProgressionintoSerializable();
            Debug.Log("hi"+modifiedData.savedProgressions_chordtypes[0, 0]+" "+ modifiedData.savedProgressions_chordtypes[0, 1]+" "+ modifiedData.savedProgressions_chordtypes[0, 2]+" "+ modifiedData.savedProgressions_chordtypes[0, 3] + " " + modifiedData.savedProgressions_chordtypes[0, 4]);

            //Debug.Log(savedProgression.name);
           // if(modifiedData.savedProgressions==null)
           // {
           //     Debug.Log("dest list is null");
          //  }
          //  else
           // modifiedData.savedProgressions.AddRange(List_of_progressions);
            
            SaveSystem.SavePlayer(modifiedData);


            // Clear the input field
            progressionNameInput.text = string.Empty;
            current_index_List_of_progressions = List_of_progressions.Count - 1;   //to allow for deletion of entire progression after saving the progression
            PopulateDropdown();
        }
    }

    public void save_chordType()
    {
        Debug.Log("list of custom chords in savechordtypefunction:" + List_of_CustomChords.Count);
       // myCustomChord_intervalList.Clear();
        // QuestionList_chordtypes.Add(myCustomChord.intervals);
        string value = CustomChordInput.text;
        if (string.IsNullOrEmpty(value))
        {
            // Display an error message in the debug console
            Debug.LogError("Input field is empty!");

            // Clear the input field
            CustomChordInput.text = string.Empty;
        }
        else
        {
            CustomChordType savedChordType = new CustomChordType(myCustomChord_intervalList, value);
            List_of_CustomChords.Add(savedChordType);
            ConvertCustomChordtypeToSerializable();


            SaveSystem.SavePlayer(modifiedData);
            current_index_list_of_customchords = List_of_CustomChords.Count - 1;
            PopulateDropdown();

         
        }
           // QuestionList_customchords.Add(0);
        //myCustomChord = new CustomChordType(myCustomChord_intervalList, "customchord");

    }
    public void ConvertCustomChordtypeToSerializable()
    {
        Debug.Log("reached convertSerCUSTOMCHORD 0");
        int rows = modifiedData.savedCustomChordTypes.GetLength(0);
        int columns = modifiedData.savedCustomChordTypes.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                modifiedData.savedCustomChordTypes[i, j] = -1;
                modifiedData.savedCustomChord_names[i] = null;
            }
        }
        Debug.Log("reached convertcustomchordseriazable!  listofcustomchords" + List_of_CustomChords[0].intervals.Count);
        for(int i=0;i<List_of_CustomChords.Count;i++)
        {
            for(int j=0;j<List_of_CustomChords[i].intervals.Count;j++)
            {
                modifiedData.savedCustomChordTypes[i, j] = List_of_CustomChords[i].intervals[j];
                modifiedData.savedCustomChord_names[i] = List_of_CustomChords[i].name;
            }

        }
        Debug.Log("convertcustomchordtoseriazable modifieddata.savedcustomchordtype=" + modifiedData.savedCustomChordTypes[0,1]);
     
    }
    public void ConvertProgressionintoSerializable()
    {

        //reset the modified datavalues before reassigning them
        int rows = modifiedData.savedProgressions_tonics.GetLength(0);
        int columns = modifiedData.savedProgressions_tonics.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                modifiedData.savedProgressions_tonics[i, j] = 0;
                modifiedData.savedProgressions_chordtypes[i, j] = 0;
                modifiedData.savedProgression_names[i] = null;
            }
        }

        Debug.Log("reached convertprogressionintoserializable and list of progression");
        int no_of_progressions = List_of_progressions.Count;
        for(int i=0;i<no_of_progressions;i++)
        {
            Debug.Log("reached convertprogressionintoserializable 2");
            modifiedData.savedProgressions_chordtypes[i, 0] = List_of_progressions[i].Progression.Count;
            modifiedData.savedProgressions_tonics[i, 0] = List_of_progressions[i].Progression.Count;
            Debug.Log("reached convertprogressionintoserializable 3");
            for (int j=1;j<= List_of_progressions[i].Progression.Count;j++)
            {
                Debug.Log("reached convertprogressionintoserializable 4");
                modifiedData.savedProgressions_tonics[i, j] = List_of_progressions[i].Progression[j-1].note;
                modifiedData.savedProgressions_chordtypes[i, j] = List_of_progressions[i].Progression[j-1].chordtype;
                modifiedData.savedProgression_names[i] = List_of_progressions[i].name;
            }
            Debug.Log("reached convertprogressionintoserializable 5");
        }
        Debug.Log("reached convertprogressionintoserializable 6");
    }

    public void ConvertSerializedCustomChordsIntoUsableData()
    {
        Debug.Log(" reached convSerCUSTOMCHORD 0)"+ modifiedData.savedCustomChordTypes.GetLength(1)+" "+ modifiedData.savedCustomChordTypes[0,0]);
        for(int i=0;i<20;i++)
        {
            myCustomChord_intervalList.Clear();
            for(int j=0;j<modifiedData.savedCustomChordTypes.GetLength(1);j++)
            {
                int temp= modifiedData.savedCustomChordTypes[i,j];
                if (temp != -1)
                {
                    myCustomChord_intervalList.Add(temp);
                    Debug.Log(temp + "added");
                }
                else
                    break;
            }
           
            if (myCustomChord_intervalList.Count!=0)
            {
                Debug.Log("reached convertserCUSTOMCHORD 2");
                CustomChordType myCC = new CustomChordType(myCustomChord_intervalList, modifiedData.savedCustomChord_names[i]);

                List_of_CustomChords.Add(myCC);
                Debug.Log("first element of listofcustomchords" + List_of_CustomChords[i].intervals[0]+" "+List_of_CustomChords[i].intervals[1]+" "+ List_of_CustomChords[i].intervals[2]);
            }
        }
        myCustomChord_intervalList.Clear();
        Debug.Log("hi reached convertserialCUSTOMCHORD 3");
    }

    public void ConvertSerializedProgressionToUsableDatatype()
    {

        Debug.Log("reached convertserprogUsable() 1");
        for (int i = 0;i< 15;i++)
        {
            myProgression.Clear();
            int progressionLength = modifiedData.savedProgressions_chordtypes[i, 0];
            Debug.Log("reached convertserprogUsable() 2 progressionlength="+progressionLength);
            if (progressionLength == 0)
                break;
            for(int j=1;j<=progressionLength;j++)
            {
                Pair newPair = new Pair();
                newPair.note = modifiedData.savedProgressions_tonics[i, j];
                newPair.chordtype = modifiedData.savedProgressions_chordtypes[i, j];

                myProgression.Add(newPair);
               

            }
            chord_Progression savedProgression = new chord_Progression(myProgression, modifiedData.savedProgression_names[i]);
            List_of_progressions.Add(savedProgression);
        }
        myProgression.Clear();  //in the special case where all 15 menu options are filled, we want to clear out myprogression to make room for user inputed chord progs
        Debug.Log("reached convertserprogUsable() 3");
      //  Debug.Log("List of progressions:"+List_of_progressions.Count+" "+List_of_progressions[0].Progression[0].chordtype);
    }

    public void layoutSwitcher(int layoutNumber)
    {
        layout1.gameObject.SetActive(false);
        layout2.gameObject.SetActive(false);
        layout3.gameObject.SetActive(false);
        layout4.gameObject.SetActive(false);
        layout5.gameObject.SetActive(false);
        layout6.gameObject.SetActive(false);
        layout7.gameObject.SetActive(false);
        
        switch(layoutNumber)
        {
            case 1: layout1.gameObject.SetActive(true);
                break;

            case 2:
                layout2.gameObject.SetActive(true);
                break;

            case 3:
                layout3.gameObject.SetActive(true);
                break;

            case 4:                
                layout3.gameObject.SetActive(true);  //general layout of progression disply
                layout4.gameObject.SetActive(true);  //the edit tools
                break;

            case 5:
                layout5.gameObject.SetActive(true);
                break;

            case 6: layout6.gameObject.SetActive(true);
                break;

            case 7: layout7.gameObject.SetActive(true);
                break;


            default:break;


        }
    }
   
}
