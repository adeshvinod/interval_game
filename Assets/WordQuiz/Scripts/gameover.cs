using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class gameover : MonoBehaviour
{
    [SerializeField] private Text highscore_floating;
    [SerializeField] private Text currentscore_floating;

    public int HighScore=0;
    public int currentscore;


    // Start is called before the first frame update
    void Start()
    {
     
        currentscore = QuizManager.instance.score;
        //currentscore_floating.text = currentscore.ToString();
       

       // highscore_floating.text = HighScore.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        currentscore_floating.text = currentscore.ToString();


        highscore_floating.text = HighScore.ToString();
    }

    public void saveGame()
    {
        SaveSystem.SavePlayer(this);
    }

    public void loadGame()
    {

        string path = Application.persistentDataPath + "/player.fun";

        if (File.Exists(path))
        {
            savedData Data = SaveSystem.Loaddata();
            HighScore = Data.HighScore;
        }
            if (currentscore> HighScore)
            HighScore = currentscore;
        Debug.Log(path);
    }
}
