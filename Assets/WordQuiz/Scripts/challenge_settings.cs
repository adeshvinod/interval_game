using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class challenge_settings : MonoBehaviour
{
    public static challenge_settings instance;
    [SerializeField] private interval_option[] optionintervalList;    //list of ALL interval options in the game (R,b2,M2,b3 etc)
    private GameObject optionintervalList_parent;
    [SerializeField] private Text questionListFloating;   //the text which shows the question
    [SerializeField] private Text stringListfloating;

    public List<int> questionList;
    public List<int> stringList;

    //making it a singleton, so that other scripts can access the interval question list
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
        optionintervalList_parent = GameObject.Find("interval_button");
        optionintervalList = GameObject.Find("interval_button").GetComponentsInChildren<interval_option>();
        for (int k = 0; k < optionintervalList.Length; k++)
        {
            optionintervalList[k].SetValue(k);
        }
    }

    // Update is called once per frame
    void Update()
    {
        questionListFloating.text ="SELECTED INTERVALS:"+ ListToText(questionList);
        stringListfloating.text ="SELECTED STRINGS:"+ ListToText(stringList);

    }
    private string ListToText(List<int> list)
    {
        string result = "";
        foreach (var listMember in list)
        {
            result += listMember.ToString() + " ";
        }
        return result;
    }

    public void StringSelect(int stringnum)
    {
       
        foreach(var listmember in stringList)
        {
            if (listmember == stringnum)
            {
                stringList.Remove(stringnum);
                Debug.Log(ListToText(stringList));
                return;
            }


        }
        stringList.Add(stringnum);
        Debug.Log(ListToText(stringList));
    }
}
        