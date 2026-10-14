using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using TMPro;

public class gameover_notes : MonoBehaviour
{
    [SerializeField] private TMP_Text highscore_floating;
    [SerializeField] private TMP_Text currentscore_floating;
    [SerializeField] private GameObject wholefretboard;
    [SerializeField] private GameObject scorecard;
    [SerializeField] private GameObject shadedregion;
    [SerializeField] private NotesGameData gameData;

    [Header("Engagement (optional)")]
    [SerializeField] private TMP_Text levelUp_floating;
    // The ticket's big word: "VICTORY!" when the goal was beaten, else "TRY AGAIN".
    [SerializeField] private TMP_Text outcome_heading;
    // The ticket's small top title: "GAME OVER" reads wrong on a win.
    [SerializeField] private TMP_Text title_heading;
    // The scorecard's "target" column. It shipped with a hardcoded 600;
    // this makes it show the level's real required score.
    [SerializeField] private TMP_Text targetScore_number;
   
    
    public int currentscore;
    public DateTime currentTime;
    public savedData modifiedData;
    private bool isNewHighScore;

    // Start is called before the first frame update
    void Start()
    {
        currentscore = gameData.score;
        currentTime = DateTime.Now;

        loadGame();
        saveGame();

        int level = gameData != null ? (int)gameData.currentLevel + 1 : 0;
        bool passedLevel = EngagementManager.EndSession(LearningMode.Notes, level, currentscore, isNewHighScore);


        if (targetScore_number != null)
        {
            int goal = EngagementManager.ScoreThreshold(LearningMode.Notes, level);
            targetScore_number.text = goal == int.MaxValue ? "-" : goal.ToString();
        }
        if (outcome_heading != null)
            outcome_heading.text = passedLevel ? "VICTORY!" : "TRY AGAIN";
        if (title_heading != null)
            title_heading.text = passedLevel ? "LEVEL CLEARED" : "GAME OVER";

        if (levelUp_floating != null)
        {
            if (EngagementManager.DidSessionAvatarEvolve())
            {
                levelUp_floating.text = $"NEW AVATAR: {EngagementManager.GetAvatarName()}";
            }
            else
            {
                // Keep this short - it sits inside the ticket's coral block.
                int cleared = EngagementManager.CountLevelsCleared();
                levelUp_floating.text =
                    $"{cleared}/{EngagementManager.LevelsClearedOutOf} LEVELS CLEARED";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentscore_floating.text = currentscore.ToString();
        highscore_floating.text = gameData.highScore.ToString();
    }

    public void saveGame()
    {
        SaveSystem.SavePlayer(modifiedData);
    }

    public void loadGame()
    {
        string path = Application.persistentDataPath + "/player.fun";

        if (File.Exists(path))
        {
            savedData Data = SaveSystem.Loaddata();
            if (Data == null)
            {
                modifiedData = new savedData();
                return;
            }
            modifiedData = Data;
            
            // Set the high score based on current level
            switch (gameData.currentLevel)
            {
                case NotesGameData.NoteLevel.level1:
                    gameData.highScore = Data.l1_notes_highscore;
                    if (currentscore > Data.l1_notes_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l1_notes_highscore = currentscore;
                        isNewHighScore = true;
                    }
                    break;

                case NotesGameData.NoteLevel.level2:
                    gameData.highScore = Data.l2_notes_highscore;
                    if (currentscore > Data.l2_notes_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l2_notes_highscore = currentscore;
                        isNewHighScore = true;
                    }
                    break;

                case NotesGameData.NoteLevel.level3:
                    gameData.highScore = Data.l3_notes_highscore;
                    if (currentscore > Data.l3_notes_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l3_notes_highscore = currentscore;
                        isNewHighScore = true;
                    }
                    break;

                case NotesGameData.NoteLevel.level4:
                    gameData.highScore = Data.l4_notes_highscore;
                    if (currentscore > Data.l4_notes_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l4_notes_highscore = currentscore;
                        isNewHighScore = true;
                    }
                    break;
            }
        }
    }

    public void ShowFretboard(bool showFretboard)
    {
        if (wholefretboard != null && scorecard != null)
        {
            //wholefretboard.SetActive(showFretboard);
            scorecard.SetActive(!showFretboard);
            shadedregion.SetActive(showFretboard);
        }
        else
        {
            Debug.LogWarning("wholefretboard or scorecard references are missing!");
        }
       // Debug.Log("Fretboard shown: " + showFretboard);
    }

    private void OnEnable()
    {
        ShowFretboard(false);
    }
}

 
