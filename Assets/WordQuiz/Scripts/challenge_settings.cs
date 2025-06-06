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

    public static int[] level1_intervals = new int[] {0,7};
    public static int[] level2_intervals = new int[] { 0, 7,3,4 };
    public static int[] level3_intervals = new int[] { 0, 7,3,4,1,2,10,11 };
    public static int[] level4_intervals = new int[] { 0, 7, 3, 4, 1, 2, 10, 11,5,6,8,9};
    public Level current_level=Level.CUSTOM;

    private int[] allstring_array = new int[] { 0, 1, 2, 3, 4, 5 };

    //making it a singleton, so that other scripts can access the interval question list
    private void Awake()
    {
        Debug.Log("Awake() called on challenge_settings");
        if (instance == null)
        {
            Debug.Log("Creating new instance of challenge_settings");
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            // Initialize scene here as well
            scene = SceneManager.GetActiveScene();
            Debug.Log("Awake - Current scene name: " + scene.name);
        }
        else
        {
            Debug.Log("Destroying duplicate instance of challenge_settings");
            Destroy(this.gameObject);
        }
    }

    void OnEnable()
    {
        Debug.Log("OnEnable called on challenge_settings");
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        Debug.Log("OnDisable called on challenge_settings");
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + loadedScene.name);
        scene = loadedScene;
        
        // Find the Text components again when the scene loads
        if (scene.name == "Pre_challengemode")
        {
            questionListFloating = GameObject.Find("questionListFloating")?.GetComponent<Text>();
            stringListfloating = GameObject.Find("stringListfloating")?.GetComponent<Text>();
            
            if (questionListFloating == null || stringListfloating == null)
            {
                Debug.LogError("Could not find Text components in Pre_challengemode scene!");
            }
            else
            {
                Debug.Log("Successfully found Text components in new scene");
            }

            Debug.Log("Start - Entering Pre_challengemode block");
            optionintervalList_parent = GameObject.Find("interval_button");
            if (optionintervalList_parent == null)
            {
                Debug.LogError("Could not find interval_button GameObject!");
                return;
            }
            
            optionintervalList = optionintervalList_parent.GetComponentsInChildren<interval_option>();
            Debug.Log($"Found {optionintervalList.Length} interval options");
            
            for (int k = 0; k < optionintervalList.Length; k++)
            {
                optionintervalList[k].SetValue(k);
                Debug.Log($"Set interval option {k} to value {k}");
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Update scene reference
        scene = SceneManager.GetActiveScene();
        Debug.Log($"Start - Current scene name: {scene.name}");
        Debug.Log($"Start - Expected scene name: Pre_challengemode");
        Debug.Log($"Start - Scene name matches: {scene.name == "Pre_challengemode"}");
       
        if(scene.name == "Pre_challengemode")
        {
            Debug.Log("Start - Entering Pre_challengemode block");
            optionintervalList_parent = GameObject.Find("interval_button");
            if (optionintervalList_parent == null)
            {
                Debug.LogError("Could not find interval_button GameObject!");
                return;
            }
            
            optionintervalList = optionintervalList_parent.GetComponentsInChildren<interval_option>();
            Debug.Log($"Found {optionintervalList.Length} interval options");
            
            for (int k = 0; k < optionintervalList.Length; k++)
            {
                optionintervalList[k].SetValue(k);
                Debug.Log($"Set interval option {k} to value {k}");
            }
        }
        else
        {
            Debug.Log($"Start - Skipping Pre_challengemode block because scene name is {scene.name}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update scene reference in Update to ensure it's always current
        scene = SceneManager.GetActiveScene();
        
        if (scene.name == "Pre_challengemode")
        {
            // Check if we need to find the Text components again
            if (questionListFloating == null || stringListfloating == null)
            {
                questionListFloating = GameObject.Find("questionListFloating")?.GetComponent<Text>();
                stringListfloating = GameObject.Find("stringListfloating")?.GetComponent<Text>();
            }

            if (questionListFloating != null && stringListfloating != null)
            {
                string textToDisplay = "SELECTED STRINGS:" + ListToText(stringList);
                Debug.Log("Updating string list text to: " + textToDisplay);
                questionListFloating.text = "SELECTED INTERVALS:" + ListToText(questionList);
                stringListfloating.text = textToDisplay;
            }
        }
        Debug.Log("Update - Current scene name: " + scene.name);
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
        if (stringList.Contains(stringnum))
        {
            stringList.Remove(stringnum);
        }
        else
        {
            stringList.Add(stringnum);
        }
        Debug.Log("String list updated: " + ListToText(stringList));
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
        