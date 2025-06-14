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
   
    
    public int currentscore;
    public DateTime currentTime;
    public savedData modifiedData;

    // Start is called before the first frame update
    void Start()
    {
        currentscore = gameData.score;
        currentTime = DateTime.Now;

        loadGame();
        saveGame();
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
                    }
                    break;

                case NotesGameData.NoteLevel.level2:
                    gameData.highScore = Data.l2_notes_highscore;
                    if (currentscore > Data.l2_notes_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l2_notes_highscore = currentscore;
                    }
                    break;

                case NotesGameData.NoteLevel.level3:
                    gameData.highScore = Data.l3_notes_highscore;
                    if (currentscore > Data.l3_notes_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l3_notes_highscore = currentscore;
                    }
                    break;

                case NotesGameData.NoteLevel.level4:
                    gameData.highScore = Data.l4_notes_highscore;
                    if (currentscore > Data.l4_notes_highscore)
                    {
                        gameData.UpdateHighScore();
                        modifiedData.l4_notes_highscore = currentscore;
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

 
