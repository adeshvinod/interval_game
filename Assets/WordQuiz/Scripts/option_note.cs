using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
 

//script for describing the option buttons at the bottom of the fretboard when the user is in 'guess the interval' mode
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
    public Text noteText;
    public Image noteImage;
    public Color selectedColor;
    public Color unselectedColor;
    public Color correctColor;
    public Color wrongColor;
    public Color defaultColor;

    private Button buttonComponent;

    [SerializeField] private EventManager eventManager;

    private void Awake()
    {
        buttonComponent = GetComponent<Button>();
        if (buttonComponent)
        {
            buttonComponent.onClick.AddListener(() => optionSelected());
        }
        note_option_text = this.GetComponentInChildren<Text>();

        if (eventManager == null)
        {
            eventManager = Resources.Load<EventManager>("EventManager");
            if (eventManager == null)
            {
                Debug.LogError("EventManager ScriptableObject not found in Resources folder!");
            }
        }
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
        eventManager.SelectNoteOption(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        noteImage.color = selected ? selectedColor : unselectedColor;
    }

    public void SetCorrect()
    {
        noteImage.color = correctColor;
    }

    public void SetWrong()
    {
        noteImage.color = wrongColor;
    }

    public void Reset()
    {
        isSelected = false;
        noteImage.color = defaultColor;
    }
}
