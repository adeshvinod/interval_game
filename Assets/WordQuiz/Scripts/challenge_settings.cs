using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class challenge_settings : MonoBehaviour
{
    Scene scene; //this needs to be in game settings static instance

    public static challenge_settings instance;
    [SerializeField] private interval_option[] optionintervalList;    //list of ALL interval options in the game (R,b2,M2,b3 etc)
    private GameObject optionintervalList_parent;
    [SerializeField] private Text questionListFloating;   //the text which shows the question
    [SerializeField] private Text stringListfloating;

    public List<int> questionList;
    public List<int> stringList;

   private int[] level1_intervals = new int[] {0,7};
    private int[] level2_intervals = new int[] { 0, 7,3,4 };
    private int[] level3_intervals = new int[] { 0, 7,3,4,1,2,10,11 };
    private int[] level4_intervals = new int[] { 0, 7, 3, 4, 1, 2, 10, 11,5,6,8,9};
    public Level current_level=Level.CUSTOM;

    private int[] allstring_array = new int[] { 0, 1, 2, 3, 4, 5 };

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
        scene=SceneManager.GetActiveScene();
       
        //if (scene.buildIndex == 2)  //challenge settings
        if(scene.name=="Pre_challengemode")
        {
            optionintervalList_parent = GameObject.Find("interval_button");
            optionintervalList = GameObject.Find("interval_button").GetComponentsInChildren<interval_option>();
            for (int k = 0; k < optionintervalList.Length; k++)
            {
                optionintervalList[k].SetValue(k);
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (scene.name=="challenge_settings")  //challenge settings
        {
            questionListFloating.text = "SELECTED INTERVALS:" + ListToText(questionList);
            stringListfloating.text = "SELECTED STRINGS:" + ListToText(stringList);
        }
        Debug.Log("scene buld no: " + scene.buildIndex);
    }

    public void level_select(int level)
    {
        questionList.Clear();
        stringList.Clear();
        switch (level)
        {
            case 1:     current_level = Level.level1;
                        questionList.AddRange(level1_intervals);
                         stringList.AddRange(allstring_array);
                        break;

            case 2:
                          current_level = Level.level2;
                        questionList.AddRange(level2_intervals);
                         stringList.AddRange(allstring_array);

                         break;

            case 3:
                         current_level = Level.level3;
                        questionList.AddRange(level3_intervals);
                        stringList.AddRange(allstring_array);

                          break;

            case 4:
                             current_level = Level.level4;
                questionList.AddRange(level4_intervals);
                        stringList.AddRange(allstring_array);

                          break;

            default:   
                        break;
        }
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
        Debug.Log(ListToText(stringList)+"scene index: "+scene.buildIndex);
    }

    public enum Level
    {
        level1,
        level2,
        level3,
        level4,
        CUSTOM
    }
}
        