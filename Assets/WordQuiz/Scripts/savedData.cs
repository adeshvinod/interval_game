using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class savedData
{  
    public int HighScore=0;

    public float[] ReactionTimes;
    public float[] Acurracies;
   
   
    public savedData(gameover Gameover_)
    {
        HighScore = Gameover_.HighScore;
    }
    
    //public void LoadgameData()
    //{
        
      //  SaveSystem.Loaddata();
     //   if (QuizManager.instance.gameStatus == GameStatus.Gameover)
       // { if(QuizManager.instance.score > HighScore)
      //      HighScore = QuizManager.instance.score;
          
      //  }
   // }
    
}
