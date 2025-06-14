using UnityEngine;
using UnityEngine.UI;

public class NotesLevelSelectionUI : MonoBehaviour
{
    [SerializeField] private NotesGameData gameData;
    [SerializeField] private EventManager eventManager;
    [SerializeField] private GameSettings gameSettings;

    private void Start()
    {
        if (gameData == null)
        {
            Debug.LogError("NotesGameData reference is missing!");
            return;
        }

        if (eventManager == null)
        {
            Debug.LogError("EventManager reference is missing!");
            return;
        }
    }

    public void OnLevelButtonClicked(int level)
    {
        gameData.SelectLevel(level);
        eventManager.SelectNoteLevel(level);
    }
} 