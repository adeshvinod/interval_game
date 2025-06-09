using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class interval_option : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI interval_option_Text;
    Scene scene; //this needs to be in game settings static instance

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

    [HideInInspector]
    public int intervalValue;
    public bool isSelected = false;

    private Button buttonComponent;

    private void Awake()
    {
        buttonComponent = GetComponent<Button>();
        if (buttonComponent)
        {
            buttonComponent.onClick.AddListener(() => optionSelected());
        }
    }

    private void Start()
    {
        // Get the button number from the name
        string buttonName = this.gameObject.name;
        int buttonNumber;
        
        if (buttonName == "option_Button")
        {
            buttonNumber = 0;
        }
        else
        {
            // Extract number from names like "option_Button (1)", "option_Button (2)", etc.
            string numberStr = buttonName.Replace("option_Button (", "").Replace(")", "");
            buttonNumber = int.Parse(numberStr);
        }

        // Set the value based on the button number
        SetValue(buttonNumber);
    }

    private void SetValue(int value)
    {
        if (value == -1)
            interval_option_Text.text = "_";
        else
            interval_option_Text.text = intervalname[value];

        intervalValue = value;
    }

    private void optionSelected()
    {
        this.isSelected = !this.isSelected;
        //QuizManager.instance.SelectedOption(this);
        scene = SceneManager.GetActiveScene();
        if(scene.name=="challenge_mode")
            QuizManager.instance.SelectedOption_guessmode(this);
        else if (scene.name == "learn_mode")  //challenge settings
        {
            if (this.isSelected == true)
            {
                challenge_settings.instance.questionList.Add(intervalValue);
                Debug.Log(intervalValue + "added");
            }
            else
            {     
                challenge_settings.instance.questionList.Remove(intervalValue);
                Debug.Log(intervalValue + "removed");
            }
            learnmode.instance.SelectedOption_learnmode(this);
        }
        else if (scene.name == "pre-arpeggio")
        {
            if (this.isSelected == true)
            {
                settings_arpgame.instance.myCustomChord_intervalList.Add(intervalValue);
                Debug.Log(intervalValue + "added");
            }
            else
            {
                settings_arpgame.instance.myCustomChord_intervalList.Remove(intervalValue);
                Debug.Log(intervalValue + "removed");
            }
        }
    }
}
