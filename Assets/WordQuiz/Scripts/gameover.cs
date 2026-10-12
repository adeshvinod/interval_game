using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class gameover : MonoBehaviour
{
    [SerializeField] private TMP_Text highscore_floating;
    [SerializeField] private TMP_Text currentscore_floating;
    [SerializeField] private TMP_Text reactiontimes_floating;
    [SerializeField] private TMP_Text accuracies_floating;

    [Header("Engagement (optional)")]
    [SerializeField] private TMP_Text levelUp_floating;
    // The ticket's big word: "VICTORY!" when the goal was beaten, else "TRY AGAIN".
    [SerializeField] private TMP_Text outcome_heading;
    // The ticket's small top title: "GAME OVER" reads wrong on a win.
    [SerializeField] private TMP_Text title_heading;
    // The scorecard's "target" column. It shipped with a hardcoded 600;
    // this makes it show the level's real required score.
    [SerializeField] private TMP_Text targetScore_number;
    //[SerializeField] private TMP_Text heading_text_floating;
    //[SerializeField] private TMP_Text subheading_text_floating;
    [SerializeField] private GameObject wholefretboard;
    [SerializeField] private GameObject scorecard;
    [SerializeField] private IntervalsGameData gameData;
    [SerializeField] private GameSettings gameSettings;

    public int currentscore;
    public DateTime currentTime;
    public savedData modifiedData = new savedData();
    private bool isNewHighScore;

    private float[] avg_rxntime = new float[12];
    private string[] avg_accuracy = new string[12];
    // Start is called before the first frame update
    void Start()
    {
        currentscore = gameData.score;
        currentTime = DateTime.Now;

        // Calculate averages
        for (int i = 0; i < 12; i++)
        {
            if (gameData.questioncounter[i] == 0)
                continue;
            if (i == gameData.intervalquestion_val && gameData.timer <= 0)
                avg_rxntime[i] = gameData.reactiontimes[i] / (gameData.questioncounter[i] - 1);
            else
                avg_rxntime[i] = gameData.reactiontimes[i] / gameData.questioncounter[i];
            avg_accuracy[i] = gameData.accuracies[i].ToString() + "/" + gameData.questioncounter[i].ToString();
        }
        
        loadGame();
        saveGame();

        int level = gameData != null ? (int)gameData.currentIntervalLevel + 1 : 0;
        bool passedLevel = EngagementManager.EndSession(LearningMode.Intervals, level, currentscore, isNewHighScore);

        DisplayEngagement(level, passedLevel);
    }

    void DisplayEngagement(int level, bool passedLevel)
    {

        if (targetScore_number != null)
        {
            int goal = EngagementManager.ScoreThreshold(LearningMode.Intervals, level);
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
                // The full explanation goes in the popup, not here.
                var missing = EngagementManager.ModesMissingForNextAvatar();
                levelUp_floating.text = missing.Count == 0
                    ? ""
                    : "NEXT: " + string.Join(" + ", missing).ToUpper();
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        currentscore_floating.text = currentscore.ToString();
        highscore_floating.text = gameData.highScore.ToString();

       // reactiontimes_floating.text = "rxn times: "+ string.Join(" ", avg_rxntime);
     //   accuracies_floating.text = "accuracies: " + string.Join(",", avg_accuracy);
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

            if (gameData == null) return;

            // Set the high score based on current level
            switch (gameData.currentIntervalLevel)
            {
                case IntervalLevel.level1:
                    gameData.highScore = Data.l1_intervals_highscore;
                    if (currentscore > Data.l1_intervals_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l1_intervals_highscore = currentscore;
                        isNewHighScore = true;
                    }
                    break;

                case IntervalLevel.level2:
                    gameData.highScore = Data.l2_intervals_highscore;
                    if (currentscore > Data.l2_intervals_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l2_intervals_highscore = currentscore;
                        isNewHighScore = true;
                    }
                    break;

                case IntervalLevel.level3:
                    gameData.highScore = Data.l3_intervals_highscore;
                    if (currentscore > Data.l3_intervals_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l3_intervals_highscore = currentscore;
                        isNewHighScore = true;
                    }
                    break;

                case IntervalLevel.level4:
                    gameData.highScore = Data.l4_intervals_highscore;
                    if (currentscore > Data.l4_intervals_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l4_intervals_highscore = currentscore;
                        isNewHighScore = true;
                    }
                    break;
            }
        }
    }

    private void OnEnable()
    {
        ShowFretboard(false);
    }

    public void ShowFretboard(bool showFretboard)
    {
        if (wholefretboard != null && scorecard != null)
        {
            wholefretboard.SetActive(showFretboard);
            scorecard.SetActive(!showFretboard);
        
        }
        else
        {
            Debug.LogWarning("wholefretboard or scorecard references are missing!");
        }
            Debug.Log("Fretboard shown: " + showFretboard);
    }
}
