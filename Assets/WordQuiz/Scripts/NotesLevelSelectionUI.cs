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

        ApplyLevelGating();
    }

    void ApplyLevelGating()
    {
        for (int lvl = 1; lvl <= 4; lvl++)
        {
            var go = GameObject.Find($"lv{lvl}_btn");
            if (go == null) continue;
            var btn = go.GetComponent<Button>();
            if (btn != null) btn.interactable = EngagementManager.IsLevelUnlocked(LearningMode.Notes, lvl);
        }
    }

    public void OnLevelButtonClicked(int level)
    {
        gameData.SelectLevel(level);
        eventManager.SelectNoteLevel(level);
    }
}