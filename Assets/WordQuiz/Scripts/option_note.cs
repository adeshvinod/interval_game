using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class option_note : MonoBehaviour
{
    [SerializeField] public Text note_option_text;
    

    public Dictionary<int, string> notename = new Dictionary<int, string>()
         {
            {0, "A"},
            {1, "Bb"},
            {2, "B"},
            {3, "C"},
            {4, "Db"},
            {5, "D"},
            {6, "Eb"},
            {7, "E"},
            {8, "F"},
            {9, "Gb"},
            {10, "G"},
            {11, "Ab"},
          };


    [HideInInspector]

    public int noteValue;
    public bool isSelected = false;


    private Button buttonComponent;

    private void Awake()
    {
        buttonComponent = GetComponent<Button>();
        if (buttonComponent)
        {
            buttonComponent.onClick.AddListener(() => optionSelected());
        }
        note_option_text = this.GetComponentInChildren<Text>();

    }

    private void Start()
    {
        this.SetValue(this.transform.GetSiblingIndex());
    }
    public void SetValue(int value)
    {
       
            note_option_text.text = notename[value];

        noteValue = value;


    }

    private void optionSelected()
    {
        this.isSelected = !this.isSelected;
       
            note_challenge.instance.SelectedOption_guessmode(this);
       
    }

}
