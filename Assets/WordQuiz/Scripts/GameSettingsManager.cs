using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameSettingsManager : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private Text[] stringNotesText;
    [SerializeField] private GameObject settingsPanel;

    [SerializeField] private GameObject AvatarButton; //a button to show the different avatars on home screen
    [SerializeField] private List<GameObject> avatarImages = new List<GameObject>(); // List of child avatar images
    
    private int currentAvatarIndex = 0;

    private void Start()
    {
        if (gameSettings == null)
        {
            Debug.LogError("GameSettings not assigned to GameSettingsManager!");
            return;
        }

        // Subscribe to tuning changes
        gameSettings.OnTuningChanged += UpdateStringNotesDisplay;
        
        // Initial display update
        UpdateStringNotesDisplay();
        
        // Initialize avatar images
        InitializeAvatarImages();
        gameSettings.InitializeFretboard();
    }

    private void OnDestroy()
    {
        if (gameSettings != null)
        {
            gameSettings.OnTuningChanged -= UpdateStringNotesDisplay;
        }
    }

    private void InitializeAvatarImages()
    {
        // Initialize all avatar images to inactive
        foreach (GameObject avatar in avatarImages)
        {
            if (avatar != null)
            {
                avatar.SetActive(false);
            }
        }

        // Show the first avatar by default
        if (avatarImages.Count > 0 && avatarImages[0] != null)
        {
            avatarImages[0].SetActive(true);
        }
    }

    public void ToggleSettingsPanel(bool show)
    {
        settingsPanel.SetActive(show);
        UpdateStringNotesDisplay();
    }

    public void TransposeUp(int stringNumber)
    {
        gameSettings.TransposeUp(stringNumber);
    }

    public void TransposeDown(int stringNumber)
    {
        gameSettings.TransposeDown(stringNumber);
    }

    private void UpdateStringNotesDisplay()
    {
        for (int i = 0; i < 6; i++)
        {
            int transposedNote = gameSettings.GetTransposedNote(i);
            switch (i)
            {
                case 0:
                    stringNotesText[i].text = gameSettings.notenames_highE[transposedNote];
                    break;
                case 1:
                    stringNotesText[i].text = gameSettings.notenames_BString[transposedNote];
                    break;
                case 2:
                    stringNotesText[i].text = gameSettings.notenames_GString[transposedNote];
                    break;
                case 3:
                    stringNotesText[i].text = gameSettings.notenames_DString[transposedNote];
                    break;
                case 4:
                    stringNotesText[i].text = gameSettings.notenames_AString[transposedNote];
                    break;
                case 5:
                    stringNotesText[i].text = gameSettings.notenames_lowEString[transposedNote];
                    break;
                default:
                    stringNotesText[i].text = "Error";
                    break;
            }
        }
    }

    public void SetLearningMode(int modeIndex)
    {
        gameSettings.SetLearningMode(modeIndex);
    }

    public void ShowAvatarButton()
    {
        // Deactivate all avatar images first
        foreach (GameObject avatar in avatarImages)
        {
            if (avatar != null)
            {
                avatar.SetActive(false);
            }
        }

        // Move to next avatar
        currentAvatarIndex = (currentAvatarIndex + 1) % avatarImages.Count;

        // Activate the current avatar
        if (avatarImages[currentAvatarIndex] != null)
        {
            avatarImages[currentAvatarIndex].SetActive(true);
            Debug.Log($"Showing avatar {currentAvatarIndex + 1} of {avatarImages.Count}");
        }
        else
        {
            Debug.LogWarning($"Avatar image at index {currentAvatarIndex} is null");
        }
    }

    // Public method to get current avatar index
    public int GetCurrentAvatarIndex()
    {
        return currentAvatarIndex;
    }

    // Public method to set specific avatar
    public void SetAvatar(int index)
    {
        if (index < 0 || index >= avatarImages.Count)
        {
            Debug.LogWarning($"Invalid avatar index: {index}. Must be between 0 and {avatarImages.Count - 1}");
            return;
        }

        // Deactivate all avatars
        foreach (GameObject avatar in avatarImages)
        {
            if (avatar != null)
            {
                avatar.SetActive(false);
            }
        }

        // Set the new avatar
        currentAvatarIndex = index;
        if (avatarImages[currentAvatarIndex] != null)
        {
            avatarImages[currentAvatarIndex].SetActive(true);
            Debug.Log($"Set avatar to index {currentAvatarIndex}");
        }
    }
} 