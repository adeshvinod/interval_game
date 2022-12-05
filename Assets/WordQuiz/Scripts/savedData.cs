using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class savedData : MonoBehaviour
{
    public int HighScore=0;

    public float[] ReactionTimes;
    public float[] Acurracies;

    public savedData()
    {

        SaveSystem.Loaddata();
        if (QuizManager.instance.gameStatus == GameStatus.Gameover)
        { if(QuizManager.instance.score > HighScore)
            HighScore = QuizManager.instance.score;
          
        }
    }
    
}
