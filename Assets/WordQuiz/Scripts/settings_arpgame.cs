using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class settings_arpgame : MonoBehaviour
{
    public static settings_arpgame instance;
    private int dropmenu_note;
    private int dropmenu_chordtype;
    public TMPro.TMP_Dropdown dropval_note;
    public TMPro.TMP_Dropdown dropval_chordtype;

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

    public class Pair<T1, T2>
    {
        public T1 note;
        public T2 chordtype;
    }

   public List<Pair<int, int>> myProgression = new List<Pair<int, int>>();
    public List<int> QuestionList_chordtypes = new List<int>();
    


    // Start is called before the first frame update
    void Start()
    {
        
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
        Pair<int, int> pair1 = new Pair<int, int>();
        pair1.note = dropmenu_note;
        pair1.chordtype = dropmenu_chordtype;

        myProgression.Add(pair1);

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

    
}
