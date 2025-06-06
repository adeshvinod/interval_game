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
    
    public int HighScore = 0;
    public int currentscore;
    public DateTime currentTime;

    public savedData modifiedData;

    // Start is called before the first frame update
    void Start()
    {
        currentscore = note_challenge.instance.score;
        //currentscore_floating.text = currentscore.ToString();
        currentTime = DateTime.Now;

        loadGame();
        saveGame();
    }

    // Update is called once per frame
    void Update()
    {

        currentscore_floating.text = currentscore.ToString();


        highscore_floating.text = HighScore.ToString();
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
            switch (settings_notechallenge.instance.current_level)
            {
                case 1:
                    HighScore =Data.l1_notes_highscore;
                    if (currentscore > HighScore)
                    {
                        HighScore = currentscore;
                        modifiedData.l1_notes_highscore = currentscore;
                    }
                    break;

                case 2:
                    HighScore =Data.l2_notes_highscore;
                    if (currentscore > HighScore)
                    {
                        HighScore = currentscore;
                        modifiedData.l2_notes_highscore = currentscore;
                    }
                    break;

                case 3:

                    HighScore = Data.l3_notes_highscore;
                    if (currentscore > HighScore)
                    {
                        HighScore = currentscore;
                        modifiedData.l3_notes_highscore = currentscore;
                    }
                    break;

                case 4:
                    HighScore =Data.l4_notes_highscore;
                    if (currentscore > HighScore)
                    {
                        HighScore = currentscore;
                        modifiedData.l4_notes_highscore= currentscore;
                    }
                    break;

            }

            // HighScore = Data.HighScore;
        }
 
    }

    public void ShowFretboard(bool showFretboard)
    {
        if (wholefretboard != null && scorecard != null)
        {
            wholefretboard.SetActive(showFretboard);
            scorecard.SetActive(!showFretboard);
            shadedregion.SetActive(showFretboard);
        }
        else
        {
            Debug.LogWarning("wholefretboard or scorecard references are missing!");
        }
        Debug.Log("Fretboard shown: " + showFretboard);
    }

    private void OnEnable()
    {
        ShowFretboard(false);
    }
}

 
