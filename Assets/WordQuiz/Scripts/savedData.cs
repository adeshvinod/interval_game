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

    // public List<settings_arpgame.chord_Progression> savedProgressions;
    public int[,] savedProgressions_tonics;    
    public int[,] savedProgressions_chordtypes;
    public string[] savedProgression_names;
    public int totalProgressions = 0;




    public savedData(savedData _modifieddata)
    {
        HighScore = _modifieddata.HighScore;

        l1_intervals_highscore = _modifieddata.l1_intervals_highscore;
        l2_intervals_highscore = _modifieddata.l2_intervals_highscore;
        l3_intervals_highscore = _modifieddata.l3_intervals_highscore;
        l4_intervals_highscore = _modifieddata.l4_intervals_highscore;

        l1_notes_highscore = _modifieddata.l1_notes_highscore;
        l2_notes_highscore = _modifieddata.l2_notes_highscore;
        l3_notes_highscore = _modifieddata.l3_notes_highscore;
        l4_notes_highscore = _modifieddata.l4_notes_highscore;




        savedProgressions_tonics = _modifieddata.savedProgressions_tonics;
        savedProgressions_chordtypes = _modifieddata.savedProgressions_chordtypes;
        savedProgression_names = _modifieddata.savedProgression_names;
        totalProgressions = _modifieddata.totalProgressions;
    }

    public savedData()
    {
         savedProgressions_tonics = new int[15, 40];
    savedProgressions_chordtypes = new int[15, 40];
     savedProgression_names = new string[15];
    Debug.Log("saved data constructor called)+ "+savedProgressions_tonics[0,0]);
    }
   

    public void start()
    {

    }
}