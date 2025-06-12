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
    //[SerializeField] private TMP_Text heading_text_floating;
    //[SerializeField] private TMP_Text subheading_text_floating;
    [SerializeField] private GameObject wholefretboard;
    [SerializeField] private GameObject scorecard;
    [SerializeField] private IntervalsGameData gameData;
    [SerializeField] private GameSettings gameSettings;

    public int currentscore;
    public DateTime currentTime;
    public savedData modifiedData;

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
            modifiedData = Data;
            
            // Set the high score based on current level
            switch (gameSettings.currentIntervalLevel)
            {
                case IntervalLevel.level1:
                    gameData.highScore = Data.l1_intervals_highscore;
                    if (currentscore > Data.l1_intervals_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l1_intervals_highscore = currentscore;
                    }
                    break;

                case IntervalLevel.level2:
                    gameData.highScore = Data.l2_intervals_highscore;
                    if (currentscore > Data.l2_intervals_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l2_intervals_highscore = currentscore;
                    }
                    break;

                case IntervalLevel.level3:
                    gameData.highScore = Data.l3_intervals_highscore;
                    if (currentscore > Data.l3_intervals_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l3_intervals_highscore = currentscore;
                    }
                    break;

                case IntervalLevel.level4:
                    gameData.highScore = Data.l4_intervals_highscore;
                    if (currentscore > Data.l4_intervals_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l4_intervals_highscore = currentscore;
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
