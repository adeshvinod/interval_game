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
    [SerializeField] private WordData[] optionsWordList;    //list of options word in the game
    private GameObject optionsWordList_parent;
    public intervalbutton currentrootnode; //stores an instance of the current Root button
    ColorBlock RootButton = new ColorBlock();
    ColorBlock RevealedButton = new ColorBlock();
    ColorBlock RegularButton = new ColorBlock();

    public int[] rootoptions = new int[] { 3, 10, 17, 24, 31, 38 };

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



        GameObject originalGameObject = GameObject.Find("IntervalButtons");
        intervalbuttons_ = originalGameObject.GetComponentsInChildren<intervalbutton>();

        optionsWordList_parent = GameObject.Find("option_buttons");
        optionsWordList = GameObject.Find("option_buttons").GetComponentsInChildren<WordData>();
        for (int k = 0; k < optionsWordList.Length; k++)
        {
            optionsWordList[k].SetWord2(k);
        }

        setrootbutton(rootoptions[0]);
    }

    void setrootbutton(int siblingindex_root)
    {
        currentrootnode = intervalbuttons_[siblingindex_root];
        foreach (intervalbutton intervalbutton_ in intervalbuttons_)
        {

            intervalbutton_.interactable = false; //basically to reset the button from selected state to normal state, we will reactive the interactability at the end of this iteration

            if (intervalbutton_.transform.GetSiblingIndex() == siblingindex_root)
            {
                intervalbutton_.isroot = 1;
                intervalbutton_.colors = RootButton;
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

    public void SelectedButton_learnmode(intervalbutton value)
    {
        value.colors = RevealedButton;
        Debug.Log("HIYA BITCH!");
    }

    public void SelectedOption_learnmode(WordData value)
    {
        foreach (intervalbutton intervalbutton_ in intervalbuttons_)
        {
            if((intervalbutton_.notevalue-currentrootnode.notevalue==value.wordValue2)||(12-currentrootnode.notevalue+intervalbutton_.notevalue==value.wordValue2))
            {
                intervalbutton_.colors = RevealedButton;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
