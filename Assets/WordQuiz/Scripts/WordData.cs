using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WordData : MonoBehaviour
{
    [SerializeField] private Text wordText;

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
    
    public int wordValue2;

   

    private Button buttonComponent;

    private void Awake()
    {
         buttonComponent = GetComponent<Button>();
        if (buttonComponent)
        {
            buttonComponent.onClick.AddListener(() => WordSelected());
        }
       
     }

    private void Start()
    {

    }
    public void SetWord2(int value)
    {
        if(value==-1)
            wordText.text = "_";
        else
            wordText.text = intervalname[value];
        
        wordValue2 = value;

        
    }

    private void WordSelected()
    {
        QuizManager.instance.SelectedOption(this);
    }
    
}
