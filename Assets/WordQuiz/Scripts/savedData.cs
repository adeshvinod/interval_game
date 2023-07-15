using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class savedData
{
    public int HighScore = 0;

    public float[] ReactionTimes;
    public float[] Acurracies;

    public int l1_intervals_highscore = 0;
    public int l2_intervals_highscore = 0;
    public int l3_intervals_highscore = 0;
    public int l4_intervals_highscore = 0;

    public int l1_notes_highscore = 0;
    public int l2_notes_highscore = 0;
    public int l3_notes_highscore = 0;
    public int l4_notes_highscore = 0;

    public List<settings_arpgame.chord_Progression> savedProgressions;





    public savedData(savedData _modifieddata)
    {
        HighScore = _modifieddata.HighScore;

        l1_intervals_highscore = _modifieddata.l1_intervals_highscore;
        l2_intervals_highscore = _modifieddata.l2_intervals_highscore;
        l3_intervals_highscore = _modifieddata.l3_intervals_highscore;
        l4_intervals_highscore = _modifieddata.l4_intervals_highscore;

        savedProgressions = _modifieddata.savedProgressions;
    }

   

    public void start()
    {

    }
}