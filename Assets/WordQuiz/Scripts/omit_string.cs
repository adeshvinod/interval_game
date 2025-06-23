using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class omit_string : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IntervalsGameData intervalsGameData;
    [SerializeField] private Button button;
    
    [Header("UI Images")]
    [SerializeField] private GameObject firstImage;  // First child image
    [SerializeField] private GameObject secondImage; // Second child image
    
    [Header("String Configuration")]
    
    
    private bool isStringOmitted = false; // Track the current state
    
    void Start()
    {
        // Get the Button component if not assigned
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        
        // Get the child images if not assigned
        if (firstImage == null || secondImage == null)
        {
            Transform[] children = GetComponentsInChildren<Transform>();
            if (children.Length >= 3) // Parent + 2 children
            {
                firstImage = children[1].gameObject;  // First child
                secondImage = children[2].gameObject; // Second child
            }
            else
            {
                Debug.LogError("omit_string: Not enough child objects found. Expected 2 UI images.");
            }
        }
        
       isStringOmitted=false;
        
        // Initialize UI state
        UpdateUIState();
    }

    void initialize_omitString()
    { if (firstImage == null || secondImage == null)
        {
            Transform[] children = GetComponentsInChildren<Transform>();
            if (children.Length >= 3) // Parent + 2 children
            {
                firstImage = children[1].gameObject;  // First child
                secondImage = children[2].gameObject; // Second child
            }
            else
            {
                Debug.LogError("omit_string: Not enough child objects found. Expected 2 UI images.");
            }
        }
        isStringOmitted=false;
        UpdateUIState();

    }
    
    public void OnButtonClicked(int stringNum)
    {
        // Toggle the string selection
        isStringOmitted = !isStringOmitted;
        
        // Call the IntervalsGameData method
        if (intervalsGameData != null)
        {
            intervalsGameData.ToggleIntervalString(stringNum);
            Debug.Log($"Toggled string {stringNum}. Selected: {isStringOmitted}");
        }
        else
        {
            Debug.LogError("omit_string: IntervalsGameData reference not assigned!");
        }
        
        // Update the UI
        UpdateUIState();
    }
    
    void UpdateUIState()
    {
        if (firstImage != null && secondImage != null)
        {
            firstImage.SetActive(!isStringOmitted);  // Show first image when not selected
            secondImage.SetActive(isStringOmitted);  // Show second image when selected
        }
    }
    
    // Public method to set the string number (can be called from Inspector)
    // Public method to get current state
    public bool IsStringSelected()
    {
        return isStringOmitted;
    }
    
    // Public method to set state externally (useful for initialization)
    public void SetStringSelected(bool selected)
    {
        isStringOmitted = selected;
        UpdateUIState();
    }
    
    void OnDestroy()
    {
    }

      
}
