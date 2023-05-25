using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
public class learnmode : MonoBehaviour
{
    public static learnmode instance; //Instance to make is available in other scripts without reference

    public intervalbutton[] intervalbuttons_;   //array of objects of class intervalbutton
    [SerializeField] private interval_option[] optionintervalList;    //list of options word in the game
    private GameObject optionintervalList_parent;
    public intervalbutton currentrootnode; //stores an instance of the current Root button
    ColorBlock RootButton = new ColorBlock();
    ColorBlock RevealedButton = new ColorBlock();
    ColorBlock RegularButton = new ColorBlock();

    public int[] rootoptions = new int[] { 3, 10, 17, 24, 31, 38 };
    public int root_options_index = 0;

    public Dictionary<int, string> intervalname = new Dictionary<int, string>()
         {
            {0, "R"},
            {1, "b2"},
            {2, "M2"},
            {3, "b3"},
            {4, "M3"},
            {5, "P4"},
            {6, "b5"},
            {7, "P5"},
            {8, "m6"},
            {9, "M6"},
            {10, "b7"},
            {11, "M7"},
          };

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
        RootButton = ColorBlock.defaultColorBlock;
        RootButton.normalColor = new Color(1, 0, 0, 1);

        RevealedButton = ColorBlock.defaultColorBlock;
        RevealedButton.normalColor = new Color(0, 1, 0, 1);
        RevealedButton.pressedColor = new Color(0, 1, 0, 1);
        RevealedButton.selectedColor = new Color(0, 1, 0, 1);


        RegularButton = ColorBlock.defaultColorBlock;
        RegularButton.normalColor = new Color(0, 0, 1, 0);
        RegularButton.selectedColor= new Color(0, 0, 1, 0);



        GameObject originalGameObject = GameObject.Find("IntervalButtons");
        intervalbuttons_ = originalGameObject.GetComponentsInChildren<intervalbutton>();

        optionintervalList_parent = GameObject.Find("option_buttons");
        optionintervalList = GameObject.Find("option_buttons").GetComponentsInChildren<interval_option>();
        for (int k = 0; k < optionintervalList.Length; k++)
        {
            optionintervalList[k].SetValue(k);
        }

        setrootbutton(rootoptions[root_options_index]);
    }

    void setrootbutton(int siblingindex_root)
    {
        currentrootnode = intervalbuttons_[siblingindex_root];
        foreach (intervalbutton intervalbutton_ in intervalbuttons_)
        {
            intervalbutton_.isSelected = false;
            intervalbutton_.interactable = false; //basically to reset the button from selected state to normal state, we will reactive the interactability at the end of this iteration

            if (intervalbutton_.transform.GetSiblingIndex() == siblingindex_root)
            {
                intervalbutton_.isroot = 1;
                intervalbutton_.colors = RootButton;
                intervalbutton_.intervalText.text = intervalname[0];
                // currentrootnode = intervalbutton_;
            }
            else
            {
                intervalbutton_.isroot = 0;
                intervalbutton_.colors = RegularButton;
                if (intervalbutton_.notevalue >= currentrootnode.notevalue)
                    intervalbutton_.intervalText.text = intervalname[intervalbutton_.notevalue - currentrootnode.notevalue];
                else
                    intervalbutton_.intervalText.text = intervalname[12 - currentrootnode.notevalue + intervalbutton_.notevalue];
            }
            intervalbutton_.interactable = true;
        }

    }

    public void shiftroot(int direction)
    {
        //deselct all options
        foreach(interval_option optionintervalList_ in optionintervalList)
        {
            optionintervalList_.isSelected = false;
        }

        if(direction==1)
        {
            root_options_index = (root_options_index + 1)%6;
            Debug.Log("Shifted up");
        }
        else if(direction==-1)
        {
            if (root_options_index > 0)
                root_options_index = root_options_index - 1;
            else
                root_options_index = 5;
        }
        setrootbutton(rootoptions[root_options_index]);
    }
    public void SelectedButton_learnmode(intervalbutton value)
    {
        StartCoroutine(Selectedinterval(value));
        Debug.Log("HIYA BITCH!");
    }

    IEnumerator Selectedinterval(intervalbutton value)
    {
        value.colors = RevealedButton;
        yield return new WaitForSeconds(3f);
        value.colors = RegularButton;

    }

    public void SelectedOption_learnmode(interval_option value)
    {
        foreach (intervalbutton intervalbutton_ in intervalbuttons_)
        {
            if((intervalbutton_.notevalue-currentrootnode.notevalue==value.intervalValue)||(12-currentrootnode.notevalue+intervalbutton_.notevalue==value.intervalValue))
            {
               if(value.isSelected==true)
                intervalbutton_.colors = RevealedButton;
               else
                intervalbutton_.colors = RegularButton;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
