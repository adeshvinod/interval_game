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
    public int[,] savedProgressions_tonics;   //tonics here does not refer to the key centres but rather just the Root notes of the chords  
    public int[,] savedProgressions_chordtypes;
    public string[] savedProgression_names;
    public int totalProgressions = 0;

    public int[,] savedCustomChordTypes;
    public string[] savedCustomChord_names;
    public int totalCustomChordtypes = 0;


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

        savedCustomChordTypes=_modifieddata.savedCustomChordTypes;
        savedCustomChord_names=_modifieddata.savedCustomChord_names;
        totalCustomChordtypes = _modifieddata.totalCustomChordtypes;
}

    
    public savedData()
    {
        int maximum_progressions = 15;
        int maximum_chords_in_prog = 40;
         savedProgressions_tonics = new int[maximum_progressions, maximum_chords_in_prog];
    savedProgressions_chordtypes = new int[maximum_progressions, maximum_chords_in_prog];
     savedProgression_names = new string[maximum_progressions];
    Debug.Log("saved data constructor called)+ "+savedProgressions_tonics[0,0]);
        
        int maximum_custom_chords = 20;
     savedCustomChordTypes=new int[maximum_custom_chords,12];
    savedCustomChord_names=new string[maximum_custom_chords];

        for(int i=0;i<maximum_progressions;i++)
        {
            for(int j=0;j<maximum_chords_in_prog;j++)
            {
                savedProgressions_chordtypes[i,j] = 0;
                savedProgressions_tonics[i, j] = 0;
                

            }
            savedProgression_names[i] = null;
        }

        for(int i=0;i<maximum_custom_chords;i++)
        {
            for(int j=0;j<12;j++)
            {
                savedCustomChordTypes[i, j] = -1;
            }
            savedCustomChord_names[i] = null;
        }
        
    
}
   

    public void start()
    {

    }
}