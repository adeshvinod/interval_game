using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameSettingsManager : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private Text[] stringNotesText;
    [SerializeField] private GameObject settingsPanel;

    [SerializeField] private GameObject AvatarButton; //a button to show the different avatars on home screen
    [SerializeField] private AvatarGallery avatarGallery;
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

    /// Shows the avatar the player has actually earned. Stage 1 is the fresh
    /// install, and the avatar only advances when the same level has been
    /// cleared in all three modes — see EngagementManager.
    private void InitializeAvatarImages()
    {
        int stage = EngagementManager.GetAvatarStage();   // 1..5
        ShowAvatarForStage(stage);
    }

    private void ShowAvatarForStage(int stage)
    {
        if (avatarImages.Count == 0) return;

        // Clamp, so a missing art slot shows the highest one that exists
        // rather than nothing at all.
        int index = Mathf.Clamp(stage - 1, 0, avatarImages.Count - 1);

        for (int i = 0; i < avatarImages.Count; i++)
            if (avatarImages[i] != null) avatarImages[i].SetActive(i == index);

        currentAvatarIndex = index;

        if (stage - 1 >= avatarImages.Count)
            Debug.LogWarning($"No avatar art for stage {stage} " +
                             $"({EngagementManager.AvatarNameForStage(stage)}); showing index {index}.");
    }

    // The tuning sheet is a modal too: lift its canvas over the menu's other
    // canvases while it is open, and put it back afterwards.
    const int ModalSortingOrder = 150;
    int settingsPreviousOrder = int.MinValue;

    public void ToggleSettingsPanel(bool show)
    {
        settingsPanel.SetActive(show);

        var canvas = settingsPanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (show)
            {
                if (settingsPreviousOrder == int.MinValue) settingsPreviousOrder = canvas.sortingOrder;
                canvas.sortingOrder = ModalSortingOrder;
            }
            else if (settingsPreviousOrder != int.MinValue)
            {
                canvas.sortingOrder = settingsPreviousOrder;
            }
        }

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

    /// Tapping the avatar opens the gallery: every stage side by side, locked
    /// ones as silhouettes, plus the rule for evolving. A one-line toast was
    /// not enough to explain "all three modes".
    public void ShowAvatarButton()
    {
        if (avatarGallery != null) { avatarGallery.Open(); return; }

        // Fallback if the gallery is not wired in this scene.
        string requirement = EngagementManager.NextAvatarRequirement();
        AchievementPopup.Notify(EngagementManager.GetAvatarName(),
            string.IsNullOrEmpty(requirement) ? "Every level cleared!" : requirement);
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
            avatarImages[currentAvatarIndex].SetActive(true);
    }
} 