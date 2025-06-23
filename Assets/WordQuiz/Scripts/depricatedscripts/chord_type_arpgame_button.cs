using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class chord_type_arpgame_button : Button
{
    // Start is called before the first frame update
   protected override void Start()
    {

        this.chordtype = this.transform.GetSiblingIndex();
        this.GetComponentInChildren<Text>().text = settings_arpgame.instance.chord_name_list[this.chordtype]+ "  "+this.chordtype.ToString();
        //this.GetComponentInChildren<Text>().text = settings_arpgame.instance.chord_name_list[3];
    }

    private Button buttonComponent;
    public bool isSelected = false;
    public int chordtype;

    protected override void Awake()
    {

        buttonComponent = GetComponent<chord_type_arpgame_button>();
        if (buttonComponent)
        {
            buttonComponent.onClick.AddListener(() => buttonselected());

        }

        // this.GetComponentInChildren<Text>().text = arpeggio_manager.instance.chord_name_list[this.chordtype];
        
    }

    public void buttonselected()
    {
        this.isSelected = !this.isSelected;

        if (this.isSelected == true)
            settings_arpgame.instance.QuestionList_chordtypes.Add(this.chordtype);  //assigns the chordtype value to the list of questions when clicked
        else
            settings_arpgame.instance.QuestionList_chordtypes.Remove(this.chordtype);

    }

    

        // Update is called once per frame
        void Update()
    {
        
    }
}
