using UnityEngine;
using System;

[CreateAssetMenu(fileName = "IntervalsGameData", menuName = "Game/Intervals Game Data")]
public class IntervalsGameData : ScriptableObject
{
    [Header("Score Data")]
    public int score = 0;
    public int highScore = 0;
    public int lives = 3;
    public float timer = 10f;

    [Header("Level High Scores")]
    public int level1HighScore = 0;
    public int level2HighScore = 0;
    public int level3HighScore = 0;
    public int level4HighScore = 0;

    [Header("Question History")]
    public float[] accuracies = new float[12];
    public float[] reactiontimes = new float[12];
    public int[] questioncounter = new int[12];

    [Header("Detailed History")]
    public float[,] questionHistory_accuracy = new float[6, 42];
    public float[,] questionHistory_rxntimes = new float[6, 42];
    public float[,] questionHistory_Counter = new float[6, 42];

    [Header("Current Question Data")]
    public int currentQuestion_Question_node;
    public int currentQuestion_Answer_node;
    public int intervalquestion_val;

    public void ResetGameData()
    {
        score = 0;
        lives = 3;
        timer = 10f;

        // Reset arrays
        for (int i = 0; i < 12; i++)
        {
            accuracies[i] = 0;
            reactiontimes[i] = 0;
            questioncounter[i] = 0;
        }

        // Reset 2D arrays
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 42; j++)
            {
                questionHistory_accuracy[i, j] = -1;
                questionHistory_rxntimes[i, j] = -1;
                questionHistory_Counter[i, j] = 0;
            }
        }

        // Reset current question data
        currentQuestion_Question_node = 0;
        currentQuestion_Answer_node = 0;
        intervalquestion_val = 0;
    }

    public void UpdateHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
        }
    }

    public void LoadHighScores()
    {
        string path = Application.persistentDataPath + "/player.fun";
        if (System.IO.File.Exists(path))
        {
            savedData data = SaveSystem.Loaddata();
            if (data != null)
            {
                level1HighScore = data.l1_intervals_highscore;
                level2HighScore = data.l2_intervals_highscore;
                level3HighScore = data.l3_intervals_highscore;
                level4HighScore = data.l4_intervals_highscore;
            }
        }
    }
} 