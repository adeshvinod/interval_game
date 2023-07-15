using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;


public class gameover : MonoBehaviour
{
    [SerializeField] private Text highscore_floating;
    [SerializeField] private Text currentscore_floating;
    [SerializeField] private Text reactiontimes_floating;
    [SerializeField] private Text accuracies_floating;

    public int HighScore=0;
    public int currentscore;
    public DateTime currentTime;

    public savedData modifiedData;



    private float[] avg_rxntime=new float[12];
    private string[] avg_accuracy=new string[12];
    // Start is called before the first frame update
    void Start()
    {
     
        currentscore = QuizManager.instance.score;
        //currentscore_floating.text = currentscore.ToString();
        currentTime = DateTime.Now;

        // highscore_floating.text = HighScore.ToString();
        for (int i=0;i<12;i++)
        {
            if (QuizManager.instance.questioncounter[i] == 0)
                continue;
            if (i == QuizManager.instance.intervalquestion_val && QuizManager.instance.time <= 0)
                avg_rxntime[i] = QuizManager.instance.reactiontimes[i] / (QuizManager.instance.questioncounter[i] - 1);
            else
                avg_rxntime[i] = QuizManager.instance.reactiontimes[i] / QuizManager.instance.questioncounter[i];
            avg_accuracy[i] = QuizManager.instance.accuracies[i].ToString() + "/" + QuizManager.instance.questioncounter[i].ToString() ;
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentscore_floating.text = currentscore.ToString();


        highscore_floating.text = HighScore.ToString();

        reactiontimes_floating.text = "rxn times: "+ string.Join(" ", avg_rxntime);
        accuracies_floating.text = "accuracies: " + string.Join(",", avg_accuracy);

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
            switch (challenge_settings.instance.current_level)
      {
         case challenge_settings.Level.level1:  HighScore =
                        
                        Data.l1_intervals_highscore;
                    if (currentscore > HighScore)
                    {
                        HighScore = currentscore;
                        modifiedData.l1_intervals_highscore = currentscore;
                    }
                    break;

         case challenge_settings.Level.level2: HighScore =
                        
                        Data.l2_intervals_highscore;
                    if (currentscore > HighScore)
                    {
                        HighScore = currentscore;
                        modifiedData.l2_intervals_highscore = currentscore;
                    }
                    break;

         case challenge_settings.Level.level3: 

                    HighScore = Data.l3_intervals_highscore;
                    if (currentscore > HighScore)
                    {
                        HighScore = currentscore;
                        modifiedData.l3_intervals_highscore = currentscore;
                    }
                    break;

         case challenge_settings.Level.level4: HighScore =
                        Data.l4_intervals_highscore;
                    if (currentscore > HighScore)
                    {
                        HighScore = currentscore;
                        modifiedData.l4_intervals_highscore = currentscore;
                    }
                    break;

            }

           // HighScore = Data.HighScore;
        }
        /*
        if (currentscore > HighScore)
        {
            HighScore = currentscore;
            modifiedData.HighScore = currentscore;
        }
        Debug.Log(path);*/
    }
}
