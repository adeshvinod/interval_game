using UnityEngine;
using UnityEngine.UI;

public class GameSettingsManager : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private Text[] stringNotesText;
    [SerializeField] private GameObject settingsPanel;

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
    }

    private void OnDestroy()
    {
        if (gameSettings != null)
        {
            gameSettings.OnTuningChanged -= UpdateStringNotesDisplay;
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
            stringNotesText[i].text = gameSettings.GetNoteName(transposedNote);
        }
    }

    public void SetLearningMode(int modeIndex)
    {
        gameSettings.SetLearningMode(modeIndex);
    }
} 