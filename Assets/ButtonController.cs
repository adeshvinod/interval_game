using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    private Button button;

    void Start()
    {
        // Get the Button component
        button = GetComponent<Button>();
        
        // Add click event listener
        button.onClick.AddListener(ButtonSelected);
    }

    void ButtonSelected()
    {
        // Empty function for now
    }

    void OnDestroy()
    {
        // Remove the event listener when the object is destroyed
        if (button != null)
        {
            button.onClick.RemoveListener(ButtonSelected);
        }
    }
} 