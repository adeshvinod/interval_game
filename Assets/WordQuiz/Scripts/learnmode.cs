using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;


public class learnmode : MonoBehaviour
{
    public static learnmode instance; //Instance to make is available in other scripts without reference
/*
    public intervalbutton[] intervalbuttons_;   //array of objects of class intervalbutton
    [SerializeField] private interval_option[] optionintervalList;    //list of options word in the game
    private GameObject optionintervalList_parent;
    public intervalbutton currentrootnode; //stores an instance of the current Root button
    ColorBlock RootButton = new ColorBlock();
    ColorBlock RevealedButton = new ColorBlock();
    ColorBlock RegularButton = new ColorBlock();
*/
    public prog_button[] progbuttons_;  // Array of prog buttons
    public prog_button currentRootProgButton;  // Current root prog button
    public List<int> selectedIntervals = new List<int>();  // List of intervals to show
    public List<int> darkenIntervals = new List<int>();  // List of intervals to darken in learn mode

    public int[] rootoptions = new int[] { 3, 10, 17, 24, 31, 38 };
    public int root_options_index = 3;

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
            {8, "b6"},
            {9, "M6"},
            {10, "b7"},
            {11, "M7"},
          };

    private Coroutine currentTransitionCoroutine;
    private bool isTransitioning = false;

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
/*        RootButton = ColorBlock.defaultColorBlock;
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
*/
        GameObject progButtonsObject = GameObject.Find("Prog_buttons");
        if (progButtonsObject != null)
        {
            progbuttons_ = progButtonsObject.GetComponentsInChildren<prog_button>();
            //Debug.Log($"Found {progbuttons_.Length} prog buttons");
        }
        else
        {
            Debug.LogWarning("Prog_buttons GameObject not found in scene");
        }

        // Wait 2 seconds before accessing challenge_settings
        Invoke("InitializeChallengeSettings", 1f);

       // setrootbutton(rootoptions[root_options_index]);
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
       

         Invoke("InitializeChallengeSettings", 1f);
      
    }

    private void InitializeChallengeSettings()
    {
      //  if (challenge_settings.instance != null && challenge_settings.instance.questionList != null)
      //  {
            SetSelectedIntervals(challenge_settings.instance.questionList);
            Debug.Log($"Setting intervals from challenge settings: {string.Join(", ", challenge_settings.instance.questionList)}");
            switch(challenge_settings.instance.current_level)
            {
                case challenge_settings.Level.level1:
                    darkenIntervals = new List<int> { };
                    break;
                case challenge_settings.Level.level2:
                    darkenIntervals = challenge_settings.level1_intervals.ToList();
                    break;
                case challenge_settings.Level.level3:
                    darkenIntervals = challenge_settings.level2_intervals.ToList();
                    break;
                case challenge_settings.Level.level4:  
                    darkenIntervals = challenge_settings.level3_intervals.ToList();
                    break;
                case challenge_settings.Level.CUSTOM:
                    darkenIntervals = new List<int> { };
                    break;
            }
            Debug.Log("current level: " + challenge_settings.instance.current_level);
            Debug.Log("darkenIntervals count: " + darkenIntervals.Count + " " + string.Join(", ", darkenIntervals));
      //  }
      //  else
      //  {
      //      Debug.LogWarning("challenge_settings or questionList is null");
      //  }
              
              
         setrootbutton(rootoptions[root_options_index]);
    }

    void setrootbutton(int siblingindex_root, bool Wait=true, bool scalingAnimation=true)
    {
        if (progbuttons_ != null)
        {
            // Stop any existing transition
            if (currentTransitionCoroutine != null)
            {
                StopCoroutine(currentTransitionCoroutine);
            }

            currentRootProgButton = progbuttons_[siblingindex_root];
            
            // Reset all buttons first
            foreach (prog_button progButton in progbuttons_)
            {
                progButton.SetActiveCircle(0,scalingAnimation);
            }

            // Immediately set the root button
            currentRootProgButton.SetActiveCircle(3,scalingAnimation);
            currentRootProgButton.text.text = "R";  // Set root button text to "R"

            // Start new coroutine
            currentTransitionCoroutine = StartCoroutine(DelayedSetOtherButtons(siblingindex_root,Wait,scalingAnimation));
        }
    }

    private IEnumerator DelayedSetOtherButtons(int siblingindex_root, bool Wait, bool scalingAnimation=true)
    {
        isTransitioning = true;
        
        // Wait for 1 second
        if(Wait==true)
        yield return new WaitForSeconds(1f);

        foreach (prog_button progButton in progbuttons_)
        {
            if (progButton != currentRootProgButton)  // Skip the root button
            {
                int interval;
                if (progButton.notevalue >= currentRootProgButton.notevalue)
                    interval = progButton.notevalue - currentRootProgButton.notevalue;
                else
                    interval = 12 - currentRootProgButton.notevalue + progButton.notevalue;

              //  Debug.Log("interval: " + interval + "on prog button: " + progButton.buttonNumber + "selected intervals: " + string.Join(", ", selectedIntervals));

                if (selectedIntervals.Contains(interval))
                {
                    progButton.SetActiveCircle(1,scalingAnimation);
                    progButton.text.text = intervalname[interval];
                }

                if (darkenIntervals.Contains(interval))
                {
                    progButton.SetActiveCircle(2,scalingAnimation);
                    progButton.text.text = intervalname[interval];
                }
            }
        }
      //  Debug.Log("2----darkenIntervals count: " + darkenIntervals.Count + " " + string.Join(", ", darkenIntervals));
        
        isTransitioning = false;
        currentTransitionCoroutine = null;
    }

    public void shiftroot(int direction)
    {
        //deselct all options
  /*      foreach(interval_option optionintervalList_ in optionintervalList)
        {
            optionintervalList_.isSelected = false;
        }
*/
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

    public void SetSelectedIntervals(List<int> intervals)
    {
        selectedIntervals = new List<int>(intervals);
        if (currentRootProgButton != null)
        {
            setrootbutton(rootoptions[root_options_index]);
        }
    }
        
    public void SelectedOption_learnmode(interval_option value)
    {
        if(value.isSelected==true)
        {
            selectedIntervals.Add(value.intervalValue);
        }
        else
        {
            selectedIntervals.Remove(value.intervalValue);
        }
        setrootbutton(rootoptions[root_options_index],false,false);
    }

/*
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
 */   // Update is called once per frame
    void Update()
    {
        
    }
}
