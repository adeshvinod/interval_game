using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class interval_option : MonoBehaviour
{
    [SerializeField] private Text interval_option_Text;
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
            {8, "m6"},
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

    }
    public void SetValue(int value)
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
        if (scene.buildIndex == 3) //challenge mode
            QuizManager.instance.SelectedOption_guessmode(this);
        else if (scene.buildIndex == 1) //learn mode
            learnmode.instance.SelectedOption_learnmode(this);
        else if (scene.buildIndex == 2)  //challenge settings
        {
            if (this.isSelected == true)
            {
                challenge_settings.instance.questionList.Add(intervalValue);
                Debug.Log(intervalValue + "added");
            }
            else
            {     challenge_settings.instance.questionList.Remove(intervalValue);
                Debug.Log(intervalValue + "removed");
            }

        }
    }

}
